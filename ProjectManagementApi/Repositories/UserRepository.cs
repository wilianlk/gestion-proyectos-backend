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
    }
}