using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;

namespace ProjectManagementApi.Controllers
{
    /// <summary>
    /// Controlador para la gestión de áreas del sistema
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AreasController : ControllerBase
    {
        private readonly IAreaRepository _areaRepository;
        private readonly ILogger<AreasController> _logger;
        private const string DefaultErrorMessage = "Ocurrió un error al procesar la solicitud.";

        public AreasController(
            IAreaRepository areaRepository,
            ILogger<AreasController> logger)
        {
            _areaRepository = areaRepository;
            _logger = logger;
        }

        /// <summary>
        /// Method to get all areas ordered by name
        /// </summary>
        /// <returns>List of all areas</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Area>>> GetAll()
        {
            try
            {
                var areas = await _areaRepository.GetAllOrderedAsync();
                return Ok(areas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo la lista de áreas");
                return BadRequest(new { message = DefaultErrorMessage });
            }
        }
    }
}