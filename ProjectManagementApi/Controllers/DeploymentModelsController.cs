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
    public class DeploymentModelsController : ControllerBase
    {
        private readonly IDeploymentModelRepository _deploymentModelRepository;
        private readonly ILogger<DeploymentModelsController> _logger;
        private const string DefaultErrorMessage = "OcurriÃ³ un error al procesar la solicitud.";

        public DeploymentModelsController(
            IDeploymentModelRepository deploymentModelRepository,
            ILogger<DeploymentModelsController> logger)
        {
            _deploymentModelRepository = deploymentModelRepository;
            _logger = logger;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<DeploymentModel>>> GetAll()
        {
            try
            {
                var result = await _deploymentModelRepository.GetAllOrderedAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo modelos de despliegue");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DeploymentModel>> Create([FromBody] SolutionTypeUpsertDto dto)
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

                if (await _deploymentModelRepository.ExistsByNameAsync(name))
                {
                    return BadRequest(new { message = $"El modelo de despliegue '{name}' ya existe" });
                }

                var created = await _deploymentModelRepository.CreateAsync(name);
                return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando modelo de despliegue");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        [HttpPut("{id:int}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DeploymentModel>> Update(int id, [FromBody] SolutionTypeUpsertDto dto)
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

                if (await _deploymentModelRepository.ExistsByNameExcludingIdAsync(id, name))
                {
                    return BadRequest(new { message = $"El modelo de despliegue '{name}' ya existe" });
                }

                var updated = await _deploymentModelRepository.UpdateAsync(id, name);
                if (updated == null)
                {
                    return NotFound(new { message = "Modelo de despliegue no encontrado" });
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando modelo de despliegue {Id}", id);
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
                var deleted = await _deploymentModelRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = "Modelo de despliegue no encontrado" });
                }

                return Ok(new { message = "Modelo de despliegue eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando modelo de despliegue {Id}", id);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }
    }
}
