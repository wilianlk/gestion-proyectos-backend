using DatabasesLib.Interfaces;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Repositories
{
    public interface IAreaRepository : IRepository<Area>
    {
        Task<List<Area>> GetAllOrderedAsync();
    }
}