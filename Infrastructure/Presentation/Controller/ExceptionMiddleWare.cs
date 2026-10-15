using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controller
{
   public class ExceptionMiddleware
   {
       private readonly RequestDelegate _next;
       private readonly ILogger<ExceptionMiddleware> _logger;

       public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
       {
           _next = next;
           _logger = logger;
       }

       public async Task InvokeAsync(HttpContext context)
       {
           try
           {
               await _next(context);
           }
           catch (Exception ex)
           {
               context.Response.StatusCode = ex switch
               {
                   KeyNotFoundException => StatusCodes.Status404NotFound,          // مش موجود
                   UnauthorizedAccessException => StatusCodes.Status403Forbidden,   // مش مسموح
                   InvalidOperationException => StatusCodes.Status400BadRequest,    // طلب غلط
                   _ => StatusCodes.Status500InternalServerError
               };

               if (context.Response.StatusCode == 500)
                   _logger.LogError(ex, "Unhandled exception");

               var message = context.Response.StatusCode == 500 ? "Something went wrong" : ex.Message;
               await context.Response.WriteAsJsonAsync(new { error = message });
           }
       }
   }
 
}
