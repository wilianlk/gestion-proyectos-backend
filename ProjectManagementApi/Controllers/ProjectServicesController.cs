using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.Context;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils;

namespace ProjectManagementApi.Controllers
{
    /// <summary>
    /// APIs de ProjectServices para consulta, inserción y actualización de proyectos.
    /// Optimizadas para consumo programático externo.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectServicesController : ControllerBase
    {
        private readonly IProjectDocumentRepository<ProjectDocument> _projectDocumentRepository;
        private readonly IProjectDocumentRequirementRepository<ProjectDocumentRequirement> _requirementRepository;
        private readonly IProjectDocumentRiskRepository<ProjectDocumentRisk> _riskRepository;
        private readonly IProjectDocumentTestCaseRepository<ProjectDocumentTestCase> _testCaseRepository;
        private readonly IProjectDocumentIntegrationRepository<ProjectDocumentIntegration> _integrationRepository;
        private readonly IProjectDocumentRaciActorRepository<ProjectDocumentRaciActor> _raciActorRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<ProjectServicesController> _logger;
        private readonly ApplicationContext _context;
        private readonly ITokenUserService _tokenUserService;
        private const string DefaultErrorMessage = "Ocurrió un error al procesar la solicitud.";

        public ProjectServicesController(
            IProjectDocumentRepository<ProjectDocument> projectDocumentRepository,
            IProjectDocumentRequirementRepository<ProjectDocumentRequirement> requirementRepository,
            IProjectDocumentRiskRepository<ProjectDocumentRisk> riskRepository,
            IProjectDocumentTestCaseRepository<ProjectDocumentTestCase> testCaseRepository,
            IProjectDocumentIntegrationRepository<ProjectDocumentIntegration> integrationRepository,
            IProjectDocumentRaciActorRepository<ProjectDocumentRaciActor> raciActorRepository,
            IFileService fileService,
            ILogger<ProjectServicesController> logger,
            ApplicationContext context,
            ITokenUserService tokenUserService)
        {
            _projectDocumentRepository = projectDocumentRepository;
            _requirementRepository = requirementRepository;
            _riskRepository = riskRepository;
            _testCaseRepository = testCaseRepository;
            _integrationRepository = integrationRepository;
            _raciActorRepository = raciActorRepository;
            _fileService = fileService;
            _logger = logger;
            _context = context;
            _tokenUserService = tokenUserService;
        }

        private static bool IsAdmin(User? user) =>
            string.Equals(user?.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase);

        // =============================================
        // CONSULTA - Endpoints de lectura
        // =============================================

        /// <summary>
        /// Retorna todos los proyectos con detalle completo: todas las secciones, entidades hijas y estados calculados.
        /// </summary>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<ProjectDocumentDto>>> GetAll()
        {
            try
            {
                var projects = await _projectDocumentRepository.GetAllOrderedAsync();
                var allIds = projects.Select(p => p.Id).ToList();
                var detailedByIds = await _projectDocumentRepository.GetDetailedByIdsAsync(allIds);

                var result = new List<ProjectDocumentDto>();
                foreach (var project in projects)
                {
                    if (detailedByIds.TryGetValue(project.Id, out var detailed))
                    {
                        var dto = MapToObject.MapToDto(detailed, _fileService);
                        if (dto != null) result.Add(dto);
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo todos los proyectos con detalle completo");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Retorna un proyecto específico con detalle completo por su código.
        /// </summary>
        [HttpGet("[action]/{projectCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> GetByCode(string projectCode)
        {
            try
            {
                var project = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(
                    StringSanitizer.SanitizeForInformix(projectCode));

                if (project == null)
                    return NotFound(new { message = $"Proyecto con código '{projectCode}' no encontrado" });

                return Ok(MapToObject.MapToDto(project, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo el proyecto {ProjectCode}", projectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Retorna un resumen de todos los proyectos con estados de sección calculados.
        /// </summary>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<ProjectSummaryDto>>> GetSummary()
        {
            try
            {
                var projects = await _projectDocumentRepository.GetAllOrderedAsync();
                var allIds = projects.Select(p => p.Id).ToList();
                var detailedByIds = await _projectDocumentRepository.GetDetailedByIdsAsync(allIds);

                var result = new List<ProjectSummaryDto>();
                foreach (var project in projects)
                {
                    if (detailedByIds.TryGetValue(project.Id, out var detailed))
                    {
                        result.Add(new ProjectSummaryDto
                        {
                            Id = detailed.Id,
                            ProjectCode = detailed.ProjectCode,
                            ProjectName = detailed.ProjectName,
                            Sponsor = detailed.Sponsor,
                            FunctionalLead = detailed.FunctionalLead,
                            TechnicalLead = detailed.TechnicalLead,
                            DocumentStatus = detailed.DocumentStatus,
                            Area = detailed.Area,
                            Division = detailed.Division,
                            CreatedAt = detailed.CreatedAt,
                            RequirementsCount = detailed.Requirements?.Count ?? 0,
                            RisksCount = detailed.Risks?.Count ?? 0,
                            TestCasesCount = detailed.TestCases?.Count ?? 0,
                            IntegrationsCount = detailed.Integrations?.Count ?? 0,
                            RaciActorsCount = detailed.RaciActors?.Count ?? 0,
                            SectionStatuses = new Dictionary<string, string>
                            {
                                ["General"] = CalculateStatus.CalculateGeneralSectionStatus(detailed),
                                ["Arquitectura"] = CalculateStatus.CalculateArchitectureSectionStatus(detailed),
                                ["CasosUx"] = CalculateStatus.CalculateUxCasesSectionStatus(detailed),
                                ["Restricciones"] = CalculateStatus.CalculateConstraintsSectionStatus(detailed),
                                ["AreasIntegraciones"] = CalculateStatus.CalculateAreasIntegrationsSectionStatus(detailed),
                                ["RACI"] = CalculateStatus.CalculateRaciSectionStatus(detailed),
                                ["Requerimientos"] = CalculateStatus.CalculateRequirementsSectionStatus(detailed),
                                ["Integraciones"] = CalculateStatus.CalculateIntegrationsSectionStatus(detailed),
                                ["Riesgos"] = CalculateStatus.CalculateRiskSectionStatus(detailed),
                                ["Pruebas"] = CalculateStatus.CalculateTestCaseSectionStatus(detailed)
                            }
                        });
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo el resumen de proyectos");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        // =============================================
        // INSERCIÓN - Endpoints de escritura
        // =============================================

        /// <summary>
        /// Agrega requerimientos a un proyecto (JSON). Reemplaza los existentes.
        /// </summary>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RequirementDto>>> AddRequirements([FromBody] CreateRequirementDto dto)
        {
            if (dto.Requirements == null || dto.Requirements.Count == 0)
                return BadRequest(new { message = "No se proporcionaron requerimientos" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(dto.ProjectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, dto.ProjectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _requirementRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

                var entities = dto.Requirements.Select(r => new ProjectDocumentRequirement
                {
                    ProjectDocumentId = project!.Id,
                    Code = StringSanitizer.SanitizeForInformix(r.Code),
                    Description = StringSanitizer.SanitizeForInformix(r.Description),
                    Type = StringSanitizer.SanitizeForInformix(r.Type),
                    Priority = r.Priority,
                    AcceptanceCriteria = StringSanitizer.SanitizeForInformix(r.AcceptanceCriteria),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedBy = currentUser.Username,
                    Identification = currentUser.Identification,
                    Username = currentUser.Username
                }).ToList();

                _requirementRepository.AddRange(entities);
                await _requirementRepository.SaveChangesAsync();
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error agregando requerimientos para {ProjectCode}", dto.ProjectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Agrega riesgos a un proyecto (JSON). Reemplaza los existentes.
        /// </summary>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RiskDto>>> AddRisks([FromBody] CreateRiskDto dto)
        {
            if (dto.Risks == null || dto.Risks.Count == 0)
                return BadRequest(new { message = "No se proporcionaron riesgos" });

            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(dto.ProjectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, dto.ProjectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _riskRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

                var entities = dto.Risks.Select(r => new ProjectDocumentRisk
                {
                    ProjectDocumentId = project!.Id,
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
                _logger.LogError(ex, "Error agregando riesgos para {ProjectCode}", dto.ProjectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Agrega casos de prueba a un proyecto (JSON). Reemplaza los existentes.
        /// </summary>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<TestCaseDto>>> AddTestCases([FromBody] CreateTestCaseDto dto)
        {
            if (dto.TestCases == null || dto.TestCases.Count == 0)
                return BadRequest(new { message = "No se proporcionaron casos de prueba" });

            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(dto.ProjectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, dto.ProjectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _testCaseRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

                var entities = dto.TestCases.Select(t => new ProjectDocumentTestCase
                {
                    ProjectDocumentId = project!.Id,
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
                _logger.LogError(ex, "Error agregando casos de prueba para {ProjectCode}", dto.ProjectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Agrega integraciones a un proyecto (JSON). Reemplaza las existentes.
        /// </summary>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<IntegrationDto>>> AddIntegrations([FromBody] CreateIntegrationDto dto)
        {
            if (dto.Integrations == null || dto.Integrations.Count == 0)
                return BadRequest(new { message = "No se proporcionaron integraciones" });

            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(dto.ProjectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, dto.ProjectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _integrationRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

                var entities = dto.Integrations
                    .Where(i => !string.IsNullOrWhiteSpace(i.System) && !string.IsNullOrWhiteSpace(i.Description))
                    .Select(i => new ProjectDocumentIntegration
                    {
                        ProjectDocumentId = project!.Id,
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
                _logger.LogError(ex, "Error agregando integraciones para {ProjectCode}", dto.ProjectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Agrega actores RACI a un proyecto (JSON). Reemplaza los existentes.
        /// </summary>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RaciActorDto>>> AddRaciActors([FromBody] CreateRaciActorDto dto)
        {
            if (dto.Rows == null || dto.Rows.Count == 0)
                return BadRequest(new { message = "No se proporcionaron actores RACI" });

            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(dto.ProjectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, dto.ProjectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _raciActorRepository.DeleteByProjectCodeAsync(dto.ProjectCode);

                var entities = dto.Rows.Select(a => new ProjectDocumentRaciActor
                {
                    ProjectDocumentId = project!.Id,
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
                _logger.LogError(ex, "Error agregando actores RACI para {ProjectCode}", dto.ProjectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        // =============================================
        // ACTUALIZACIÓN - Endpoints de actualización de secciones
        // =============================================

        /// <summary>
        /// Actualiza la sección General de un proyecto.
        /// </summary>
        [HttpPut("[action]/{projectCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateGeneralSection(string projectCode, [FromBody] UpdateProjectGeneralSectionDto dto)
        {
            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(projectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _projectDocumentRepository.UpdateGeneralSectionAsync(projectCode, dto, currentUser);

                var updated = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updated, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando sección General para {ProjectCode}", projectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Actualiza la sección de Arquitectura de un proyecto.
        /// </summary>
        [HttpPut("[action]/{projectCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateArchitectureSection(string projectCode, [FromBody] UpdateArchitectureSectionDto dto)
        {
            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(projectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _projectDocumentRepository.UpdateArchitectureSectionAsync(projectCode, dto, currentUser);

                var updated = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updated, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando sección Arquitectura para {ProjectCode}", projectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Actualiza la sección de Casos de Uso y UX de un proyecto.
        /// </summary>
        [HttpPut("[action]/{projectCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateUxCasesSection(string projectCode, [FromBody] UpdateUxCasesSectionDto dto)
        {
            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(projectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _projectDocumentRepository.UpdateUxCasesSectionAsync(projectCode, dto, currentUser);

                var updated = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updated, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando sección CasosUx para {ProjectCode}", projectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Actualiza la sección de Restricciones de un proyecto.
        /// </summary>
        [HttpPut("[action]/{projectCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateConstraintsSection(string projectCode, [FromBody] UpdateConstraintsSectionDto dto)
        {
            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(projectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _projectDocumentRepository.UpdateConstraintsSectionAsync(projectCode, dto, currentUser);

                var updated = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updated, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando sección Restricciones para {ProjectCode}", projectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Actualiza la sección de Áreas e Integraciones de un proyecto.
        /// </summary>
        [HttpPut("[action]/{projectCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateAreasIntegrationsSection(string projectCode, [FromBody] UpdateAreasIntegrationsSectionDto dto)
        {
            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(projectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _projectDocumentRepository.UpdateAreasIntegrationsSectionAsync(projectCode, dto, currentUser);

                var updated = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updated, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando sección AreasIntegraciones para {ProjectCode}", projectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }

        /// <summary>
        /// Actualiza la sección RACI de un proyecto.
        /// </summary>
        [HttpPut("[action]/{projectCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateRaciSection(string projectCode, [FromBody] UpdateRaciSectionDto dto)
        {
            try
            {
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                    return Unauthorized(new { message = "User information not found in token" });

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(
                    StringSanitizer.SanitizeForInformix(projectCode));
                var guard = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (guard != null) return guard;

                await _projectDocumentRepository.UpdateRaciSectionAsync(projectCode, dto, currentUser);

                var updated = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updated, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando sección RACI para {ProjectCode}", projectCode);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = DefaultErrorMessage });
            }
        }
    }
}
