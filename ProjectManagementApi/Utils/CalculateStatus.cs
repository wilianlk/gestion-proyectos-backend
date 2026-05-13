using ProjectManagementApi.Models;

namespace ProjectManagementApi.Utils
{
    public static class CalculateStatus
    {
        private static bool HasMeaningfulText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var normalized = value
                .Replace("&nbsp;", " ", StringComparison.OrdinalIgnoreCase)
                .Replace("<br>", " ", StringComparison.OrdinalIgnoreCase)
                .Replace("<br/>", " ", StringComparison.OrdinalIgnoreCase)
                .Replace("<br />", " ", StringComparison.OrdinalIgnoreCase);

            var plainText = System.Text.RegularExpressions.Regex
                .Replace(normalized, "<[^>]*>", " ")
                .Trim();

            return !string.IsNullOrWhiteSpace(plainText);
        }

        /// <summary>
        /// Description: Calculate the completion status of the General section
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing general section fields
        /// Output Parameters: Status string - "Pendiente" if no fields filled, "Completo" if all fields filled, "Incomplete" otherwise
        /// </summary>
        public static string CalculateGeneralSectionStatus(ProjectDocument project)
        {
            int totalFields = 10;
            int filledFields = 0;

            if (!string.IsNullOrEmpty(project.ProjectName)) filledFields++;
            if (!string.IsNullOrEmpty(project.Sponsor)) filledFields++;
            if (!string.IsNullOrEmpty(project.FunctionalLead)) filledFields++;
            if (!string.IsNullOrEmpty(project.TechnicalLead)) filledFields++;
            if (!string.IsNullOrEmpty(project.DocumentStatus)) filledFields++;
            if (!string.IsNullOrEmpty(project.ProjectVision)) filledFields++;
            if (!string.IsNullOrEmpty(project.GeneralObjective)) filledFields++;
            if (!string.IsNullOrEmpty(project.SpecificObjectives)) filledFields++;
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
            int totalFields = 4;
            int filledFields = 0;

            if (!string.IsNullOrWhiteSpace(project.SolutionDescription)) filledFields++;
            if (!string.IsNullOrWhiteSpace(project.SoftwareStack)) filledFields++;
            if (!string.IsNullOrWhiteSpace(project.SecurityControl)) filledFields++;

            bool hasHardwareDescription = !string.IsNullOrWhiteSpace(project.HardwareArchitecture);
            bool hasArchitectureAttachment = project.Attachments?.Any(a =>
                !string.IsNullOrWhiteSpace(a.Section) &&
                string.Equals(a.Section.Trim(), "Architecture", StringComparison.OrdinalIgnoreCase)) ?? false;

            if (hasHardwareDescription || hasArchitectureAttachment) filledFields++;

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
            int totalFields = 2;
            int filledFields = 0;

            bool hasUseCasesAttachment = project.Attachments?.Any(a =>
                a.Section == "UxCasesUseCases" || a.Section == "UxCases") ?? false;
            if (hasUseCasesAttachment) filledFields++;

            bool hasUxAttachment = project.Attachments?.Any(a =>
                a.Section == "UxCasesExperienceDesignMockups" || a.Section == "UxCases") ?? false;
            if (hasUxAttachment) filledFields++;

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
            int requiredFields = 3;
            int filledRequiredFields = 0;

            bool hasEstimatedBudget = HasMeaningfulText(project.EstimatedBudget);
            bool hasTargetDate = project.TargetDate.HasValue;
            bool hasTechnicalConstraints = HasMeaningfulText(project.TechnicalConstraints);
            bool hasBusinessConstraints = HasMeaningfulText(project.BusinessConstraints);
            bool hasRegulationsCompliance = HasMeaningfulText(project.RegulationsCompliance);

            if (hasTechnicalConstraints) filledRequiredFields++;
            if (hasBusinessConstraints) filledRequiredFields++;
            if (hasRegulationsCompliance) filledRequiredFields++;

            bool hasOptionalData = hasEstimatedBudget || hasTargetDate;

            if (filledRequiredFields == 0 && !hasOptionalData) return "Pendiente";
            if (filledRequiredFields == requiredFields) return "Completo";
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
            var hasCompleteRaciActor = project.RaciActors?.Any(actor =>
                !string.IsNullOrWhiteSpace(actor.Activity) &&
                !string.IsNullOrWhiteSpace(actor.Type) &&
                !string.IsNullOrWhiteSpace(actor.Area) &&
                !string.IsNullOrWhiteSpace(actor.Role)
            ) ?? false;

            if (!hasCompleteRaciActor) return "Pendiente";
            return "Completo";
        }

        public static string CalculateRequirementsSectionStatus(ProjectDocument project)
        {
            var requirements = project.Requirements ?? Enumerable.Empty<ProjectDocumentRequirement>();

            var hasAnyRequirementData = requirements.Any(r =>
                !string.IsNullOrWhiteSpace(r.Code) ||
                !string.IsNullOrWhiteSpace(r.Description) ||
                !string.IsNullOrWhiteSpace(r.Type) ||
                !string.IsNullOrWhiteSpace(r.Priority) ||
                !string.IsNullOrWhiteSpace(r.AcceptanceCriteria));

            var hasCompleteRequirement = requirements.Any(r =>
                !string.IsNullOrWhiteSpace(r.Code) &&
                !string.IsNullOrWhiteSpace(r.Description) &&
                !string.IsNullOrWhiteSpace(r.Type) &&
                !string.IsNullOrWhiteSpace(r.Priority) &&
                !string.IsNullOrWhiteSpace(r.AcceptanceCriteria));

            if (!hasAnyRequirementData) return "Pendiente";
            if (hasCompleteRequirement) return "Completo";
            return "Incompleto";
        }

        public static string CalculateIntegrationsSectionStatus(ProjectDocument project)
        {
            var integrations = project.Integrations ?? Enumerable.Empty<ProjectDocumentIntegration>();

            var hasAnyIntegrationData = integrations.Any(i =>
                !string.IsNullOrWhiteSpace(i.System) ||
                !string.IsNullOrWhiteSpace(i.Description));

            var hasCompleteIntegration = integrations.Any(i =>
                !string.IsNullOrWhiteSpace(i.System) &&
                !string.IsNullOrWhiteSpace(i.Description));

            if (!hasAnyIntegrationData) return "Pendiente";
            if (hasCompleteIntegration) return "Completo";
            return "Incompleto";
        }

        /// <summary>
        /// Description: Calculate the completion status of the Risk section based on risks
        /// Input Parameters: 
        ///     * project (ProjectDocument): Project document object containing risks collection
        /// Output Parameters: Status string - "Pendiente" if no complete risks found, "Completo" if at least one risk has all fields filled
        /// </summary>
        public static string CalculateRiskSectionStatus(ProjectDocument project)
        {
            var hasCompleteRisk = project.Risks?.Any(r =>
                !string.IsNullOrEmpty(r.Risk) &&
                !string.IsNullOrEmpty(r.Impact) &&
                !string.IsNullOrEmpty(r.Probability) &&
                !string.IsNullOrEmpty(r.Mitigation) &&
                !string.IsNullOrEmpty(r.Owner)
            ) ?? false;

            if (!hasCompleteRisk) return "Pendiente";
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
