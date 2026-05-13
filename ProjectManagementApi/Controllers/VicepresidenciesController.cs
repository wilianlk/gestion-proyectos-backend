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
    public class VicepresidenciesController : ControllerBase
    {
        private readonly IVicepresidencyRepository _vicepresidencyRepository;
        private readonly ILogger<VicepresidenciesController> _logger;
        private const string DefaultErrorMessage = "Ocurrio un error al procesar la solicitud.";

        public VicepresidenciesController(
            IVicepresidencyRepository vicepresidencyRepository,
            ILogger<VicepresidenciesController> logger)
        {
            _vicepresidencyRepository = vicepresidencyRepository;
            _logger = logger;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Vicepresidency>>> GetAll()
        {
            try
            {
                var result = await _vicepresidencyRepository.GetAllOrderedAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo vicepresidencias");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Vicepresidency>> Create([FromBody] SolutionTypeUpsertDto dto)
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

                if (await _vicepresidencyRepository.ExistsByNameAsync(name))
                {
                    return BadRequest(new { message = $"La vicepresidencia '{name}' ya existe" });
                }

                var created = await _vicepresidencyRepository.CreateAsync(name);
                return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando vicepresidencia");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        [HttpPut("{id:int}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Vicepresidency>> Update(int id, [FromBody] SolutionTypeUpsertDto dto)
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

                if (await _vicepresidencyRepository.ExistsByNameExcludingIdAsync(id, name))
                {
                    return BadRequest(new { message = $"La vicepresidencia '{name}' ya existe" });
                }

                var updated = await _vicepresidencyRepository.UpdateAsync(id, name);
                if (updated == null)
                {
                    return NotFound(new { message = "Vicepresidencia no encontrada" });
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando vicepresidencia {Id}", id);
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
                var deleted = await _vicepresidencyRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Vicepresidencia no encontrada" });
                }

                return Ok(new { message = "Vicepresidencia eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando vicepresidencia {Id}", id);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }
    }
}
