using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.Controller.Extension
{
   

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    namespace Quiz.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        [Authorize] 
        public class UploadsController : ControllerBase
        {
            private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            private const long MaxFileSize = 2 * 1024 * 1024; 

            private readonly IWebHostEnvironment _env;
            public UploadsController(IWebHostEnvironment env) => _env = env;

            
            [HttpPost("image")]
            public async Task<IActionResult> UploadImage(IFormFile file)
            {
                if (file is null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                if (file.Length > MaxFileSize)
                    return BadRequest(new { error = "Image must be 2MB or less" });

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension) || !file.ContentType.StartsWith("image/"))
                    return BadRequest(new { error = "Only jpg, png, webp and gif images are allowed" });

                var fileName = $"{Guid.NewGuid()}{extension}";

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                await using (var stream = System.IO.File.Create(Path.Combine(uploadsFolder, fileName)))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
                return Ok(new { url });
            }
        }
    }

    
}
