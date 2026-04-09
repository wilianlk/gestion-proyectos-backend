namespace ProjectManagementApi.Services.Contracts
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, int projectId, string section);
        Task<bool> DeleteFileAsync(string filePath);
        string GetFileUrl(string filePath);
    }
}