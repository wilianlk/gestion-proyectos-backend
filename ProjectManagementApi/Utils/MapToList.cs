using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Utils
{
    public static class MapToList
    {
        public static ProjectDocumentListDto MapToListDto(ProjectDocument project)
        {
            return new ProjectDocumentListDto
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
                UseCases = project.UseCases,
                RequiredDiagrams = project.RequiredDiagrams,
                ExperienceDesignMockups = project.ExperienceDesignMockups,
                TargetUsers = project.TargetUsers,
                EstimatedBudget = project.EstimatedBudget,
                TargetDate = project.TargetDate,
                TechnicalConstraints = project.TechnicalConstraints,
                BusinessConstraints = project.BusinessConstraints,
                RegulationsCompliance = project.RegulationsCompliance,
                InvolvedAreas = project.InvolvedAreas,
                OrganizationalImpact = project.OrganizationalImpact,
                MasterDataMigration = project.MasterDataMigration,
                ResponsibilitiesSummary = project.ResponsibilitiesSummary,
                ChangeManagementAdoption = project.ChangeManagementAdoption,
                OperationSupport = project.OperationSupport,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                IsActive = project.IsActive
            };
        }
    }
    
}