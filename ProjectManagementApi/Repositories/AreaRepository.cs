using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    /// <summary>
    /// Repositorio para la gestión de áreas en la base de datos
    /// </summary>
    public class AreaRepository : InformixBaseRepository<Area>, IAreaRepository
    {
        private readonly ApplicationContext _context;

        public AreaRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        /**
        * Description: Get all areas ordered by name
        * Input Parameters: None
        * Output Parameters: List of all areas ordered alphabetically by name
        */
        Task<List<Area>> IAreaRepository.GetAllOrderedAsync()
        {
            return _context.Areas
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}
