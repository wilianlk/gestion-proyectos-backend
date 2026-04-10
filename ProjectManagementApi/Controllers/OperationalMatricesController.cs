using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationalMatricesController : ControllerBase
    {
        private readonly IProjectDocumentRepository<ProjectDocument> _projectDocumentRepository;
        private readonly IProjectDocumentRequirementRepository<ProjectDocumentRequirement> _requirementRepository;
        private readonly IProjectDocumentIntegrationRepository<ProjectDocumentIntegration> _integrationRepository;
        private readonly IProjectDocumentRaciActorRepository<ProjectDocumentRaciActor> _raciActorRepository;

        public OperationalMatricesController(
            IProjectDocumentRepository<ProjectDocument> projectDocumentRepository,
            IProjectDocumentRequirementRepository<ProjectDocumentRequirement> requirementRepository,
            IProjectDocumentIntegrationRepository<ProjectDocumentIntegration> integrationRepository,
            IProjectDocumentRaciActorRepository<ProjectDocumentRaciActor> raciActorRepository)
        {
            _projectDocumentRepository = projectDocumentRepository;
            _requirementRepository = requirementRepository;
            _integrationRepository = integrationRepository;
            _raciActorRepository = raciActorRepository;
        }

        /// <summary>
        /// Method to create or update requerimientos for a project document
        /// </summary>
        /// <param name="dto">Requirements data</param>
        /// <returns>Created requirements</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<RequirementDto>>> UpsertRequirements([FromBody] CreateRequirementDto dto)
        {
            if (dto.Requirements == null || dto.Requirements.Count == 0)
            {
                return BadRequest(new { message = "No requirements provided" });
            }

            var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(dto.ProjectCode));
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{dto.ProjectCode}' not found" });
            }

            await _requirementRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

            var entities = dto.Requirements.Select(r => new ProjectDocumentRequirement
            {
                ProjectDocumentId = project.Id,
                Code = r.Code,
                Description = r.Description,
                Type = r.Type,
                Priority = r.Priority,
                AcceptanceCriteria = r.AcceptanceCriteria,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                // TODO: Add user context to get actual username instead of hardcoding
                Identification = "1234567890",
                Username = "dev"
            }).ToList();

            _requirementRepository.AddRange(entities);
            await _requirementRepository.SaveChangesAsync();

            var result = entities.Select(e => new RequirementDto
            {
                Id = e.Id,
                ProjectDocumentId = e.ProjectDocumentId,
                Code = e.Code,
                Description = e.Description,
                Type = e.Type,
                Priority = e.Priority,
                AcceptanceCriteria = e.AcceptanceCriteria
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Method to create or update integrations for a project document
        /// </summary>
        /// <param name="dto">Integrations data</param>
        /// <returns>Created integrations</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<IntegrationDto>>> UpsertIntegrations([FromBody] CreateIntegrationDto dto)
        {
            if (dto.Integrations == null || dto.Integrations.Count == 0)
            {
                return BadRequest(new { message = "No integrations provided" });
            }

            var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(dto.ProjectCode));
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{dto.ProjectCode}' not found" });
            }

            await _integrationRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

            var entities = dto.Integrations.Select(i => new ProjectDocumentIntegration
            {
                ProjectDocumentId = project.Id,
                System = i.System,
                Description = i.Description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                // TODO: Add user context to get actual username instead of hardcoding
                Identification = "1234567890",
                Username = "dev"
            }).ToList();

            _integrationRepository.AddRange(entities);
            await _integrationRepository.SaveChangesAsync();

            var result = entities.Select(e => new IntegrationDto
            {
                Id = e.Id,
                ProjectDocumentId = e.ProjectDocumentId,
                System = e.System,
                Description = e.Description
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Method to create or update RACI actors for a project document
        /// </summary>
        /// <param name="dto">RACI Actors data</param>
        /// <returns>Created RACI actors</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<RaciActorDto>>> UpsertRaciActor([FromBody] CreateRaciActorDto dto)
        {
            if (dto.Rows == null || dto.Rows.Count == 0)
            {
                return BadRequest(new { message = "No RACI matrix rows provided" });
            }

            var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(dto.ProjectCode));
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{dto.ProjectCode}' not found" });
            }

            await _raciActorRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

            var entities = dto.Rows.Select(a => new ProjectDocumentRaciActor
            {
                ProjectDocumentId = project.Id,
                Activity = a.Activity,
                Type = a.Type,
                Area = a.Area,
                Role = a.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                // TODO: Add user context to get actual username instead of hardcoding
                Identification = "1234567890",
                Username = "dev"
            }).ToList();

            _raciActorRepository.AddRange(entities);
            await _raciActorRepository.SaveChangesAsync();

            var result = entities.Select(e => new RaciActorDto
            {
                Id = e.Id,
                ProjectDocumentId = e.ProjectDocumentId,
                Activity = e.Activity,
                Type = e.Type,
                Area = e.Area,
                Role = e.Role
            }).ToList();

            return Ok(result);
        }

    }
}