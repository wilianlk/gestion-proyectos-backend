using ProjectManagementApi.Models;

namespace ProjectManagementApi.Services.Contracts
{
    public interface ISsoAuthService
    {
        Task<User?> ResolveUserByCodeAsync(string code);
    }
}
