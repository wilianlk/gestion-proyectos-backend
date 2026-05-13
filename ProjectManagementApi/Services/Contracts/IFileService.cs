namespace ProjectManagementApi.Services.Contracts
{
    public interface IFileService
    {
        /// <summary>
        /// Upload a file to the server after validation
        /// </summary>
        Task<string> UploadFileAsync(IFormFile file, int projectId, string section, string projectCode);
        Task<bool> DeleteFileAsync(string filePath);
        string GetFileUrl(string filePath);
        string GetAbsoluteFilePath(string filePath);
        string? TryExtractReferenceCode(string filePath);
    }
}
