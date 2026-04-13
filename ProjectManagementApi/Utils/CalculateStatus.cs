using ProjectManagementApi.Models;

namespace ProjectManagementApi.Utils
{
    public static class CalculateStatus
    {
        /// <summary>
        /// Description: Calculate the completion status of the General section
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing general section fields
        /// Output Parameters: Status string - "Pendiente" if no fields filled, "Completo" if all fields filled, "Incomplete" otherwise
        /// </summary>
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

        /// <summary>
        /// Description: Calculate the completion status of the Architecture section
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing architecture section fields
        /// Output Parameters: Status string - "Pendiente" if no fields filled, "Completo" if all fields filled, "Incompleto" otherwise
        /// </summary>
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

        /// <summary>
        /// Description: Calculate the completion status of the UX Cases section
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing UX cases section fields
        /// Output Parameters: Status string - "Pendiente" if no fields filled, "Completo" if all fields filled, "Incompleto" otherwise
        /// </summary>
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

        /// <summary>
        /// Description: Calculate the completion status of the Constraints section
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing constraints section fields
        /// Output Parameters: Status string - "Pendiente" if no fields filled, "Completo" if all fields filled, "Incompleto" otherwise
        /// </summary>
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

        /// <summary>
        /// Description: Calculate the completion status of the Areas Integrations section
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing areas integrations section fields
        /// Output Parameters: Status string - "Pendiente" if no fields filled, "Completo" if all fields filled, "Incompleto" otherwise
        /// </summary>
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

        /// <summary>
        /// Description: Calculate the completion status of the RACI section
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing RACI section fields
        /// Output Parameters: Status string - "Pendiente" if no fields filled, "Completo" if all fields filled, "Incompleto" otherwise
        /// </summary>
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

        /// <summary>
        /// Description: Calculate the completion status of the Risk section based on integrations
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing integrations collection
        /// Output Parameters: Status string - "Pendiente" if no complete integrations found, "Completo" if at least one integration has all fields filled
        /// </summary>
        public static string CalculateRiskSectionStatus(ProjectDocument project)
        {
            var hasCompleteIntegration = project.Integrations?.Any(i =>
                !string.IsNullOrEmpty(i.System) &&
                !string.IsNullOrEmpty(i.Description)
            ) ?? false;

            if (!hasCompleteIntegration) return "Pendiente";
            return "Completo";
        }

        /// <summary>
        /// Description: Calculate the completion status of the Test Case section based on test cases
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing test cases collection
        /// Output Parameters: Status string - "Pendiente" if no complete test cases found, "Completo" if at least one test case has all fields filled
        /// </summary>
        public static string CalculateTestCaseSectionStatus(ProjectDocument project)
        {
            var hasCompleteTestCases = project.TestCases?.Any(i =>
                !string.IsNullOrEmpty(i.TestStrategy) &&
                !string.IsNullOrEmpty(i.AcceptanceCriteria) &&
                !string.IsNullOrEmpty(i.DeployProductionCriteria)
            ) ?? false;

            if (!hasCompleteTestCases) return "Pendiente";
            return "Completo";
        }

    }
}