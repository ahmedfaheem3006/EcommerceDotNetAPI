using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ECommerce.API.Helpers;
using ECommerce.BLL.Interfaces;
using ECommerce.Common.Result;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// ImageController: Handles general image uploads
    /// 
    /// Endpoints:
    /// POST /api/image/upload - Upload an image and get its URL (Authorized users)
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : BaseController
    {
        private readonly IImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IImageService imageService, ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        /// <summary>
        /// Upload general image
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(Result<string>.Failure("No file uploaded."));

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(Result<string>.Failure("Invalid file type. Allowed extensions are: .jpg, .jpeg, .png, .gif"));

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _imageService.UploadImageAsync(stream, file.FileName);

                if (result.IsFailure)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image.");
                return BadRequest(Result<string>.Failure($"Image upload failed: {ex.Message}"));
            }
        }
    }
}
