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
            var project = await _projectDocumentRepository.GetByProjectCodeWithAttachmentsAsync(StringSanitizer.SanitizeForInformix(projectCode));
            if (project == null)
            {
                return NotFound(new { message = $"Project with code '{projectCode}' not found" });
            }

            var result = MapToDto(project);
            
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
            return Ok(MapToDto(updatedProject));
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
            return Ok(MapToDto(updatedProject));
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
            return Ok(MapToDto(updatedProject));
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
                // Casos y UX
                UseCases = project.UseCases,
                RequiredDiagrams = project.RequiredDiagrams,
                ExperienceDesignMockups = project.ExperienceDesignMockups,
                TargetUsers = project.TargetUsers,
                // Restricciones
                EstimatedBudget = project.EstimatedBudget,
                TargetDate = project.TargetDate,
                TechnicalConstraints = project.TechnicalConstraints,
                BusinessConstraints = project.BusinessConstraints,
                RegulationsCompliance = project.RegulationsCompliance,
                // Áreas e Integraciones
                InvolvedAreas = project.InvolvedAreas,
                OrganizationalImpact = project.OrganizationalImpact,
                MasterDataMigration = project.MasterDataMigration,
                // RACI
                ResponsibilitiesSummary = project.ResponsibilitiesSummary,
                ChangeManagementAdoption = project.ChangeManagementAdoption,
                OperationSupport = project.OperationSupport,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                IsActive = project.IsActive,
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
                }).ToList() ?? new List<ProjectDocumentAttachmentDto>(),

                // Section Status
                GeneralSectionStatus = CalculateGeneralSectionStatus(project),
                ArchitectureSectionStatus = CalculateArchitectureSectionStatus(project),
                UxCasesSectionStatus = CalculateUxCasesSectionStatus(project),
                ConstraintsSectionStatus = CalculateConstraintsSectionStatus(project),
                AreasIntegrationsSectionStatus = CalculateAreasIntegrationsSectionStatus(project),
                RaciSectionStatus = CalculateRaciSectionStatus(project)
            };
        }

        private string CalculateGeneralSectionStatus(ProjectDocument project)
        {
            int totalFields = 11;
            int filledFields = 0;

            if (!string.IsNullOrEmpty(project.ProjectName)) filledFields++;
            if (!string.IsNullOrEmpty(project.Sponsor)) filledFields++;
            if (!string.IsNullOrEmpty(project.FunctionalLead)) filledFields++;
            if (!string.IsNullOrEmpty(project.TechnicalLead)) filledFields++;
            if (!string.IsNullOrEmpty(project.DocumentStatus)) filledFields++;
            if (!string.IsNullOrEmpty(project.ProjectVision)) filledFields++;
            if (!string.IsNullOrEmpty(project.GeneralObjective)) filledFields++;
            if (!string.IsNullOrEmpty(project.SpecificObjectives)) filledFields++;
            if (!string.IsNullOrEmpty(project.ExpectedValue)) filledFields++;
            if (!string.IsNullOrEmpty(project.Scope)) filledFields++;
            if (!string.IsNullOrEmpty(project.Exclusions)) filledFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields == totalFields) return "Completo";
            return "Incompleto";
        }

        private string CalculateArchitectureSectionStatus(ProjectDocument project)
        {
            int totalFields = 9;
            int filledFields = 0;

            if (!string.IsNullOrEmpty(project.SolutionDescription)) filledFields++;
            if (!string.IsNullOrEmpty(project.SolutionType)) filledFields++;
            if (!string.IsNullOrEmpty(project.DeploymentModel)) filledFields++;
            if (!string.IsNullOrEmpty(project.SoftwareStack)) filledFields++;
            if (!string.IsNullOrEmpty(project.HardwareArchitecture)) filledFields++;
            if (!string.IsNullOrEmpty(project.SecurityControl)) filledFields++;
            if (project.ExpectedConcurrentUsers.HasValue) filledFields++;
            if (!string.IsNullOrEmpty(project.SlaResponseTime)) filledFields++;

            bool hasAttachments = project.Attachments?.Any(a => a.Section == "Architecture") ?? false;
            if (!hasAttachments) totalFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields >= totalFields) return "Completo";
            return "Incompleto";
        }

        private string CalculateUxCasesSectionStatus(ProjectDocument project)
        {
            int totalFields = 5;
            int filledFields = 0;

            if (!string.IsNullOrEmpty(project.UseCases)) filledFields++;
            if (!string.IsNullOrEmpty(project.RequiredDiagrams)) filledFields++;
            if (!string.IsNullOrEmpty(project.ExperienceDesignMockups)) filledFields++;
            if (!string.IsNullOrEmpty(project.TargetUsers)) filledFields++;

            bool hasAttachments = project.Attachments?.Any(a => a.Section == "UxCases") ?? false;
            if (!hasAttachments) totalFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields >= totalFields) return "Completo";
            return "Incompleto";
        }

        private string CalculateConstraintsSectionStatus(ProjectDocument project)
        {
            int totalFields = 5;
            int filledFields = 0;

            if (!string.IsNullOrEmpty(project.EstimatedBudget)) filledFields++;
            if (project.TargetDate.HasValue) filledFields++;
            if (!string.IsNullOrEmpty(project.TechnicalConstraints)) filledFields++;
            if (!string.IsNullOrEmpty(project.BusinessConstraints)) filledFields++;
            if (!string.IsNullOrEmpty(project.RegulationsCompliance)) filledFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields == totalFields) return "Completo";
            return "Incompleto";
        }

        private string CalculateAreasIntegrationsSectionStatus(ProjectDocument project)
        {
            int totalFields = 3;
            int filledFields = 0;

            if (!string.IsNullOrEmpty(project.InvolvedAreas)) filledFields++;
            if (!string.IsNullOrEmpty(project.OrganizationalImpact)) filledFields++;
            if (!string.IsNullOrEmpty(project.MasterDataMigration)) filledFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields == totalFields) return "Completo";
            return "Incompleto";
        }

        private string CalculateRaciSectionStatus(ProjectDocument project)
        {
            int totalFields = 3;
            int filledFields = 0;

            if (!string.IsNullOrEmpty(project.ResponsibilitiesSummary)) filledFields++;
            if (!string.IsNullOrEmpty(project.ChangeManagementAdoption)) filledFields++;
            if (!string.IsNullOrEmpty(project.OperationSupport)) filledFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields == totalFields) return "Completo";
            return "Incompleto";
        }
    }
}