using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    /// <summary>
    /// Repositorio para la gestión de roles en la base de datos
    /// </summary>
    public class RoleRepository : InformixBaseRepository<Role>, IRoleRepository
    {
        private readonly ApplicationContext _context;

        public RoleRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }
    }
}