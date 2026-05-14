using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de roles
    /// </summary>
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        Task<Role?> GetByIdAsync(int id);
        Task<Role?> GetByNameAsync(string name);
        Task<Role> CreateAsync(Role role);
    }
}
