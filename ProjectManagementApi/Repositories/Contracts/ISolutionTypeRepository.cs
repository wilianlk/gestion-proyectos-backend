using DatabasesLib.Interfaces;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface ISolutionTypeRepository : IRepository<SolutionType>
    {
        Task<List<SolutionType>> GetAllOrderedAsync();
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameExcludingIdAsync(int id, string name);
        Task<SolutionType> CreateAsync(string name);
        Task<SolutionType?> UpdateAsync(int id, string name);
        Task<bool> DeleteAsync(int id);
    }
}
