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

        /// <summary>
        /// Get user by identification including role information
        /// </summary>
        /// <param name="identification">Identification to search</param>
        /// <returns>User object with Role if found</returns>
        Task<User?> GetByIdentificationAsync(string identification);

        /// <summary>
        /// Get all users including role information
        /// </summary>
        /// <returns>List of all users with their Role</returns>
        Task<IEnumerable<User>> GetAllWithRoleAsync();

        /// <summary>
        /// Get user by id including role information
        /// </summary>
        /// <param name="id">User id to search</param>
        /// <returns>User object with Role if found</returns>
        Task<User?> GetByIdWithRoleAsync(int id);
    }
}
