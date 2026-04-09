using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils.Helpers;

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
            var project = await _projectDocumentRepository.GetByProjectCodeWithAttachmentsAsync(projectCode);
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{projectCode}' not found" });
            }

            var result = new ProjectDocumentDto
            {
                Id = project.Id,
                ProjectCode = project.ProjectCode,
                ProjectName = project.ProjectName,
                Sponsor = project.Sponsor,
                FunctionalLead = project.FunctionalLead,
                TechnicalLead = project.TechnicalLead,
                DocumentStatus = project.DocumentStatus,
                ProjectVision = project.ProjectVision,
                GeneralObjective = project.GeneralObjective,
                SpecificObjectives = project.SpecificObjectives,
                ExpectedValue = project.ExpectedValue,
                Scope = project.Scope,
                Exclusions = project.Exclusions,
                SolutionDescription = project.SolutionDescription,
                SolutionType = project.SolutionType,
                DeploymentModel = project.DeploymentModel,
                SoftwareStack = project.SoftwareStack,
                HardwareArchitecture = project.HardwareArchitecture,
                SecurityControl = project.SecurityControl,
                ExpectedConcurrentUsers = project.ExpectedConcurrentUsers,
                SlaResponseTime = project.SlaResponseTime,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                Attachments = project.Attachments?.Select(a => new ProjectDocumentAttachmentDto
                {
                    Id = a.Id,
                    ProjectDocumentId = a.ProjectDocumentId,
                    Section = a.Section,
                    FileName = a.FileName,
                    FilePath = _fileService.GetFileUrl(a.FilePath),
                    FileSize = a.FileSize,
                    ContentType = a.ContentType,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList() ?? new List<ProjectDocumentAttachmentDto>()
            };

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
            return Ok(MapToDto(updatedProject));
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
            return Ok(MapToDto(updatedProject));
        }

        /// <summary>
        /// Method to upload multiple attachments for a project section
        /// </summary>
        /// <param name="projectCode">Project code</param>
        /// <param name="section">Section name (General, Architecture, etc.)</param>
        /// <param name="files">Files to upload</param>
        /// <returns>List of created attachments</returns>
        [HttpPost("{projectCode}/[action]")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ProjectDocumentAttachmentDto>>> UploadAttachments(
            string projectCode, 
            [FromForm] string section, 
            [FromForm] IFormFileCollection files)
        {
            var project = await _projectDocumentRepository.GetByProjectCodeAsync(projectCode);
            if (project == null)
            {
                return BadRequest(new { message = $"Project with code '{projectCode}' not found" });
            }

            if (files == null || files.Count == 0)
            {
                return BadRequest(new { message = "No files provided" });
            }

            var attachments = new List<ProjectDocumentAttachmentDto>();
            
            foreach (var file in files)
            {
                var filePath = await _fileService.UploadFileAsync(file, project.Id, section);
                
                var attachment = await _attachmentRepository.CreateAttachmentAsync(new ProjectDocumentAttachmentDto
                {
                    ProjectDocumentId = project.Id,
                    Section = section,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    ContentType = file.ContentType
                });

                attachments.Add(new ProjectDocumentAttachmentDto
                {
                    Id = attachment.Id,
                    ProjectDocumentId = attachment.ProjectDocumentId,
                    Section = attachment.Section,
                    FileName = attachment.FileName,
                    FilePath = _fileService.GetFileUrl(attachment.FilePath),
                    FileSize = attachment.FileSize,
                    ContentType = attachment.ContentType
                });
            }

            return CreatedAtAction(nameof(GetByProjectCode), new { projectCode }, attachments);
        }

        /// <summary>
        /// Method to delete an attachment
        /// </summary>
        /// <param name="attachmentId">Attachment id to delete</param>
        /// <returns>Success or failure</returns>
        [HttpDelete("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var allAttachments = await _attachmentRepository.GetAllAsync();
            var attachment = allAttachments.FirstOrDefault(a => a.Id == attachmentId);
            
            if (attachment == null)
            {
                return NotFound(new { message = $"Attachment with id '{attachmentId}' not found" });
            }

            await _fileService.DeleteFileAsync(attachment.FilePath);
            await _attachmentRepository.DeleteAttachmentAsync(attachmentId);

            return Ok(new { message = "Attachment deleted successfully" });
        }

        private ProjectDocumentDto MapToDto(ProjectDocument? project)
        {
            if (project == null) return null;

            return new ProjectDocumentDto
            {
                Id = project.Id,
                ProjectCode = project.ProjectCode,
                ProjectName = project.ProjectName,
                Sponsor = project.Sponsor,
                FunctionalLead = project.FunctionalLead,
                TechnicalLead = project.TechnicalLead,
                DocumentStatus = project.DocumentStatus,
                ProjectVision = project.ProjectVision,
                GeneralObjective = project.GeneralObjective,
                SpecificObjectives = project.SpecificObjectives,
                ExpectedValue = project.ExpectedValue,
                Scope = project.Scope,
                Exclusions = project.Exclusions,
                SolutionDescription = project.SolutionDescription,
                SolutionType = project.SolutionType,
                DeploymentModel = project.DeploymentModel,
                SoftwareStack = project.SoftwareStack,
                HardwareArchitecture = project.HardwareArchitecture,
                SecurityControl = project.SecurityControl,
                ExpectedConcurrentUsers = project.ExpectedConcurrentUsers,
                SlaResponseTime = project.SlaResponseTime,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                Attachments = project.Attachments?.Select(a => new ProjectDocumentAttachmentDto
                {
                    Id = a.Id,
                    ProjectDocumentId = a.ProjectDocumentId,
                    Section = a.Section,
                    FileName = a.FileName,
                    FilePath = _fileService.GetFileUrl(a.FilePath),
                    FileSize = a.FileSize,
                    ContentType = a.ContentType,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList() ?? new List<ProjectDocumentAttachmentDto>()
            };
        }
    }
}