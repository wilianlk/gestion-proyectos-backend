using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProjectManagementApi.Context;
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
        private readonly IProjectDocumentRiskRepository<ProjectDocumentRisk> _riskRepository;
        private readonly IProjectDocumentTestCaseRepository<ProjectDocumentTestCase> _testCaseRepository;
        private readonly IAttachmentRepository<Attachment> _attachmentRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<OperationalMatricesController> _logger;
        private readonly ApplicationContext _context;
        private const string DefaultErrorMessage = "Ocurrió un error al procesar la solicitud.";
        private readonly ITokenUserService _tokenUserService;

        public OperationalMatricesController(
            IProjectDocumentRepository<ProjectDocument> projectDocumentRepository,
            IProjectDocumentRequirementRepository<ProjectDocumentRequirement> requirementRepository,
            IProjectDocumentIntegrationRepository<ProjectDocumentIntegration> integrationRepository,
            IProjectDocumentRaciActorRepository<ProjectDocumentRaciActor> raciActorRepository,
            IProjectDocumentRiskRepository<ProjectDocumentRisk> riskRepository,
            IProjectDocumentTestCaseRepository<ProjectDocumentTestCase> testCaseRepository,
            IAttachmentRepository<Attachment> attachmentRepository,
            IFileService fileService,
            ILogger<OperationalMatricesController> logger,
            ApplicationContext context,
            ITokenUserService tokenUserService)
        {
            _projectDocumentRepository = projectDocumentRepository;
            _requirementRepository = requirementRepository;
            _integrationRepository = integrationRepository;
            _raciActorRepository = raciActorRepository;
            _riskRepository = riskRepository;
            _testCaseRepository = testCaseRepository;
            _attachmentRepository = attachmentRepository;
            _fileService = fileService;
            _logger = logger;
            _context = context;
            _tokenUserService = tokenUserService;
        }

        /// <summary>
        /// Method to create or update requerimientos for a project document
        /// </summary>
        /// <param name="dto">Requirements data</param>
        /// <param name="files">Optional attachments to upload</param>
        /// <returns>Created requirements</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<List<RequirementDto>>> UpsertRequirements(
            [FromForm] CreateRequirementDto dto,
            [FromForm] IFormFileCollection? files)
        {
            if (dto.Requirements == null || dto.Requirements.Count == 0)
            {
                return BadRequest(new { message = "No se proporcionaron requerimientos" });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var currentUser = await _tokenUserService.GetCurrentUser(User);
                    if (currentUser == null)
                    {
                        await transaction.RollbackAsync();
                        return Unauthorized(new { message = "User information not found in token" });
                    }

                    var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(dto.ProjectCode));
                    if (project == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound(new { message = $"Project with code '{dto.ProjectCode}' not found" });
                    }

                    await _requirementRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

                    var entities = dto.Requirements.Select(r => new ProjectDocumentRequirement
                    {
                        ProjectDocumentId = project.Id,
                        Code = StringSanitizer.SanitizeForInformix(r.Code),
                        Description = StringSanitizer.SanitizeForInformix(r.Description),
                        Type = StringSanitizer.SanitizeForInformix(r.Type),
                        Priority = r.Priority,
                        AcceptanceCriteria = StringSanitizer.SanitizeForInformix(r.AcceptanceCriteria),
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        Identification = currentUser.Identification,
                        Username = currentUser.Username
                    }).ToList();

                    _requirementRepository.AddRange(entities);
                    await _requirementRepository.SaveChangesAsync();

                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            var filePath = await _fileService.UploadFileAsync(file, project.Id, "Requirements");
                            
                            await _attachmentRepository.CreateAttachmentAsync(new AttachmentDto
                            {
                                ProjectDocumentId = project.Id,
                                Section = "Requirements",
                                FileName = file.FileName,
                                FilePath = filePath,
                                FileSize = file.Length,
                                ContentType = file.ContentType
                            });
                        }
                    }

                    await transaction.CommitAsync();

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
                catch (InvalidOperationException ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error al crear o actualizar los requerimientos para el proyecto {ProjectCode}", dto.ProjectCode);
                    return BadRequest(new { message = DefaultErrorMessage });
                }
            }
        }

        /// <summary>
        /// Method to create or update integrations for a project document
        /// </summary>
        /// <param name="dto">Integrations data</param>
        /// <returns>Created integrations</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<IntegrationDto>>> UpsertIntegrations([FromBody] CreateIntegrationDto dto)
        {
            try
            {
                if (dto.Integrations == null || dto.Integrations.Count == 0)
                {
                    return BadRequest(new { message = "No se proporcionaron integraciones" });
                }

                // Get current user from JWT token
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
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
                    System = StringSanitizer.SanitizeForInformix(i.System),
                    Description = StringSanitizer.SanitizeForInformix(i.Description),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Identification = currentUser.Identification,
                    Username = currentUser.Username
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear o actualizar las integraciones para el proyecto {ProjectCode}", dto.ProjectCode);
                return BadRequest(new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Method to create or update RACI actors for a project document
        /// </summary>
        /// <param name="dto">RACI Actors data</param>
        /// <returns>Created RACI actors</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RaciActorDto>>> UpsertRaciActor([FromBody] CreateRaciActorDto dto)
        {
            try
            {
                if (dto.Rows == null || dto.Rows.Count == 0)
                {
                    return BadRequest(new { message = "No se proporcionaron filas para la matriz RACI" });
                }

                // Get current user from JWT token
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
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
                    Activity = StringSanitizer.SanitizeForInformix(a.Activity),
                    Type = a.Type,
                    Area = a.Area,
                    Role = a.Role,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Identification = currentUser.Identification,
                    Username = currentUser.Username
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear o actualizar los actores RACI para el proyecto {ProjectCode}", dto.ProjectCode);
                return BadRequest(new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Method to create or update risks for a project document
        /// </summary>
        /// <param name="dto">Risks data</param>
        /// <returns>Created risks</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RiskDto>>> UpsertRisks([FromBody] CreateRiskDto dto)
        {
            try
            {
                if (dto.Risks == null || dto.Risks.Count == 0)
                {
                    return BadRequest(new { message = "No se proporcionaron riesgos" });
                }

                // Get current user from JWT token
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
                }

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(dto.ProjectCode));
                if (project == null)
                {
                    return NotFound(new { message = $"Project with code '{dto.ProjectCode}' not found" });
                }

                await _riskRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

                var entities = dto.Risks.Select(r => new ProjectDocumentRisk
                {
                    ProjectDocumentId = project.Id,
                    Risk = StringSanitizer.SanitizeForInformix(r.Risk),
                    Impact = r.Impact,
                    Probability = r.Probability,
                    Mitigation = StringSanitizer.SanitizeForInformix(r.Mitigation),
                    Owner = StringSanitizer.SanitizeForInformix(r.Owner),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Identification = currentUser.Identification,
                    Username = currentUser.Username
                }).ToList();

                _riskRepository.AddRange(entities);
                await _riskRepository.SaveChangesAsync();

                var result = entities.Select(e => new RiskDto
                {
                    Id = e.Id,
                    ProjectDocumentId = e.ProjectDocumentId,
                    Risk = e.Risk,
                    Impact = e.Impact,
                    Probability = e.Probability,
                    Mitigation = e.Mitigation,
                    Owner = e.Owner
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear o actualizar los riesgos para el proyecto {ProjectCode}", dto.ProjectCode);
                return BadRequest(new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Method to create or update test cases for a project document
        /// </summary>
        /// <param name="dto">Test cases data</param>
        /// <returns>Created test cases</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<TestCaseDto>>> UpsertTestCases([FromBody] CreateTestCaseDto dto)
        {
            try
            {
                if (dto.TestCases == null || dto.TestCases.Count == 0)
                {
                    return BadRequest(new { message = "No se proporcionaron casos de prueba" });
                }

                // Get current user from JWT token
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
                }

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(dto.ProjectCode));
                if (project == null)
                {
                    return NotFound(new { message = $"Project with code '{dto.ProjectCode}' not found" });
                }

                await _testCaseRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

                var entities = dto.TestCases.Select(t => new ProjectDocumentTestCase
                {
                    ProjectDocumentId = project.Id,
                    TestStrategy = StringSanitizer.SanitizeForInformix(t.TestStrategy),
                    AcceptanceCriteria = StringSanitizer.SanitizeForInformix(t.AcceptanceCriteria),
                    DeployProductionCriteria = StringSanitizer.SanitizeForInformix(t.DeployProductionCriteria),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Identification = currentUser.Identification,
                    Username = currentUser.Username
                }).ToList();

                _testCaseRepository.AddRange(entities);
                await _testCaseRepository.SaveChangesAsync();

                var result = entities.Select(e => new TestCaseDto
                {
                    Id = e.Id,
                    ProjectDocumentId = e.ProjectDocumentId,
                    TestStrategy = e.TestStrategy,
                    AcceptanceCriteria = e.AcceptanceCriteria,
                    DeployProductionCriteria = e.DeployProductionCriteria
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear o actualizar los casos de prueba para el proyecto {ProjectCode}", dto.ProjectCode);
                return BadRequest(new { message = DefaultErrorMessage });
            }
        }

    }
}