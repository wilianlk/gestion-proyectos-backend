using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenusController : ControllerBase
    {
        private readonly IMenuRepository<Menu> _menuRepository;
        public MenusController(
            IMenuRepository<Menu> menuRepository)
        {
            _menuRepository = menuRepository;
        }

        /// <summary>
        /// Method list all menus
        /// </summary>
        /// <param name="isActive">Menu Status</param>
        /// <returns>Menus list</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Menu>> All([FromQuery] bool? isActive)
        {
            var menus = await _menuRepository.GetAllMenusAsync();
            if (isActive is not null) menus = menus.Where(x => x.IsActive == isActive);

            return Ok(menus);
        }
    }
}