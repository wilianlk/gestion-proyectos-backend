using DatabasesLib.Interfaces;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IDeploymentModelRepository : IRepository<DeploymentModel>
    {
        Task<List<DeploymentModel>> GetAllOrderedAsync();
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameExcludingIdAsync(int id, string name);
        Task<DeploymentModel> CreateAsync(string name);
        Task<DeploymentModel?> UpdateAsync(int id, string name);
        Task<bool> DeleteAsync(int id);
    }
}
