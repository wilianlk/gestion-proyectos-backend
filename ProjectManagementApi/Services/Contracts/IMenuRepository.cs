using DatabasesLib.Interfaces;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IMenuRepository<T> : IRepository<T> where T : Menu
    {
        public Task<IEnumerable<MenuViewDto>> GetAllMenusAsync();
    }
}