using DatabasesLib.Interfaces;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de usuarios
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Get user by username including role information
        /// </summary>
        /// <param name="username">Username to search</param>
        /// <returns>User object with Role if found</returns>
        public Task<User?> GetByUsernameAsync(string username);
    }
}