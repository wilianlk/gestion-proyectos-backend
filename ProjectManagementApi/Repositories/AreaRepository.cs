using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

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
        async Task<List<Area>> IAreaRepository.GetAllOrderedAsync()
        {
            const string sql = @"
                SELECT DISTINCT
                    CASE
                        WHEN TRIM(aprobador3_cargo) LIKE 'VICEPRESIDENTE DE %'
                            THEN TRIM(SUBSTR(TRIM(aprobador3_cargo), LENGTH('VICEPRESIDENTE DE ')+1))
                        WHEN TRIM(aprobador3_cargo) LIKE 'VICEPRESIDENTE %'
                            THEN TRIM(SUBSTR(TRIM(aprobador3_cargo), LENGTH('VICEPRESIDENTE ')+1))
                        WHEN TRIM(aprobador3_cargo) = 'DIRECTOR TECNICO'
                            THEN 'DIRECCION TECNICA'
                        WHEN TRIM(aprobador3_cargo) LIKE 'DIRECTOR TECNICO %'
                            THEN TRIM(SUBSTR(TRIM(aprobador3_cargo), LENGTH('DIRECTOR TECNICO ')+1))
                        WHEN TRIM(aprobador3_cargo) LIKE 'DIRECTOR %'
                            THEN TRIM(SUBSTR(TRIM(aprobador3_cargo), LENGTH('DIRECTOR ')+1))
                        WHEN TRIM(aprobador3_cargo) LIKE '%PRESIDENTE%'
                            THEN 'PRESIDENCIA'
                        ELSE TRIM(aprobador3_cargo)
                    END AS area_name
                FROM solicitudes_aprobaciones
                WHERE aprobador3_cargo IS NOT NULL
                  AND TRIM(aprobador3_cargo) <> ''
                ORDER BY area_name";

            var result = new List<Area>();

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
                var name = reader["area_name"]?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                result.Add(new Area
                {
                    Id = index++,
                    Name = name
                });
            }

            return result;
        }
    }
}
