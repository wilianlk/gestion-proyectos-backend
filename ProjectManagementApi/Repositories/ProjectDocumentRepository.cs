using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagementApi.Repositories
{
    public class ProjectDocumentRepository : InformixBaseRepository<ProjectDocument>, IProjectDocumentRepository<ProjectDocument>
    {
        private readonly ApplicationContext _context;

        public ProjectDocumentRepository(ApplicationContext context) : base(context)
        {
            _context = context;
        }

        /**
        * Description: Create a new project document (initial general section)
        * Input Parameters: 
        *      * dto (CreateProjectDocumentDto): Project document data
        *      * user (User): Current authenticated user from JWT token
        * Output Parameters: Created project document
        */
        public async Task<ProjectDocument> CreateAsync(CreateProjectDocumentDto dto, User user)
        {
            var entity = new ProjectDocument
            {
                // Temporary unique value required because ProjectCode is NOT NULL
                ProjectCode = $"TMP-{Guid.NewGuid():N}",
                ProjectName = StringSanitizer.SanitizeForInformix(dto.ProjectName),
                Sponsor = StringSanitizer.SanitizeForInformix(dto.Sponsor),
                FunctionalLead = StringSanitizer.SanitizeForInformix(dto.FunctionalLead),
                TechnicalLead = StringSanitizer.SanitizeForInformix(dto.TechnicalLead),
                DocumentStatus = StringSanitizer.SanitizeForInformix(dto.DocumentStatus),
                Area = StringSanitizer.SanitizeForInformix(dto.Area),
                Division = StringSanitizer.SanitizeForInformix(dto.Division),
                ProjectVision = StringSanitizer.SanitizeForInformix(dto.ProjectVision),
                GeneralObjective = StringSanitizer.SanitizeForInformix(dto.GeneralObjective),
                SpecificObjectives = StringSanitizer.SanitizeForInformix(dto.SpecificObjectives),
                Scope = StringSanitizer.SanitizeForInformix(dto.Scope),
                Exclusions = StringSanitizer.SanitizeForInformix(dto.Exclusions),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Identification = user.Identification,
                Username = user.Username,
                CreatedBy = user.Username
            };

            _context.ProjectDocuments.Add(entity);
            await _context.SaveChangesAsync();

            // Generate autoincremental code based on the DB identity
            entity.ProjectCode = $"PRJ-{entity.Id:D6}";
            await _context.SaveChangesAsync();

            return entity;
        }

        /**
        * Description: Get project document by project code
        * Input Parameters: 
        *      * projectCode (string): Project code to search
        * Output Parameters: Project document if found, null otherwise
        */
        public async Task<ProjectDocument?> GetByProjectCodeAsync(string projectCode)
        {
            return await _context.ProjectDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
        }

        /**
        * Description: Get project document by project code with all attachments
        * Input Parameters: 
        *      * projectCode (string): Project code to search
        * Output Parameters: Project document with attachments if found, null otherwise
        */
        public async Task<ProjectDocument?> GetByProjectCodeDetailedAsync(string projectCode)
        {
            var result = await _context.ProjectDocuments
                .AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Attachments)
                .Include(x => x.Requirements)
                .Include(x => x.Integrations)
                .Include(x => x.RaciActors)
                .Include(x => x.Risks)
                .Include(x => x.TestCases)
                .FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            return result;
        }

        /**
        * Description: Check if project code already exists
        * Input Parameters: 
        *      * projectCode (string): Project code to check
        * Output Parameters: True if exists, false otherwise
        */
        public async Task<bool> ExistsByProjectCodeAsync(string projectCode)
        {
            return await _context.ProjectDocuments.AnyAsync(x => x.ProjectCode == projectCode);
        }

        /**
        * Description: Update only general section fields
        * Input Parameters: 
        *      * projectCode (string): Project code to update
        *      * dto (UpdateProjectGeneralSectionDto): General section data
        * Output Parameters: None
        */
        public async Task UpdateGeneralSectionAsync(string projectCode, UpdateProjectGeneralSectionDto dto, User user)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == StringSanitizer.SanitizeForInformix(projectCode));
            if (entity != null)
            {
                entity.ProjectName = StringSanitizer.SanitizeForInformix(dto.ProjectName);
                entity.Sponsor = StringSanitizer.SanitizeForInformix(dto.Sponsor);
                entity.FunctionalLead = StringSanitizer.SanitizeForInformix(dto.FunctionalLead);
                entity.TechnicalLead = StringSanitizer.SanitizeForInformix(dto.TechnicalLead);
                entity.DocumentStatus = StringSanitizer.SanitizeForInformix(dto.DocumentStatus);
                entity.Area = StringSanitizer.SanitizeForInformix(dto.Area);
                entity.Division = StringSanitizer.SanitizeForInformix(dto.Division);
                entity.ProjectVision = StringSanitizer.SanitizeForInformix(dto.ProjectVision);
                entity.GeneralObjective = StringSanitizer.SanitizeForInformix(dto.GeneralObjective);
                entity.SpecificObjectives = StringSanitizer.SanitizeForInformix(dto.SpecificObjectives);
                entity.Scope = StringSanitizer.SanitizeForInformix(dto.Scope);
                entity.Exclusions = StringSanitizer.SanitizeForInformix(dto.Exclusions);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Identification = user.Identification;
                entity.Username = user.Username;

                await _context.SaveChangesAsync();
            }
        }

        /**
        * Description: Update only architecture section fields
        * Input Parameters: 
        *      * projectCode (string): Project code to update
        *      * dto (UpdateArchitectureSectionDto): Architecture section data
        * Output Parameters: None
        */
        public async Task UpdateArchitectureSectionAsync(string projectCode, UpdateArchitectureSectionDto dto, User user)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.SolutionDescription = StringSanitizer.SanitizeForInformix(dto.SolutionDescription);
                entity.SolutionType = StringSanitizer.SanitizeForInformix(dto.SolutionType);
                entity.DeploymentModel = StringSanitizer.SanitizeForInformix(dto.DeploymentModel);
                entity.SoftwareStack = StringSanitizer.SanitizeForInformix(dto.SoftwareStack);
                entity.HardwareArchitecture = StringSanitizer.SanitizeForInformix(dto.HardwareArchitecture);
                entity.SecurityControl = StringSanitizer.SanitizeForInformix(dto.SecurityControl);
                entity.ExpectedConcurrentUsers = dto.ExpectedConcurrentUsers;
                entity.SlaResponseTime = string.IsNullOrWhiteSpace(dto.SlaResponseTime)
                    ? null
                    : StringSanitizer.SanitizeForInformix(dto.SlaResponseTime);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Identification = user.Identification;
                entity.Username = user.Username;

                await _context.SaveChangesAsync();
            }
        }

        /**
        * Description: Update only UX Cases section fields
        * Input Parameters: 
        *      * projectCode (string): Project code to update
        *      * dto (UpdateUxCasesSectionDto): UX Cases section data
        * Output Parameters: None
        */
        public async Task UpdateUxCasesSectionAsync(string projectCode, UpdateUxCasesSectionDto dto, User user)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.UseCases = StringSanitizer.SanitizeForInformix(dto.UseCases);
                entity.RequiredDiagrams = StringSanitizer.SanitizeForInformix(dto.RequiredDiagrams);
                entity.ExperienceDesignMockups = StringSanitizer.SanitizeForInformix(dto.ExperienceDesignMockups);
                entity.TargetUsers = StringSanitizer.SanitizeForInformix(dto.TargetUsers);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Identification = user.Identification;
                entity.Username = user.Username;

                await _context.SaveChangesAsync();
            }
        }

        /**
        * Description: Update only Constraints section fields
        * Input Parameters: 
        *      * projectCode (string): Project code to update
        *      * dto (UpdateConstraintsSectionDto): Constraints section data
        * Output Parameters: None
        */
        public async Task UpdateConstraintsSectionAsync(string projectCode, UpdateConstraintsSectionDto dto, User user)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.EstimatedBudget = string.IsNullOrWhiteSpace(dto.EstimatedBudget)
                    ? null
                    : StringSanitizer.SanitizeForInformix(dto.EstimatedBudget);
                entity.TargetDate = dto.TargetDate;
                entity.TechnicalConstraints = StringSanitizer.SanitizeForInformix(dto.TechnicalConstraints);
                entity.BusinessConstraints = StringSanitizer.SanitizeForInformix(dto.BusinessConstraints);
                entity.RegulationsCompliance = StringSanitizer.SanitizeForInformix(dto.RegulationsCompliance);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Identification = user.Identification;
                entity.Username = user.Username;

                await _context.SaveChangesAsync();
            }
        }

        /**
        * Description: Update only Areas and Integrations section fields
        * Input Parameters: 
        *      * projectCode (string): Project code to update
        *      * dto (UpdateAreasIntegrationsSectionDto): Areas and Integrations section data
        * Output Parameters: None
        */
        public async Task UpdateAreasIntegrationsSectionAsync(string projectCode, UpdateAreasIntegrationsSectionDto dto, User user)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.InvolvedAreas = StringSanitizer.SanitizeForInformix(dto.InvolvedAreas);
                entity.OrganizationalImpact = StringSanitizer.SanitizeForInformix(dto.OrganizationalImpact);
                entity.MasterDataMigration = StringSanitizer.SanitizeForInformix(dto.MasterDataMigration);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Identification = user.Identification;
                entity.Username = user.Username;

                await _context.SaveChangesAsync();
            }
        }

        /**
        * Description: Update only RACI section fields
        * Input Parameters: 
        *      * projectCode (string): Project code to update
        *      * dto (UpdateRaciSectionDto): RACI section data
        * Output Parameters: None
        */
        public async Task UpdateRaciSectionAsync(string projectCode, UpdateRaciSectionDto dto, User user)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.ResponsibilitiesSummary = StringSanitizer.SanitizeForInformix(dto.ResponsibilitiesSummary);
                entity.ChangeManagementAdoption = StringSanitizer.SanitizeForInformix(dto.ChangeManagementAdoption);
                entity.OperationSupport = StringSanitizer.SanitizeForInformix(dto.OperationSupport);
                entity.UpdatedAt = DateTime.UtcNow;
                entity.Identification = user.Identification;
                entity.Username = user.Username;

                await _context.SaveChangesAsync();
            }
        }

        /**
        * Description: Get all project documents ordered by creation date (descending)
        * Input Parameters: None
        * Output Parameters: List of project documents
        */
        public async Task<List<ProjectDocument>> GetAllOrderedAsync()
        {
            return await _context.ProjectDocuments
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ProjectDocument
                {
                    Id = x.Id,
                    ProjectCode = x.ProjectCode,
                    ProjectName = x.ProjectName,
                    Sponsor = x.Sponsor,
                    TechnicalLead = x.TechnicalLead,
                    DocumentStatus = x.DocumentStatus,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }
    }
}
