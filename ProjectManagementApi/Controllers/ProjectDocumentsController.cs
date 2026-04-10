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
    public class ProjectDocumentsController : ControllerBase
    {
        private readonly IProjectDocumentRepository<ProjectDocument> _projectDocumentRepository;
        private readonly IProjectDocumentAttachmentRepository<ProjectDocumentAttachment> _attachmentRepository;
        private readonly IFileService _fileService;

        public ProjectDocumentsController(
            IProjectDocumentRepository<ProjectDocument> projectDocumentRepository,
            IProjectDocumentAttachmentRepository<ProjectDocumentAttachment> attachmentRepository,
            IFileService fileService)
        {
            _projectDocumentRepository = projectDocumentRepository;
            _attachmentRepository = attachmentRepository;
            _fileService = fileService;
        }

        /// <summary>
        /// Method to create a new project document with general section (must be created first before other sections)
        /// </summary>
        /// <param name="dto">Project document data with general section fields</param>
        /// <returns>Created project document</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProjectDocument>> Create([FromBody] CreateProjectDocumentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _projectDocumentRepository.ExistsByProjectCodeAsync(dto.ProjectCode))
            {
                return BadRequest(new { message = $"Project code '{dto.ProjectCode}' already exists" });
            }

            var created = await _projectDocumentRepository.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByProjectCode), new { projectCode = created.ProjectCode }, created);
        }

        /// <summary>
        /// Method to get project by project code with all fields and attachments
        /// </summary>
        /// <param name="projectCode">Project code to search</param>
        /// <returns>Project with all sections and attachments grouped by section</returns>
        [HttpGet("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProjectDocumentDto>> GetByProjectCode(string projectCode)
        {
            var project = await _projectDocumentRepository.GetByProjectCodeWithAttachmentsAsync(StringSanitizer.SanitizeForInformix(projectCode));
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{projectCode}' not found" });
            }

            var result = MapToObject.MapToDto(project, _fileService);
            return Ok(result);
        }

        /// <summary>
        /// Method to get all project documents ordered by creation date (descending)
        /// </summary>
        /// <returns>List of project documents with section status</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProjectDocumentListDto>>> GetAll()
        {
            var projects = await _projectDocumentRepository.GetAllOrderedAsync();
            var result = projects.Select(MapToList.MapToListDto).ToList();
            return Ok(result);
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
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateArchitectureSection(
            string projectCode, 
            [FromForm] UpdateArchitectureSectionDto dto,
            [FromForm] IFormFileCollection? files)
        {
            var project = await _projectDocumentRepository.GetByProjectCodeAsync(StringSanitizer.SanitizeForInformix(projectCode));
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{projectCode}' not found" });
            }

            await _projectDocumentRepository.UpdateArchitectureSectionAsync(projectCode, dto);

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    var filePath = await _fileService.UploadFileAsync(file, project.Id, "Architecture");
                    
                    await _attachmentRepository.CreateAttachmentAsync(new ProjectDocumentAttachmentDto
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

            var updatedProject = await _projectDocumentRepository.GetByProjectCodeWithAttachmentsAsync(projectCode);
            return Ok(MapToObject.MapToDto(updatedProject, _fileService));
        }

        /// <summary>
        /// Method to update general section fields and upload attachments in a single request
        /// </summary>
        /// <param name="projectCode">Project code to update</param>
        /// <param name="dto">General section data</param>
        /// <param name="files">Optional attachments to upload</param>
        /// <returns>Updated project with attachments</returns>
        [HttpPut("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateGeneralSection(
            string projectCode, 
            [FromForm] UpdateProjectGeneralSectionDto dto,
            [FromForm] IFormFileCollection? files)
        {
            var project = await _projectDocumentRepository.GetByProjectCodeAsync(projectCode);
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{projectCode}' not found" });
            }

            await _projectDocumentRepository.UpdateGeneralSectionAsync(projectCode, dto);

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    var filePath = await _fileService.UploadFileAsync(file, project.Id, "General");
                    
                    await _attachmentRepository.CreateAttachmentAsync(new ProjectDocumentAttachmentDto
                    {
                        ProjectDocumentId = project.Id,
                        Section = "General",
                        FileName = file.FileName,
                        FilePath = filePath,
                        FileSize = file.Length,
                        ContentType = file.ContentType
                    });
                }
            }

            var updatedProject = await _projectDocumentRepository.GetByProjectCodeWithAttachmentsAsync(projectCode);
            return Ok(MapToObject.MapToDto(updatedProject, _fileService));
        }

        /// <summary>
        /// Method to update UX Cases section fields and upload attachments in a single request
        /// </summary>
        /// <param name="projectCode">Project code to update</param>
        /// <param name="dto">UX Cases section data</param>
        /// <param name="files">Optional attachments to upload</param>
        /// <returns>Updated project with attachments</returns>
        [HttpPut("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProjectDocumentDto>> UpdateUxCasesSection(
            string projectCode, 
            [FromForm] UpdateUxCasesSectionDto dto,
            [FromForm] IFormFileCollection? files)
        {
            var project = await _projectDocumentRepository.GetByProjectCodeAsync(projectCode);
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{projectCode}' not found" });
            }

            await _projectDocumentRepository.UpdateUxCasesSectionAsync(projectCode, dto);

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    var filePath = await _fileService.UploadFileAsync(file, project.Id, "UxCases");
                    
                    await _attachmentRepository.CreateAttachmentAsync(new ProjectDocumentAttachmentDto
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

            var updatedProject = await _projectDocumentRepository.GetByProjectCodeWithAttachmentsAsync(projectCode);
            return Ok(MapToObject.MapToDto(updatedProject, _fileService));
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
        public async Task<ActionResult<ProjectDocumentDto>> UpdateConstraintsSection(
            string projectCode, 
            [FromBody] UpdateConstraintsSectionDto dto)
        {
            var project = await _projectDocumentRepository.GetByProjectCodeAsync(projectCode);
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{projectCode}' not found" });
            }

            await _projectDocumentRepository.UpdateConstraintsSectionAsync(projectCode, dto);

            var updatedProject = await _projectDocumentRepository.GetByProjectCodeWithAttachmentsAsync(projectCode);
            return Ok(MapToObject.MapToDto(updatedProject, _fileService));
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
        public async Task<ActionResult<ProjectDocumentDto>> UpdateAreasIntegrationsSection(
            string projectCode, 
            [FromBody] UpdateAreasIntegrationsSectionDto dto)
        {
            var project = await _projectDocumentRepository.GetByProjectCodeAsync(projectCode);
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{projectCode}' not found" });
            }

            await _projectDocumentRepository.UpdateAreasIntegrationsSectionAsync(projectCode, dto);

            var updatedProject = await _projectDocumentRepository.GetByProjectCodeWithAttachmentsAsync(projectCode);
            return Ok(MapToObject.MapToDto(updatedProject, _fileService));
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
        public async Task<ActionResult<ProjectDocumentDto>> UpdateRaciSection(
            string projectCode, 
            [FromBody] UpdateRaciSectionDto dto)
        {
            var project = await _projectDocumentRepository.GetByProjectCodeAsync(projectCode);
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{projectCode}' not found" });
            }

            await _projectDocumentRepository.UpdateRaciSectionAsync(projectCode, dto);

            var updatedProject = await _projectDocumentRepository.GetByProjectCodeWithAttachmentsAsync(projectCode);
            return Ok(MapToObject.MapToDto(updatedProject, _fileService));
        }

    }
}