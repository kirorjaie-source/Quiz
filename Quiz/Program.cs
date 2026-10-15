using Domain;
using Domain.Contract;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence.Data;
using Persistence.Repositories;
using Presentation.Controller;
using Services.Abstraction.Contracts;
using Services.Implementation;
using Services.Specifications;
using System.Text;
using System.Text.Json.Serialization;
using IDbInititlazer = Domain.IDbInititlazer;

namespace Quiz
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- Services ---

            builder.Services.AddControllers()
                   .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            builder.Services.AddScoped<IDbInititlazer, DbInititlazer>();
            builder.Services.AddScoped<IUniteOfWork, UniteOfWork>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<IScoringService, ScoringService>();
            builder.Services.AddScoped<IGameService, GameService>();

            builder.Services.AddSignalR(o => o.AddFilter<HubExceptionFilter>());
            builder.Services.AddHostedService<GameTimerService>();

            // 1. إعداد متغيرات JWT مع قيم افتراضية في حال عدم وجودها في appsettings.json
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyHere1234567890KeyMustBeAtLeast32Bytes!";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "QuizApi";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "QuizUsers";

            // 2. إعداد الـ Authentication مع تفعيل كامل الفحوصات
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero // إلغاء فارق التوقيت الافتراضي (5 دقائق) لضمان الدقة
                };
            });

            // 3. إعداد Swagger لدعم JWT
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "أدخل قيمة الـ Token فقط بدون كتابة كلمة Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            builder.Services.AddDbContext<AppDbContext>(op => {
                op.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
                      .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
                      .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

            var app = builder.Build();

            await InitializeDbAsync(app);

            // --- Middleware Pipeline ---

            app.UseMiddleware<ExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<GameHub>("/gamehub");

            app.Run();

            async Task InitializeDbAsync(WebApplication app)
            {
                using var scope = app.Services.CreateScope();
                var dbinitilazer = scope.ServiceProvider.GetRequiredService<IDbInititlazer>();
                await dbinitilazer.InititlazerAsync();
            }
        }
    }
}