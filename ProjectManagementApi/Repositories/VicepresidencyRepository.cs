using DatabasesLib;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using System.Data;

namespace ProjectManagementApi.Repositories
{
    public class VicepresidencyRepository : InformixBaseRepository<Vicepresidency>, IVicepresidencyRepository
    {
        private readonly ApplicationContext _context;

        public VicepresidencyRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Vicepresidency>> GetAllOrderedAsync()
        {
            const string sql = @"
                SELECT DISTINCT
                    TRIM(aprobador3_nombre) AS vp
                FROM solicitudes_aprobaciones
                WHERE aprobador3_nombre IS NOT NULL
                  AND TRIM(aprobador3_nombre) <> ''
                ORDER BY vp";

            var result = new List<Vicepresidency>();

            await using var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            await using var reader = await command.ExecuteReaderAsync();
            var index = 1;
            while (await reader.ReadAsync())
            {
                var name = reader["vp"]?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                result.Add(new Vicepresidency
                {
                    Id = index++,
                    Name = name
                });
            }

            return result;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Vicepresidencies.AnyAsync(x => x.Name == name);
        }

        public async Task<bool> ExistsByNameExcludingIdAsync(int id, string name)
        {
            return await _context.Vicepresidencies.AnyAsync(x => x.Id != id && x.Name == name);
        }

        public async Task<Vicepresidency> CreateAsync(string name)
        {
            var entity = new Vicepresidency
            {
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vicepresidencies.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Vicepresidency?> UpdateAsync(int id, string name)
        {
            var entity = await _context.Vicepresidencies.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return null;
            }

            entity.Name = name;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Vicepresidencies.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return false;
            }

            _context.Vicepresidencies.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
