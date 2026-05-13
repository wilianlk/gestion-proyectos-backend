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
    }
}
