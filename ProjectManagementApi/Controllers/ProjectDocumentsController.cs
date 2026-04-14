using Microsoft.AspNetCore.Authorization;
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

                if (await _projectDocumentRepository.ExistsByProjectCodeAsync(dto.ProjectCode))
                {
                    return BadRequest(new { message = $"Project code '{dto.ProjectCode}' already exists" });
                }

                var created = await _projectDocumentRepository.CreateAsync(dto, currentUser);
                return CreatedAtAction(nameof(GetByProjectCode), new { projectCode = created.ProjectCode }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando el documento de proyecto");
                return BadRequest(new { message = DefaultErrorMessage });
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
                return BadRequest(new { message = DefaultErrorMessage });
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
        public async Task<ActionResult<List<ProjectDocumentListDto>>> GetAll()
        {
            try
            {
                var projects = await _projectDocumentRepository.GetAllOrderedAsync();
                var result = projects.Select(MapToList.MapToListDto).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo la lista de documentos de proyecto");
                return BadRequest(new { message = DefaultErrorMessage });
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
            [FromForm] IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "Se requiere al menos un archivo." });
            }

            // Initialize transaction context
            using (var transaction = await _context.Database.BeginTransactionAsync())
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
                    if (project == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound(new { message = $"Project with code '{projectCode}' not found" });
                    }

                    await _projectDocumentRepository.UpdateArchitectureSectionAsync(projectCode, dto, currentUser);

                    // Process and upload attachments
                    foreach (var file in files)
                    {
                        var filePath = await _fileService.UploadFileAsync(file, project.Id, "Architecture");
                        
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

                    // Commit transaction if all operations succeed
                    await transaction.CommitAsync();

                    var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                    return Ok(MapToObject.MapToDto(updatedProject, _fileService));
                }
                catch (InvalidOperationException ex) // Captura de excepción de FileService
                {
                    // Rollback transaction on any error
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = ex.Message }); // Retorna el mensaje de validación personalizado
                }
                catch (Exception ex)
                {
                    // Rollback transaction on any error
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error actualizando la sección de arquitectura para el documento de proyecto {ProjectCode}", projectCode);
                    return BadRequest(new { message = DefaultErrorMessage });
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
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateGeneralSection(
            string projectCode, 
            [FromForm] UpdateProjectGeneralSectionDto dto)
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
                if (project == null)
                {
                    return NotFound(new { message = $"Project with code '{projectCode}' not found" });
                }

                await _projectDocumentRepository.UpdateGeneralSectionAsync(projectCode, dto, currentUser);

                var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updatedProject, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la sección general para el documento de proyecto {ProjectCode}", projectCode);
                return BadRequest(new { message = DefaultErrorMessage });
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
            [FromForm] IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "Se requiere al menos un archivo." });
            }

            // Initialize transaction context
            using (var transaction = await _context.Database.BeginTransactionAsync())
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
                    if (project == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound(new { message = $"Project with code '{projectCode}' not found" });
                    }

                    // Update UX Cases section
                    await _projectDocumentRepository.UpdateUxCasesSectionAsync(projectCode, dto, currentUser);

                    // Process and upload attachments
                    foreach (var file in files)
                    {
                        var filePath = await _fileService.UploadFileAsync(file, project.Id, "UxCases");
                        
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

                    // Commit transaction if all operations succeed
                    await transaction.CommitAsync();

                    var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                    return Ok(MapToObject.MapToDto(updatedProject, _fileService));
                }
                catch (InvalidOperationException ex) // Captura de excepción de FileService
                {
                    // Rollback transaction on any error
                    await transaction.RollbackAsync();
                    return BadRequest(new { message = ex.Message }); // Retorna el mensaje de validación personalizado
                }
                catch (Exception ex)
                {
                    // Rollback transaction on any error
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error actualizando la sección UX para el documento de proyecto {ProjectCode}. Transacción revertida.", projectCode);
                    return BadRequest(new { message = DefaultErrorMessage });
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
                if (project == null)
                {
                    return NotFound(new { message = $"Project with code '{projectCode}' not found" });
                }

                await _projectDocumentRepository.UpdateConstraintsSectionAsync(projectCode, dto, currentUser);

                var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updatedProject, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la sección de restricciones para el documento de proyecto {ProjectCode}", projectCode);
                return BadRequest(new { message = DefaultErrorMessage });
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
                if (project == null)
                {
                    return NotFound(new { message = $"Project with code '{projectCode}' not found" });
                }

                await _projectDocumentRepository.UpdateAreasIntegrationsSectionAsync(projectCode, dto, currentUser);

                var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updatedProject, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la sección de áreas e integraciones para el documento de proyecto {ProjectCode}", projectCode);
                return BadRequest(new { message = DefaultErrorMessage });
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
                if (project == null)
                {
                    return NotFound(new { message = $"Project with code '{projectCode}' not found" });
                }

                await _projectDocumentRepository.UpdateRaciSectionAsync(projectCode, dto, currentUser);

                var updatedProject = await _projectDocumentRepository.GetByProjectCodeDetailedAsync(projectCode);
                return Ok(MapToObject.MapToDto(updatedProject, _fileService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando la sección RACI para el documento de proyecto {ProjectCode}", projectCode);
                return BadRequest(new { message = DefaultErrorMessage });
            }
        }

    }
}