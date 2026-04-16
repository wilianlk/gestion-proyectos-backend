using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Utils
{
    public static class MapToObject
    {
        public static ProjectDocumentDto? MapToDto(ProjectDocument? project, IFileService fileService)
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
                Area = project.Area,
                Division = project.Division,
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
                Attachments = project.Attachments?.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    ProjectDocumentId = a.ProjectDocumentId,
                    Section = a.Section,
                    FileName = a.FileName,
                    FilePath = fileService.GetFileUrl(a.FilePath),
                    FileSize = a.FileSize,
                    ContentType = a.ContentType,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                }).ToList() ?? new List<AttachmentDto>(),
                Requirements = project.Requirements?.Select(a => new RequirementDto
                {
                    Id = a.Id,
                    ProjectDocumentId = a.ProjectDocumentId,
                    Code = a.Code,
                    Description = a.Description,
                    Type = a.Type,
                    Priority = a.Priority,
                    AcceptanceCriteria = a.AcceptanceCriteria
                }).ToList() ?? new List<RequirementDto>(),
                Integrations = project.Integrations?.Select(a => new IntegrationDto
                {
                    Id = a.Id,
                    ProjectDocumentId = a.ProjectDocumentId,
                    System = a.System,
                    Description = a.Description
                }).ToList() ?? new List<IntegrationDto>(),
                RaciActors = project.RaciActors?.Select(a => new RaciActorDto
                {
                    Id = a.Id,
                    ProjectDocumentId = a.ProjectDocumentId,
                    Activity = a.Activity,
                    Type = a.Type,
                    Area = a.Area,
                    Role = a.Role
                }).ToList() ?? new List<RaciActorDto>(),
                Risks = project.Risks?.Select(a => new RiskDto
                {
                    Id = a.Id,
                    ProjectDocumentId = a.ProjectDocumentId,
                    Risk = a.Risk,
                    Impact = a.Impact,
                    Probability = a.Probability,
                    Mitigation = a.Mitigation,
                    Owner = a.Owner
                }).ToList() ?? new List<RiskDto>(),
                TestCases = project.TestCases?.Select(a => new TestCaseDto
                {
                    Id = a.Id,
                    ProjectDocumentId = a.ProjectDocumentId,
                    TestStrategy = a.TestStrategy,
                    AcceptanceCriteria = a.AcceptanceCriteria,
                    DeployProductionCriteria = a.DeployProductionCriteria
                }).ToList() ?? new List<TestCaseDto>(),

                // Section Status
                GeneralSectionStatus = CalculateStatus.CalculateGeneralSectionStatus(project),
                ArchitectureSectionStatus = CalculateStatus.CalculateArchitectureSectionStatus(project),
                UxCasesSectionStatus = CalculateStatus.CalculateUxCasesSectionStatus(project),
                ConstraintsSectionStatus = CalculateStatus.CalculateConstraintsSectionStatus(project),
                AreasIntegrationsSectionStatus = CalculateStatus.CalculateAreasIntegrationsSectionStatus(project),
                RaciSectionStatus = CalculateStatus.CalculateRaciSectionStatus(project),
                RiskSectionStatus = CalculateStatus.CalculateRiskSectionStatus(project),
                TestCaseSectionStatus = CalculateStatus.CalculateTestCaseSectionStatus(project)
            };
        }
        
    }
}