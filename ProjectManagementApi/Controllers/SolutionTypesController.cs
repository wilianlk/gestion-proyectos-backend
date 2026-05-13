using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Utils;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SolutionTypesController : ControllerBase
    {
        private readonly ISolutionTypeRepository _solutionTypeRepository;
        private readonly ILogger<SolutionTypesController> _logger;
        private const string DefaultErrorMessage = "Ocurrió un error al procesar la solicitud.";

        public SolutionTypesController(
            ISolutionTypeRepository solutionTypeRepository,
            ILogger<SolutionTypesController> logger)
        {
            _solutionTypeRepository = solutionTypeRepository;
            _logger = logger;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<SolutionType>>> GetAll()
        {
            try
            {
                var result = await _solutionTypeRepository.GetAllOrderedAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo tipos de solución");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SolutionType>> Create([FromBody] SolutionTypeUpsertDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var name = StringSanitizer.SanitizeForInformix(dto.Name)?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "El nombre es requerido" });
                }

                if (await _solutionTypeRepository.ExistsByNameAsync(name))
                {
                    return BadRequest(new { message = $"El tipo de solución '{name}' ya existe" });
                }

                var created = await _solutionTypeRepository.CreateAsync(name);
                return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando tipo de solución");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        [HttpPut("{id:int}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SolutionType>> Update(int id, [FromBody] SolutionTypeUpsertDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var name = StringSanitizer.SanitizeForInformix(dto.Name)?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { message = "El nombre es requerido" });
                }

                if (await _solutionTypeRepository.ExistsByNameExcludingIdAsync(id, name))
                {
                    return BadRequest(new { message = $"El tipo de solución '{name}' ya existe" });
                }

                var updated = await _solutionTypeRepository.UpdateAsync(id, name);
                if (updated == null)
                {
                    return NotFound(new { message = "Tipo de solución no encontrado" });
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando tipo de solución {Id}", id);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        [HttpDelete("{id:int}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _solutionTypeRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Tipo de solución no encontrado" });
                }

                return Ok(new { message = "Tipo de solución eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando tipo de solución {Id}", id);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }
    }
}
