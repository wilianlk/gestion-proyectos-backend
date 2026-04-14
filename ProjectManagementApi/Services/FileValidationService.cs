using Microsoft.Extensions.Options;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils.Helpers;

namespace ProjectManagementApi.Services
{
    /// <summary>
    /// Service for validating uploaded files (extension, content type, and size)
    /// </summary>
    public class FileValidationService : IFileValidationService
    {
        private readonly AppSettings _appSettings;

        public FileValidationService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Description: Validate a file against configured allowed extensions, content types, and max size
        /// Input Parameters: 
        ///     * file (IFormFile): File to validate
        /// Output Parameters: Tuple with (isValid, errorMessage) - returns error message if validation fails
        /// </summary>
        public (bool isValid, string? errorMessage) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "File is empty");
            }

            long maxSizeBytes = _appSettings.FileUpload.MaxSizeMB * 1024 * 1024;
            if (file.Length > maxSizeBytes)
            {
                return (false, $"El tamaño del archivo excede el tamaño máximo permitido de {_appSettings.FileUpload.MaxSizeMB}MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_appSettings.FileUpload.AllowedExtensions.Contains(extension))
            {
                return (false, $"La extensión del archivo '{extension}' no está permitida. Extensiones permitidas: {string.Join(", ", _appSettings.FileUpload.AllowedExtensions)}");
            }

            if (!_appSettings.FileUpload.AllowedContentTypes.Contains(file.ContentType))
            {
                return (false, $"El tipo de contenido del archivo '{file.ContentType}' no está permitido");
            }

            return (true, null);
        }
    }
}