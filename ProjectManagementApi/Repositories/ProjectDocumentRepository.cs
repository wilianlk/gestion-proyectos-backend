using DatabasesLib;
using ProjectManagementApi.Context;
using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Utils.Helpers;
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
        * Output Parameters: Created project document
        */
        public async Task<ProjectDocument> CreateAsync(CreateProjectDocumentDto dto)
        {
            var entity = new ProjectDocument
            {
                ProjectCode = StringSanitizer.SanitizeForInformix(dto.ProjectCode),
                ProjectName = StringSanitizer.SanitizeForInformix(dto.ProjectName),
                Sponsor = StringSanitizer.SanitizeForInformix(dto.Sponsor),
                FunctionalLead = StringSanitizer.SanitizeForInformix(dto.FunctionalLead),
                TechnicalLead = StringSanitizer.SanitizeForInformix(dto.TechnicalLead),
                DocumentStatus = StringSanitizer.SanitizeForInformix(dto.DocumentStatus),
                ProjectVision = StringSanitizer.SanitizeForInformix(dto.ProjectVision),
                GeneralObjective = StringSanitizer.SanitizeForInformix(dto.GeneralObjective),
                SpecificObjectives = StringSanitizer.SanitizeForInformix(dto.SpecificObjectives),
                ExpectedValue = StringSanitizer.SanitizeForInformix(dto.ExpectedValue),
                Scope = StringSanitizer.SanitizeForInformix(dto.Scope),
                Exclusions = StringSanitizer.SanitizeForInformix(dto.Exclusions),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                // TODO: Add user context to get actual username instead of hardcoding
                Identification = "1234567890",
                Username = "dev"
            };

            _context.ProjectDocuments.Add(entity);
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
                .FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
        }

        /**
        * Description: Get project document by project code with all attachments
        * Input Parameters: 
        *      * projectCode (string): Project code to search
        * Output Parameters: Project document with attachments if found, null otherwise
        */
        public async Task<ProjectDocument?> GetByProjectCodeWithAttachmentsAsync(string projectCode)
        {
            return await _context.ProjectDocuments
                .Include(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
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
        public async Task UpdateGeneralSectionAsync(string projectCode, UpdateProjectGeneralSectionDto dto)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == StringSanitizer.SanitizeForInformix(projectCode));
            if (entity != null)
            {
                entity.ProjectName = StringSanitizer.SanitizeForInformix(dto.ProjectName);
                entity.Sponsor = StringSanitizer.SanitizeForInformix(dto.Sponsor);
                entity.FunctionalLead = StringSanitizer.SanitizeForInformix(dto.FunctionalLead);
                entity.TechnicalLead = StringSanitizer.SanitizeForInformix(dto.TechnicalLead);
                entity.DocumentStatus = StringSanitizer.SanitizeForInformix(dto.DocumentStatus);
                entity.ProjectVision = StringSanitizer.SanitizeForInformix(dto.ProjectVision);
                entity.GeneralObjective = StringSanitizer.SanitizeForInformix(dto.GeneralObjective);
                entity.SpecificObjectives = StringSanitizer.SanitizeForInformix(dto.SpecificObjectives);
                entity.ExpectedValue = StringSanitizer.SanitizeForInformix(dto.ExpectedValue);
                entity.Scope = StringSanitizer.SanitizeForInformix(dto.Scope);
                entity.Exclusions = StringSanitizer.SanitizeForInformix(dto.Exclusions);
                entity.UpdatedAt = DateTime.UtcNow;
                // TODO: Update user context to get actual username instead of hardcoding
                entity.Identification = "1234567890";
                entity.Username = "dev";

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
        public async Task UpdateArchitectureSectionAsync(string projectCode, UpdateArchitectureSectionDto dto)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.SolutionDescription = dto.SolutionDescription;
                entity.SolutionType = dto.SolutionType;
                entity.DeploymentModel = dto.DeploymentModel;
                entity.SoftwareStack = dto.SoftwareStack;
                entity.HardwareArchitecture = dto.HardwareArchitecture;
                entity.SecurityControl = dto.SecurityControl;
                entity.ExpectedConcurrentUsers = dto.ExpectedConcurrentUsers;
                entity.SlaResponseTime = dto.SlaResponseTime;
                entity.UpdatedAt = DateTime.UtcNow;
                // TODO: Update user context to get actual username instead of hardcoding
                entity.Identification = "1234567890";
                entity.Username = "dev";

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
        public async Task UpdateUxCasesSectionAsync(string projectCode, UpdateUxCasesSectionDto dto)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.UseCases = dto.UseCases;
                entity.RequiredDiagrams = dto.RequiredDiagrams;
                entity.ExperienceDesignMockups = dto.ExperienceDesignMockups;
                entity.TargetUsers = dto.TargetUsers;
                entity.UpdatedAt = DateTime.UtcNow;
                // TODO: Update user context to get actual username instead of hardcoding
                entity.Identification = "1234567890";
                entity.Username = "dev";

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
        public async Task UpdateConstraintsSectionAsync(string projectCode, UpdateConstraintsSectionDto dto)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.EstimatedBudget = dto.EstimatedBudget;
                entity.TargetDate = dto.TargetDate;
                entity.TechnicalConstraints = dto.TechnicalConstraints;
                entity.BusinessConstraints = dto.BusinessConstraints;
                entity.RegulationsCompliance = dto.RegulationsCompliance;
                entity.UpdatedAt = DateTime.UtcNow;
                // TODO: Update user context to get actual username instead of hardcoding
                entity.Identification = "1234567890";
                entity.Username = "dev";

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
        public async Task UpdateAreasIntegrationsSectionAsync(string projectCode, UpdateAreasIntegrationsSectionDto dto)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.InvolvedAreas = dto.InvolvedAreas;
                entity.OrganizationalImpact = dto.OrganizationalImpact;
                entity.MasterDataMigration = dto.MasterDataMigration;
                entity.UpdatedAt = DateTime.UtcNow;
                // TODO: Update user context to get actual username instead of hardcoding
                entity.Identification = "1234567890";
                entity.Username = "dev";

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
        public async Task UpdateRaciSectionAsync(string projectCode, UpdateRaciSectionDto dto)
        {
            var entity = await _context.ProjectDocuments.FirstOrDefaultAsync(x => x.ProjectCode == projectCode);
            if (entity != null)
            {
                entity.ResponsibilitiesSummary = dto.ResponsibilitiesSummary;
                entity.ChangeManagementAdoption = dto.ChangeManagementAdoption;
                entity.OperationSupport = dto.OperationSupport;
                entity.UpdatedAt = DateTime.UtcNow;
                // TODO: Update user context to get actual username instead of hardcoding
                entity.Identification = "1234567890";
                entity.Username = "dev";

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
                .Include(x => x.Attachments)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}