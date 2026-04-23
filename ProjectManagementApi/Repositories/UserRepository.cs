using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    /// <summary>
    /// Repositorio para la gestión de usuarios en la base de datos
    /// </summary>
    public class UserRepository : InformixBaseRepository<User>, IUserRepository
    {
        private readonly ApplicationContext _context;

        public UserRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Description: Get user by username including role information
        /// Input Parameters: 
        ///     * username (string): Username to search
        /// Output Parameters: User object with Role if found, null otherwise
        /// </summary>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Username == username);
        }

        /// <summary>
        /// Get user by identification including role information
        /// </summary>
        /// <param name="identification">Identification to search</param>
        /// <returns>User object with Role if found, null otherwise</returns>
        public async Task<User?> GetByIdentificationAsync(string identification)
        {
            return await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Identification != null && x.Identification.Trim() == identification);
        }

        /// <summary>
        /// Get all users including role information
        /// </summary>
        /// <returns>List of all users with their associated Role</returns>
        public async Task<IEnumerable<User>> GetAllWithRoleAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }

        /// <summary>
        /// Get user by id including role information
        /// </summary>
        /// <param name="id">Unique identifier of the user to search</param>
        /// <returns>User object with its associated Role if found, otherwise null</returns>
        public async Task<User?> GetByIdWithRoleAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
