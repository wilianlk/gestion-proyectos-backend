using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Context;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Utils;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SponsorsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<SponsorsController> _logger;

        public SponsorsController(
            ApplicationContext context,
            ILogger<SponsorsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<SponsorOptionDto>>> GetAll()
        {
            var results = new List<SponsorOptionDto>();
            var connection = _context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = @"
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
                        END AS area_name,
                        TRIM(aprobador3_nombre) AS responsible_name
                    FROM solicitudes_aprobaciones
                    WHERE aprobador3_cargo IS NOT NULL
                      AND TRIM(aprobador3_cargo) <> ''
                      AND aprobador3_nombre IS NOT NULL
                      AND TRIM(aprobador3_nombre) <> ''
                      AND (
                          TRIM(aprobador3_cargo) LIKE 'VICEPRESIDENTE DE %'
                       OR TRIM(aprobador3_cargo) LIKE 'VICEPRESIDENTE %'
                       OR TRIM(aprobador3_cargo) = 'DIRECTOR TECNICO'
                       OR TRIM(aprobador3_cargo) LIKE 'DIRECTOR TECNICO %'
                       OR TRIM(aprobador3_cargo) LIKE 'DIRECTOR %'
                       OR TRIM(aprobador3_cargo) LIKE '%PRESIDENTE%'
                      )
                    ORDER BY area_name, responsible_name";

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var area = reader["area_name"]?.ToString()?.Trim() ?? string.Empty;
                    var responsibleName = reader["responsible_name"]?.ToString()?.Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(responsibleName))
                    {
                        continue;
                    }

                    results.Add(new SponsorOptionDto
                    {
                        Area = area,
                        ResponsibleName = responsibleName,
                        Value = $"{area} - {responsibleName}"
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo patrocinadores");
                return ApiErrorResponse.BadRequest(this, ex, "Ocurrió un error al procesar la solicitud.");
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
