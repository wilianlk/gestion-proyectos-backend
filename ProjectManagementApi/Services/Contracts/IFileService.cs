namespace ProjectManagementApi.Services.Contracts
{
    public interface IFileService
    {
        /// <summary>
        /// Upload a file to the server after validation
        /// </summary>
        Task<string> UploadFileAsync(IFormFile file, int projectId, string section);
        Task<bool> DeleteFileAsync(string filePath);
        string GetFileUrl(string filePath);
    }
}