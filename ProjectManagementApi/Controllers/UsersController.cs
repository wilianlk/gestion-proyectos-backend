using System.Data;
using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectManagementApi.Context;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils;

namespace ProjectManagementApi.Controllers
{
    /// <summary>
    /// Controlador para la gestión de usuarios del sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            ApplicationContext context,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Method to get all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            try
            {
                var users = await _userRepository.GetAllWithRoleAsync();
                
                var usersDto = users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    LastName = u.LastName,
                    Username = u.Username,
                    Email = u.Email,
                    Identification = u.Identification,
                    RoleName = u.Role?.Name ?? "",
                    RoleId = u.RoleId
                }).ToList();

                return Ok(usersDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo la lista de usuarios");
                return ApiErrorResponse.BadRequest(this, ex, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Method to get user by id
        /// </summary>
        /// <param name="id">User id to search</param>
        /// <returns>User if found, 404 otherwise</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<User>> GetById(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdWithRoleAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = $"User with id '{id}' not found" });
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    Username = user.Username,
                    Email = user.Email,
                    Identification = user.Identification,
                    RoleName = user.Role?.Name ?? "",
                    RoleId = user.RoleId
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo el usuario con id {Id}", id);
                return ApiErrorResponse.BadRequest(this, ex, "Ocurrió un error al procesar la solicitud.");
            }
        }

        /// <summary>
        /// Method to get user options from solicitudes_aprobaciones
        /// </summary>
        /// <param name="search">Optional search term (name, email, identification)</param>
        /// <returns>List of users</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<FunctionalLeadOptionDto>>> GetActiveFunctionalLeads([FromQuery] string? search = null)
        {
            var results = new List<FunctionalLeadOptionDto>();
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
                        TRIM(solicitante_nombre) AS nombre,
                        TRIM(solicitante_email) AS e_mail,
                        TRIM(solicitante_identificacion) AS identificacion
                    FROM solicitudes_aprobaciones
                    WHERE solicitante_nombre IS NOT NULL
                      AND TRIM(solicitante_nombre) <> ''
                      AND solicitante_identificacion IS NOT NULL
                      AND TRIM(solicitante_identificacion) <> ''
                      AND (
                          @search = '' OR
                          UPPER(TRIM(solicitante_nombre)) LIKE '%' || UPPER(@search) || '%' OR
                          UPPER(TRIM(solicitante_email)) LIKE '%' || UPPER(@search) || '%' OR
                          UPPER(TRIM(solicitante_identificacion)) LIKE '%' || UPPER(@search) || '%'
                      )
                    ORDER BY nombre";

                var searchParameter = command.CreateParameter();
                searchParameter.ParameterName = "@search";
                searchParameter.Value = (search ?? string.Empty).Trim();
                command.Parameters.Add(searchParameter);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(new FunctionalLeadOptionDto
                    {
                        Name = reader["nombre"]?.ToString()?.Trim() ?? string.Empty,
                        Email = reader["e_mail"]?.ToString()?.Trim() ?? string.Empty,
                        Identification = reader["identificacion"]?.ToString()?.Trim() ?? string.Empty
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuarios desde solicitudes_aprobaciones");
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

        /// <summary>
        /// Assign role to user by identification
        /// </summary>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> AssignRole([FromBody] AssignUserRoleRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Identification) || request.RoleId <= 0)
            {
                return BadRequest(new { message = "La identificacion y el rol son obligatorios." });
            }

            var identification = request.Identification.Trim();
            var connection = _context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
            {
                await connection.OpenAsync();
            }

            DbTransaction? transaction = null;
            try
            {
                transaction = await connection.BeginTransactionAsync();

                using (var roleExistsCmd = connection.CreateCommand())
                {
                    roleExistsCmd.Transaction = transaction;
                    roleExistsCmd.CommandText = "SELECT COUNT(1) FROM requisiciones_roles WHERE id = @roleId";

                    var roleParam = roleExistsCmd.CreateParameter();
                    roleParam.ParameterName = "@roleId";
                    roleParam.Value = request.RoleId;
                    roleExistsCmd.Parameters.Add(roleParam);

                    var existsResult = await roleExistsCmd.ExecuteScalarAsync();
                    var exists = existsResult != null && Convert.ToInt32(existsResult) > 0;
                    if (!exists)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new { message = "El rol seleccionado no existe." });
                    }
                }

                using (var deleteCmd = connection.CreateCommand())
                {
                    deleteCmd.Transaction = transaction;
                    deleteCmd.CommandText = @"
                        DELETE FROM requisiciones_solicitantes_roles
                        WHERE TRIM(solicitante_identificacion) = @identification";

                    var identificationParam = deleteCmd.CreateParameter();
                    identificationParam.ParameterName = "@identification";
                    identificationParam.Value = identification;
                    deleteCmd.Parameters.Add(identificationParam);

                    await deleteCmd.ExecuteNonQueryAsync();
                }

                using (var insertCmd = connection.CreateCommand())
                {
                    insertCmd.Transaction = transaction;
                    insertCmd.CommandText = @"
                        INSERT INTO requisiciones_solicitantes_roles (solicitante_identificacion, rol_id)
                        VALUES (@identification, @roleId)";

                    var identificationParam = insertCmd.CreateParameter();
                    identificationParam.ParameterName = "@identification";
                    identificationParam.Value = identification;
                    insertCmd.Parameters.Add(identificationParam);

                    var roleParam = insertCmd.CreateParameter();
                    roleParam.ParameterName = "@roleId";
                    roleParam.Value = request.RoleId;
                    insertCmd.Parameters.Add(roleParam);

                    await insertCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Rol asignado correctamente." });
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }

                _logger.LogError(ex, "Error asignando rol a usuario");
                return ApiErrorResponse.BadRequest(this, ex, "Ocurrió un error al asignar el rol.");
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
