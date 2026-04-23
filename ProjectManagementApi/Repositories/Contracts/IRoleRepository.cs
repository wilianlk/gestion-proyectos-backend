using DatabasesLib.Interfaces;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de roles
    /// </summary>
public interface IRoleRepository : IRepository<Role>
{
        /// <summary>
        /// Get role by name
        /// </summary>
        /// <param name="name">Role name</param>
        /// <returns>Role if found</returns>
        Task<Role?> GetByNameAsync(string name);
}
}
