using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Authorize]
    public class ProjectDocumentsController : ControllerBase
    {
        private readonly IProjectDocumentRepository<ProjectDocument> _projectDocumentRepository;
        private readonly IAttachmentRepository<Attachment> _attachmentRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<ProjectDocumentsController> _logger;
        private readonly ApplicationContext _context;
        private readonly ITokenUserService _tokenUserService;
        private const string DefaultErrorMessage = "Ocurrió un error al procesar la solicitud.";

        private static bool IsAdmin(User? user) =>
            string.Equals(user?.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase);

        public ProjectDocumentsController(
            IProjectDocumentRepository<ProjectDocument> projectDocumentRepository,
            IAttachmentRepository<Attachment> attachmentRepository,
            IFileService fileService,
            ILogger<ProjectDocumentsController> logger,
            ApplicationContext context,
            ITokenUserService tokenUserService)
        {
            _projectDocumentRepository = projectDocumentRepository;
            _attachmentRepository = attachmentRepository;
            _fileService = fileService;
            _logger = logger;
            _context = context;
            _tokenUserService = tokenUserService;
        }

        /// <summary>
        /// Method to create a new project document with general section (must be created first before other sections)
        /// </summary>
        /// <param name="dto">Project document data with general section fields</param>
        /// <returns>Created project document</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocument>> Create([FromBody] CreateProjectDocumentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Get current user from JWT token
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
                }

                var created = await _projectDocumentRepository.CreateAsync(dto, currentUser);
                return CreatedAtAction(nameof(GetByProjectCode), new { projectCode = created.ProjectCode }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando el documento de proyecto");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        /// <summary>
        /// Method to get project by project code with all fields and attachments
        /// </summary>
        /// <param name="projectCode">Project code to search</param>
        /// <returns>Project with all sections and attachments grouped by section</returns>
        [HttpGet("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<ActionResult<ProjectDocumentDto>> GetByProjectCode(string projectCode)
        {
            try
            {
                var project = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(StringSanitizer.SanitizeForInformix(projectCode));
                if (project == null)
                {
                    return NotFound(new { message = $"Project with code '{projectCode}' not found" });
                }

                var result = MapToObject.MapToDto(project, _fileService);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo el documento de proyecto con código {ProjectCode}", projectCode);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        /// <summary>
        /// Method to get all project documents ordered by creation date (descending)
        /// </summary>
        /// <returns>List of project documents with section status</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResponseDto<ProjectDocumentListDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? projectCode = null,
            [FromQuery] string? projectName = null,
            [FromQuery] string? sponsor = null,
            [FromQuery] string? technicalLead = null,
            [FromQuery] string? documentStatus = null)
        {
            try
            {
                page = Math.Max(page, 1);
                pageSize = Math.Clamp(pageSize, 5, 100);

                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
                }

                var query = _context.ProjectDocuments
                    .AsNoTracking()
                    .Select(x => new ProjectDocument
                    {
                        Id = x.Id,
                        ProjectCode = x.ProjectCode,
                        ProjectName = x.ProjectName,
                        Sponsor = x.Sponsor,
                        TechnicalLead = x.TechnicalLead,
                        DocumentStatus = x.DocumentStatus,
                        CreatedAt = x.CreatedAt,
                        Identification = x.Identification,
                        Username = x.Username,
                        CreatedBy = x.CreatedBy
                    });

                if (!string.IsNullOrWhiteSpace(projectCode))
                {
                    var term = projectCode.Trim().ToUpper();
                    query = query.Where(p => p.ProjectCode != null && p.ProjectCode.ToUpper().Contains(term));
                }

                if (!string.IsNullOrWhiteSpace(projectName))
                {
                    var term = projectName.Trim().ToUpper();
                    query = query.Where(p => p.ProjectName != null && p.ProjectName.ToUpper().Contains(term));
                }

                if (!string.IsNullOrWhiteSpace(sponsor))
                {
                    var term = sponsor.Trim().ToUpper();
                    query = query.Where(p => p.Sponsor != null && p.Sponsor.ToUpper().Contains(term));
                }

                if (!string.IsNullOrWhiteSpace(technicalLead))
                {
                    var term = technicalLead.Trim().ToUpper();
                    query = query.Where(p => p.TechnicalLead != null && p.TechnicalLead.ToUpper().Contains(term));
                }

                if (!string.IsNullOrWhiteSpace(documentStatus))
                {
                    var term = documentStatus.Trim().ToUpper();
                    query = query.Where(p => p.DocumentStatus != null && p.DocumentStatus.ToUpper().Contains(term));
                }

                var total = await query.CountAsync();

                var projects = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var completedIds = projects
                    .Where(p => string.Equals(p.DocumentStatus, "Completo", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.Id)
                    .ToList();

                var completionSnapshots = await _projectDocumentRepository.GetCompletionSnapshotsByIdsAsync(completedIds);

                var result = new List<ProjectDocumentListDto>();

                foreach (var project in projects)
                {
                    var dto = MapToList.MapToListDto(project);

                    if (completionSnapshots.TryGetValue(project.Id, out var completionSnapshot))
                    {
                        if (ProjectDocumentListAlertEvaluator.HasIncompleteSection(completionSnapshot))
                        {
                            dto.HasIncompleteDocumentAlert = true;
                            dto.IncompleteDocumentAlertMessage = "Documento marcado como completo con secciones incompletas.";
                        }
                    }

                    result.Add(dto);
                }

                var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

                return Ok(new PagedResponseDto<ProjectDocumentListDto>
                {
                    Items = result,
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo la lista de documentos de proyecto");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        /// <summary>
        /// Method to get global KPI metrics for document management
        /// </summary>
        /// <returns>Global KPI summary</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DocumentKpisDto>> GetDocumentKpis()
        {
            try
            {
                var projects = await _context.ProjectDocuments
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.DocumentStatus,
                        HasIntegrations = x.Integrations.Any()
                    })
                    .ToListAsync();

                var totalDocuments = projects.Count;
                var completedDocuments = projects.Count(p => string.Equals(p.DocumentStatus, "Completo", StringComparison.OrdinalIgnoreCase));
                var pendingDocuments = projects.Count(p => string.Equals(p.DocumentStatus, "Pendiente", StringComparison.OrdinalIgnoreCase));
                var inProgressDocuments = Math.Max(0, totalDocuments - completedDocuments - pendingDocuments);
                var documentsWithIntegrations = projects.Count(p => p.HasIntegrations);

                var result = new DocumentKpisDto
                {
                    TotalDocuments = totalDocuments,
                    CompletedDocuments = completedDocuments,
                    InProgressDocuments = inProgressDocuments,
                    PendingDocuments = pendingDocuments,
                    CompletionRate = totalDocuments == 0 ? 0 : Math.Round((decimal)completedDocuments * 100 / totalDocuments, 2),
                    IntegrationsCoverageRate = totalDocuments == 0 ? 0 : Math.Round((decimal)documentsWithIntegrations * 100 / totalDocuments, 2),
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo KPIs documentales");
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        /// <summary>
        /// Method to update architecture section fields and upload attachments in a single request
        /// </summary>
        /// <param name="projectCode">Project code to update</param>
        /// <param name="dto">Architecture section data</param>
        /// <param name="files">Optional attachments to upload</param>
        /// <returns>Updated project with attachments</returns>
        [HttpPut("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateArchitectureSection(
            string projectCode, 
            [FromForm] UpdateArchitectureSectionDto dto,
            [FromForm] IFormFileCollection? files)
        {
            // Initialize transaction context
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var transactionCompleted = false;
                try
                {
                    // Get current user from JWT token
                    var currentUser = await _tokenUserService.GetCurrentUser(User);
                    if (currentUser == null)
                    {
                        return Unauthorized(new { message = "User information not found in token" });
                    }

                    var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(projectCode));
                    var lockedResponse = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                    if (lockedResponse != null)
                    {
                        await transaction.RollbackAsync();
                        return lockedResponse;
                    }

                    await _projectDocumentRepository.UpdateArchitectureSectionAsync(projectCode, dto, currentUser);

                    // Process and upload attachments
                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            var filePath = await _fileService.UploadFileAsync(file, project.Id, "Architecture", project.ProjectCode);
                            
                            await _attachmentRepository.CreateAttachmentAsync(new AttachmentDto
                            {
                                ProjectDocumentId = project.Id,
                                Section = "Architecture",
                                FileName = file.FileName,
                                FilePath = filePath,
                                FileSize = file.Length,
                                ContentType = file.ContentType
                            });
                        }
                    }

                    // Commit transaction if all operations succeed
                    await transaction.CommitAsync();
                    transactionCompleted = true;

                    var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                    return Ok(MapToObject.MapToDto(updatedProject, _fileService));
                }
                catch (InvalidOperationException ex) // Captura de excepción de FileService
                {
                    // Rollback transaction on any error
                    if (!transactionCompleted)
                    {
                        await transaction.RollbackAsync();
                    }
                    return BadRequest(new { message = ex.Message }); // Retorna el mensaje de validación personalizado
                }
                catch (Exception ex)
                {
                    // Rollback transaction on any error
                    if (!transactionCompleted)
                    {
                        await transaction.RollbackAsync();
                    }
                    _logger.LogError(ex, "Error actualizando la sección de arquitectura para el documento de proyecto {ProjectCode}", projectCode);
                    return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
                }
            }
        }

        /// <summary>
        /// Method to update general section fields and upload attachments in a single request
        /// </summary>
        /// <param name="projectCode">Project code to update</param>
        /// <param name="dto">General section data</param>
        /// <returns>Updated project</returns>
        [HttpPut("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateGeneralSection(
            string projectCode, 
            [FromBody] UpdateProjectGeneralSectionDto dto)
        {
            try
            {
                // Get current user from JWT token
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
                }

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(projectCode));
                var lockedResponse = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (lockedResponse != null)
                {
                    return lockedResponse;
                }

                await _projectDocumentRepository.UpdateGeneralSectionAsync(projectCode, dto, currentUser);

                var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updatedProject, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la sección general para el documento de proyecto {ProjectCode}", projectCode);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        /// <summary>
        /// Method to update UX Cases section fields and upload attachments in a single request
        /// Uses database transaction to ensure data integrity - all changes are committed together or rolled back on error
        /// </summary>
        /// <param name="projectCode">Project code to update</param>
        /// <param name="dto">UX Cases section data</param>
        /// <param name="files">Optional attachments to upload</param>
        /// <returns>Updated project with attachments</returns>
        [HttpPut("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateUxCasesSection(
            string projectCode, 
            [FromForm] UpdateUxCasesSectionDto dto,
            [FromForm] IFormFileCollection? useCasesFiles,
            [FromForm] IFormFileCollection? experienceDesignMockupsFiles,
            [FromForm] IFormFileCollection? files)
        {
            // Initialize transaction context
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var transactionCompleted = false;
                try
                {
                    // Get current user from JWT token
                    var currentUser = await _tokenUserService.GetCurrentUser(User);
                    if (currentUser == null)
                    {
                        return Unauthorized(new { message = "User information not found in token" });
                    }

                    var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(projectCode));
                    var lockedResponse = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                    if (lockedResponse != null)
                    {
                        await transaction.RollbackAsync();
                        return lockedResponse;
                    }

                    // Update UX Cases section
                    await _projectDocumentRepository.UpdateUxCasesSectionAsync(projectCode, dto, currentUser);

                    // Process and upload use cases attachments
                    if (useCasesFiles != null && useCasesFiles.Count > 0)
                    {
                        foreach (var file in useCasesFiles)
                        {
                            var filePath = await _fileService.UploadFileAsync(file, project.Id, "UxCasesUseCases", project.ProjectCode);

                            await _attachmentRepository.CreateAttachmentAsync(new AttachmentDto
                            {
                                ProjectDocumentId = project.Id,
                                Section = "UxCasesUseCases",
                                FileName = file.FileName,
                                FilePath = filePath,
                                FileSize = file.Length,
                                ContentType = file.ContentType
                            });
                        }
                    }

                    // Process and upload UX mockups attachments
                    if (experienceDesignMockupsFiles != null && experienceDesignMockupsFiles.Count > 0)
                    {
                        foreach (var file in experienceDesignMockupsFiles)
                        {
                            var filePath = await _fileService.UploadFileAsync(file, project.Id, "UxCasesExperienceDesignMockups", project.ProjectCode);

                            await _attachmentRepository.CreateAttachmentAsync(new AttachmentDto
                            {
                                ProjectDocumentId = project.Id,
                                Section = "UxCasesExperienceDesignMockups",
                                FileName = file.FileName,
                                FilePath = filePath,
                                FileSize = file.Length,
                                ContentType = file.ContentType
                            });
                        }
                    }

                    // Legacy support: "files" fallback
                    if (files != null && files.Count > 0)
                    {
                        foreach (var file in files)
                        {
                            var filePath = await _fileService.UploadFileAsync(file, project.Id, "UxCases", project.ProjectCode);

                            await _attachmentRepository.CreateAttachmentAsync(new AttachmentDto
                            {
                                ProjectDocumentId = project.Id,
                                Section = "UxCases",
                                FileName = file.FileName,
                                FilePath = filePath,
                                FileSize = file.Length,
                                ContentType = file.ContentType
                            });
                        }
                    }

                    // Commit transaction if all operations succeed
                    await transaction.CommitAsync();
                    transactionCompleted = true;

                    var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                    return Ok(MapToObject.MapToDto(updatedProject, _fileService));
                }
                catch (InvalidOperationException ex) // Captura de excepción de FileService
                {
                    // Rollback transaction on any error
                    if (!transactionCompleted)
                    {
                        await transaction.RollbackAsync();
                    }
                    return BadRequest(new { message = ex.Message }); // Retorna el mensaje de validación personalizado
                }
                catch (Exception ex)
                {
                    // Rollback transaction on any error
                    if (!transactionCompleted)
                    {
                        await transaction.RollbackAsync();
                    }
                    _logger.LogError(ex, "Error actualizando la sección UX para el documento de proyecto {ProjectCode}. Transacción revertida.", projectCode);
                    return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
                }
            }
        }

        /// <summary>
        /// Method to update Constraints section fields in a single request
        /// </summary>
        /// <param name="projectCode">Project code to update</param>
        /// <param name="dto">Constraints section data</param>
        /// <returns>Updated project</returns>
        [HttpPut("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateConstraintsSection(
            string projectCode, 
            [FromBody] UpdateConstraintsSectionDto dto)
        {
            try
            {
                // Get current user from JWT token
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
                }

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(projectCode));
                var lockedResponse = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (lockedResponse != null)
                {
                    return lockedResponse;
                }

                await _projectDocumentRepository.UpdateConstraintsSectionAsync(projectCode, dto, currentUser);

                var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updatedProject, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la sección de restricciones para el documento de proyecto {ProjectCode}", projectCode);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        /// <summary>
        /// Method to update Areas and Integrations section fields in a single request
        /// </summary>
        /// <param name="projectCode">Project code to update</param>
        /// <param name="dto">Areas and Integrations section data</param>
        /// <returns>Updated project</returns>
        [HttpPut("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateAreasIntegrationsSection(
            string projectCode, 
            [FromBody] UpdateAreasIntegrationsSectionDto dto)
        {
            try
            {
                // Get current user from JWT token
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
                }

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(projectCode));
                var lockedResponse = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (lockedResponse != null)
                {
                    return lockedResponse;
                }

                await _projectDocumentRepository.UpdateAreasIntegrationsSectionAsync(projectCode, dto, currentUser);

                var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updatedProject, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la sección de áreas e integraciones para el documento de proyecto {ProjectCode}", projectCode);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

        /// <summary>
        /// Method to update RACI section fields in a single request
        /// </summary>
        /// <param name="projectCode">Project code to update</param>
        /// <param name="dto">RACI section data</param>
        /// <returns>Updated project</returns>
        [HttpPut("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateRaciSection(
            string projectCode, 
            [FromBody] UpdateRaciSectionDto dto)
        {
            try
            {
                // Get current user from JWT token
                var currentUser = await _tokenUserService.GetCurrentUser(User);
                if (currentUser == null)
                {
                    return Unauthorized(new { message = "User information not found in token" });
                }

                var project = await _projectDocumentRepository.GetByProjectCodeAsync(projectCode);
                var lockedResponse = DocumentEditGuard.EnsureEditable(project, projectCode, IsAdmin(currentUser));
                if (lockedResponse != null)
                {
                    return lockedResponse;
                }

                await _projectDocumentRepository.UpdateRaciSectionAsync(projectCode, dto, currentUser);

                var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updatedProject, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la sección RACI para el documento de proyecto {ProjectCode}", projectCode);
                return ApiErrorResponse.BadRequest(this, ex, DefaultErrorMessage);
            }
        }

    }
}
