using DatabasesLib.Interfaces;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IVicepresidencyRepository : IRepository<Vicepresidency>
    {
        Task<List<Vicepresidency>> GetAllOrderedAsync();
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> ExistsByNameExcludingIdAsync(int id, string name);
        Task<Vicepresidency> CreateAsync(string name);
        Task<Vicepresidency?> UpdateAsync(int id, string name);
        Task<bool> DeleteAsync(int id);
    }
}
