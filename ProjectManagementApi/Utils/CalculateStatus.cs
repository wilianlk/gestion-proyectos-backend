using ProjectManagementApi.Models;

namespace ProjectManagementApi.Utils
{
    public static class CalculateStatus
    {
        public static string CalculateGeneralSectionStatus(ProjectDocument project)
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

        public static string CalculateArchitectureSectionStatus(ProjectDocument project)
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
            if (hasAttachments) filledFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields >= totalFields) return "Completo";
            return "Incompleto";
        }

        public static string CalculateUxCasesSectionStatus(ProjectDocument project)
        {
            int totalFields = 5;
            int filledFields = 0;

            if (!string.IsNullOrEmpty(project.UseCases)) filledFields++;
            if (!string.IsNullOrEmpty(project.RequiredDiagrams)) filledFields++;
            if (!string.IsNullOrEmpty(project.ExperienceDesignMockups)) filledFields++;
            if (!string.IsNullOrEmpty(project.TargetUsers)) filledFields++;

            bool hasAttachments = project.Attachments?.Any(a => a.Section == "UxCases") ?? false;
            if (hasAttachments) filledFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields >= totalFields) return "Completo";
            return "Incompleto";
        }

        public static string CalculateConstraintsSectionStatus(ProjectDocument project)
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

        public static string CalculateAreasIntegrationsSectionStatus(ProjectDocument project)
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

        public static string CalculateRaciSectionStatus(ProjectDocument project)
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