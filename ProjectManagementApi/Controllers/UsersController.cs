using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services;
using ProjectManagementApi.Services.Contracts;

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
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
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
                return BadRequest(new { message = "Ocurrió un error al procesar la solicitud." });
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
                return BadRequest(new { message = "Ocurrió un error al procesar la solicitud." });
            }
        }
    }
}