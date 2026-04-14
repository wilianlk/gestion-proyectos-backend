using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Services
{
    /// <summary>
    /// Service for file operations (upload, delete, URL generation)
    /// </summary>
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IFileValidationService _validationService;
        private readonly string _uploadPath;

        public FileService(IWebHostEnvironment environment, IFileValidationService validationService)
        {
            _environment = environment;
            _validationService = validationService;
            _uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "project-documents");
            
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        /**
        * Description: Upload a file to the server after validation
        * Input Parameters: 
        *      * file (IFormFile): File to upload
        *      * projectId (int): Project id for folder organization
        *      * section (string): Section name for folder organization
        * Output Parameters: Relative path of the saved file
        */
        public async Task<string> UploadFileAsync(IFormFile file, int projectId, string section)
        {
            var (isValid, errorMessage) = _validationService.ValidateFile(file);
            if (!isValid)
            {
                throw new InvalidOperationException(errorMessage);
            }

            var sectionFolder = SanitizeFolderName(section);
            var projectFolder = Path.Combine(_uploadPath, projectId.ToString());
            var sectionPath = Path.Combine(projectFolder, sectionFolder);
            
            if (!Directory.Exists(sectionPath))
            {
                Directory.CreateDirectory(sectionPath);
            }

            var fileName = $"{Guid.NewGuid()}_{SanitizeFileName(file.FileName)}";
            var filePath = Path.Combine(sectionPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("uploads", "project-documents", projectId.ToString(), sectionFolder, fileName);
            return relativePath.Replace("\\", "/");
        }

        /**
        * Description: Delete a file from the server
        * Input Parameters: 
        *      * filePath (string): Relative path of the file
        * Output Parameters: True if deleted, false otherwise
        */
        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /**
        * Description: Get the full URL for a file
        * Input Parameters: 
        *      * filePath (string): Relative path of the file
        * Output Parameters: Full URL of the file
        */
        public string GetFileUrl(string filePath)
        {
            return $"/{filePath.Replace("\\", "/")}";
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }

        private string SanitizeFolderName(string folderName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", folderName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}