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
            using var transaction = await _context.Database.BeginTransactionAsync();

            var entity = new ProjectDocument
            {
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

            entity.ProjectCode = $"PRJ-{entity.Id:D6}";
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

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
            var project = await _context.ProjectDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProjectCode == projectCode);

            if (project == null)
            {
                return null;
            }

            // Load child collections independently to avoid a large cartesian join
            // when a project has many attachments and matrix rows.
            project.Attachments = await _context.Attachments
                .AsNoTracking()
                .Where(x => x.ProjectDocumentId == project.Id)
                .OrderBy(x => x.Id)
                .ToListAsync();

            project.Requirements = await _context.ProjectDocumentRequirements
                .AsNoTracking()
                .Where(x => x.ProjectDocumentId == project.Id)
                .OrderBy(x => x.Id)
                .ToListAsync();

            project.Integrations = await _context.ProjectDocumentIntegrations
                .AsNoTracking()
                .Where(x => x.ProjectDocumentId == project.Id)
                .OrderBy(x => x.Id)
                .ToListAsync();

            project.RaciActors = await _context.ProjectDocumentRaciActors
                .AsNoTracking()
                .Where(x => x.ProjectDocumentId == project.Id)
                .OrderBy(x => x.Id)
                .ToListAsync();

            project.Risks = await _context.ProjectDocumentRisks
                .AsNoTracking()
                .Where(x => x.ProjectDocumentId == project.Id)
                .OrderBy(x => x.Id)
                .ToListAsync();

            project.TestCases = await _context.ProjectDocumentTestCases
                .AsNoTracking()
                .Where(x => x.ProjectDocumentId == project.Id)
                .OrderBy(x => x.Id)
                .ToListAsync();

            return project;
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

        public async Task<Dictionary<int, ProjectDocument>> GetDetailedByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0)
                return new Dictionary<int, ProjectDocument>();

            return await _context.ProjectDocuments
                .AsNoTracking()
                .Where(x => idList.Contains(x.Id))
                .Include(x => x.Attachments)
                .Include(x => x.Requirements)
                .Include(x => x.Integrations)
                .Include(x => x.RaciActors)
                .Include(x => x.Risks)
                .Include(x => x.TestCases)
                .ToDictionaryAsync(x => x.Id);
        }

        public async Task<Dictionary<int, ProjectDocumentCompletionSnapshot>> GetCompletionSnapshotsByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0)
            {
                return new Dictionary<int, ProjectDocumentCompletionSnapshot>();
            }

            return await _context.ProjectDocuments
                .AsNoTracking()
                .Where(x => idList.Contains(x.Id))
                .Select(x => new ProjectDocumentCompletionSnapshot
                {
                    Id = x.Id,
                    ProjectName = x.ProjectName,
                    Sponsor = x.Sponsor,
                    FunctionalLead = x.FunctionalLead,
                    TechnicalLead = x.TechnicalLead,
                    DocumentStatus = x.DocumentStatus,
                    ProjectVision = x.ProjectVision,
                    GeneralObjective = x.GeneralObjective,
                    SpecificObjectives = x.SpecificObjectives,
                    Scope = x.Scope,
                    Exclusions = x.Exclusions,
                    SolutionDescription = x.SolutionDescription,
                    SoftwareStack = x.SoftwareStack,
                    SecurityControl = x.SecurityControl,
                    HardwareArchitecture = x.HardwareArchitecture,
                    HasArchitectureAttachment = x.Attachments.Any(a =>
                        a.Section != null &&
                        a.Section.Trim().ToLower() == "architecture"),
                    HasUseCasesAttachment = x.Attachments.Any(a =>
                        a.Section == "UxCasesUseCases" || a.Section == "UxCases"),
                    HasUxAttachment = x.Attachments.Any(a =>
                        a.Section == "UxCasesExperienceDesignMockups" || a.Section == "UxCases"),
                    EstimatedBudget = x.EstimatedBudget,
                    TargetDate = x.TargetDate,
                    TechnicalConstraints = x.TechnicalConstraints,
                    BusinessConstraints = x.BusinessConstraints,
                    RegulationsCompliance = x.RegulationsCompliance,
                    HasCompleteRaciActor = x.RaciActors.Any(actor =>
                        actor.Activity != null && actor.Activity != "" &&
                        actor.Type != null && actor.Type != "" &&
                        actor.Area != null && actor.Area != "" &&
                        actor.Role != null && actor.Role != ""),
                    HasAnyRequirementData = x.Requirements.Any(r =>
                        (r.Code != null && r.Code != "") ||
                        (r.Description != null && r.Description != "") ||
                        (r.Type != null && r.Type != "") ||
                        (r.Priority != null && r.Priority != "") ||
                        (r.AcceptanceCriteria != null && r.AcceptanceCriteria != "")),
                    HasCompleteRequirement = x.Requirements.Any(r =>
                        r.Code != null && r.Code != "" &&
                        r.Description != null && r.Description != "" &&
                        r.Type != null && r.Type != "" &&
                        r.Priority != null && r.Priority != "" &&
                        r.AcceptanceCriteria != null && r.AcceptanceCriteria != ""),
                    HasAnyIntegrationData = x.Integrations.Any(i =>
                        (i.System != null && i.System != "") ||
                        (i.Description != null && i.Description != "")),
                    HasCompleteIntegration = x.Integrations.Any(i =>
                        i.System != null && i.System != "" &&
                        i.Description != null && i.Description != ""),
                    HasCompleteRisk = x.Risks.Any(r =>
                        r.Risk != null && r.Risk != "" &&
                        r.Impact != null && r.Impact != "" &&
                        r.Probability != null && r.Probability != "" &&
                        r.Mitigation != null && r.Mitigation != "" &&
                        r.Owner != null && r.Owner != ""),
                    HasCompleteTestCase = x.TestCases.Any(t =>
                        t.TestStrategy != null && t.TestStrategy != "" &&
                        t.AcceptanceCriteria != null && t.AcceptanceCriteria != "" &&
                        t.DeployProductionCriteria != null && t.DeployProductionCriteria != "")
                })
                .ToDictionaryAsync(x => x.Id);
        }

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
                    CreatedAt = x.CreatedAt,
                    Identification = x.Identification,
                    Username = x.Username,
                    CreatedBy = x.CreatedBy
                })
                .ToListAsync();
        }
    }
}
