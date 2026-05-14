using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Utils;

namespace ProjectManagementApi.Controllers
{
    /// <summary>
    /// Controlador para la gestión de roles del sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<RolesController> _logger;
        private const string DefaultErrorMessage = "Ocurrió un error al procesar la solicitud.";

        public RolesController(
            IRoleRepository roleRepository,
            ILogger<RolesController> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        /// <summary>
        /// Method to get all roles
        /// </summary>
        /// <returns>List of all roles</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Role>>> GetAll()
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo la lista de roles");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        /// <summary>
        /// Method to get role by id
        /// </summary>
        /// <param name="id">Role id to search</param>
        /// <returns>Role if found, 404 otherwise</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Role>> GetById(int id)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = $"Role with id '{id}' not found" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo el rol con id {Id}", id);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Role>> Create([FromBody] CreateRoleRequestDto request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "El nombre del rol es requerido." });
                }

                var existingRole = await _roleRepository.GetByNameAsync(request.Name.Trim());
                if (existingRole != null)
                {
                    return BadRequest(new { message = "Ya existe un rol con ese nombre en GestionProyectos." });
                }

                var roleToCreate = new Role
                {
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    Application = "GestionProyectos"
                };

                var createdRole = await _roleRepository.CreateAsync(roleToCreate);
                return StatusCode(StatusCodes.Status201Created, createdRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando rol");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }
    }
}
