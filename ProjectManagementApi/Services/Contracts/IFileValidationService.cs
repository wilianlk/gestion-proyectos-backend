namespace ProjectManagementApi.Services.Contracts
{
    public interface IFileValidationService
    {
        (bool isValid, string? errorMessage) ValidateFile(IFormFile file);
    }
}