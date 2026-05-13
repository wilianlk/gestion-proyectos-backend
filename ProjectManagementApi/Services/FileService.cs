using System.Text;
using System.Text.RegularExpressions;
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
        private readonly IFileValidationService _validationService;
        private readonly string _uploadsRootPath;
        private readonly string _projectDocumentsPath;

        public FileService(IWebHostEnvironment environment, IFileValidationService validationService)
        {
            _validationService = validationService;
            _uploadsRootPath = Path.Combine(AppContext.BaseDirectory, "uploads");
            _projectDocumentsPath = Path.Combine(_uploadsRootPath, "project-documents");

            Directory.CreateDirectory(_projectDocumentsPath);
        }

        /**
        * Description: Upload a file to the server after validation
        * Input Parameters: 
        *      * file (IFormFile): File to upload
        *      * projectId (int): Project id for folder organization
        *      * section (string): Section name for folder organization
        * Output Parameters: Relative path of the saved file
        */
        public async Task<string> UploadFileAsync(IFormFile file, int projectId, string section, string projectCode)
        {
            var (isValid, errorMessage) = _validationService.ValidateFile(file);
            if (!isValid)
            {
                throw new InvalidOperationException(errorMessage);
            }

            var sectionFolder = SanitizeFolderName(section);
            var projectFolder = Path.Combine(_projectDocumentsPath, projectId.ToString());
            var sectionPath = Path.Combine(projectFolder, sectionFolder);
            
            if (!Directory.Exists(sectionPath))
            {
                Directory.CreateDirectory(sectionPath);
            }

            var referenceCode = BuildReferenceCode(projectCode, section);
            var fileName = $"{referenceCode}__{SanitizeFileName(file.FileName)}";
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
                var fullPath = GetAbsoluteFilePath(filePath);
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
            var normalized = (filePath ?? string.Empty).Replace("\\", "/").TrimStart('/');
            if (!normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = $"uploads/{normalized}";
            }
            return $"/{normalized}";
        }

        public string GetAbsoluteFilePath(string filePath)
        {
            var normalized = (filePath ?? string.Empty).Replace("\\", "/").TrimStart('/');
            if (normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized["uploads/".Length..];
            }

            var relativePath = normalized.Replace("/", Path.DirectorySeparatorChar.ToString());
            return Path.Combine(_uploadsRootPath, relativePath);
        }

        public string? TryExtractReferenceCode(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            var normalized = filePath.Replace("\\", "/");
            var fileName = Path.GetFileName(normalized);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var separatorIndex = fileName.IndexOf("__", StringComparison.Ordinal);
            if (separatorIndex <= 0)
            {
                return null;
            }

            return fileName[..separatorIndex];
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            return string.IsNullOrWhiteSpace(sanitized) ? "archivo" : sanitized.Trim();
        }

        private string SanitizeFolderName(string folderName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", folderName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }

        private static string BuildReferenceCode(string projectCode, string section)
        {
            var projectToken = NormalizeToken(projectCode, 20);
            var sectionToken = MapSectionCode(section);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            var shortGuid = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
            return $"{projectToken}-{sectionToken}-{timestamp}-{shortGuid}";
        }

        private static string MapSectionCode(string section)
        {
            var normalized = NormalizeToken(section, 30);
            return normalized switch
            {
                "ARCHITECTURE" => "ARQ",
                "UXCASES" => "CASUX",
                "REQUIREMENTS" => "REQ",
                _ => string.IsNullOrWhiteSpace(normalized) ? "GEN" : normalized[..Math.Min(normalized.Length, 8)]
            };
        }

        private static string NormalizeToken(string input, int maxLength)
        {
            var value = (input ?? string.Empty).Trim().ToUpperInvariant();
            var ascii = RemoveDiacritics(value);
            var alphanumeric = Regex.Replace(ascii, @"[^A-Z0-9]+", string.Empty);
            if (string.IsNullOrWhiteSpace(alphanumeric))
            {
                return "NA";
            }

            return alphanumeric[..Math.Min(maxLength, alphanumeric.Length)];
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);
            foreach (var character in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
