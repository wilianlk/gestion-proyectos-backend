using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    public class MenuRepository : InformixBaseRepository<Menu>, IMenuRepository<Menu>
    {
        private readonly ApplicationContext _context;

        public MenuRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        /**
        * Description: Menu list
        * Input Parameters: None
        * Output Parameters: List of menus
        */
        public async Task<IEnumerable<MenuViewDto>> GetAllMenusAsync()
        {
            var menus = await _context.Menus.Include(m => m.FirstParent).Include(m => m.SecondParent).Select(x => new MenuViewDto
            {
                Id = x.Id,
                Name = x.Name,
                IconMenu = x.IconMenu,
                IsErrorDetails = x.IsErrorDetails,
                IsNavigate = x.IsNavigate,
                IsParent = false,
                Level = x.Level,
                IsActive = x.IsActive,
                Url = x.Url,
                OriginalUrl = x.Url,
                FirstParent = x.FirstParent != null ? new MenuParentViewDto
                {
                    Id = x.FirstParent.Id,
                    Name = x.FirstParent.Name,
                    Url = x.FirstParent.Url
                } : null,
                SecondParent = x.SecondParent != null ? new MenuParentViewDto
                {
                    Id = x.SecondParent.Id,
                    Name = x.SecondParent.Name,
                    Url = x.SecondParent.Url
                } : null
            }).OrderBy(x => x.Id).ToListAsync();

            foreach (var menu in menus)
            {
                string url = "/";
                if (menu.FirstParent != null) url += $"{menu.FirstParent.Url}/";
                if (menu.SecondParent != null) url += $"{menu.SecondParent.Url}/";

                url += $"{menu.Url}";

                menu.Url = url;

                if (await IsParentAsync(menu.Id))
                {
                    menu.IsParent = true;
                }
            }

            return menus;
        }

        /**
         * Description: Check if menu is parent
         * Input Parameters:
         *      * id (int): id to search
         * Output Parameters: Found menu object
         */
        public async Task<bool> IsParentAsync(int id)
        {
            return await _context.Menus.Where(m => m.FirstParentId == id || m.SecondParentId == id).AnyAsync();
        }
    }
}