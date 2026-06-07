using System;
using System.IO;
using System.Threading.Tasks;
using ECommerce.BLL.Interfaces;
using ECommerce.Common.Result;

namespace ECommerce.BLL.Services
{
    /// <summary>
    /// Image Service: handles image uploading
    /// 
    /// لماذا:
    /// - نحتاج رفع صور المنتجات والتصنيفات
    /// - بنحفظ الصور في مجلد محلي (wwwroot/images) مع اسم فريد
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly string _uploadPath;

        public ImageService(string uploadPath)
        {
            _uploadPath = uploadPath;
        }

        public async Task<Result<string>> UploadImageAsync(Stream fileStream, string fileName)
        {
            try
            {
                if (!Directory.Exists(_uploadPath))
                    Directory.CreateDirectory(_uploadPath);

                // Generate a unique filename to avoid overwriting
                var extension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(_uploadPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileStream.CopyToAsync(stream);
                }

                // Return the relative URL that can be used in API responses
                var url = $"/images/{uniqueFileName}";
                return Result<string>.Success(url, "Image uploaded successfully.");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"Image upload failed: {ex.Message}");
            }
        }
    }
}
