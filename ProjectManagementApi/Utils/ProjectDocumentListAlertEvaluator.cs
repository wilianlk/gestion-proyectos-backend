using ProjectManagementApi.Models;

namespace ProjectManagementApi.Utils
{
    public static class ProjectDocumentListAlertEvaluator
    {
        public static bool HasIncompleteSection(ProjectDocumentCompletionSnapshot project)
        {
            return
                !string.Equals(CalculateGeneralSectionStatus(project), "Completo", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(CalculateArchitectureSectionStatus(project), "Completo", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(CalculateUxCasesSectionStatus(project), "Completo", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(CalculateConstraintsSectionStatus(project), "Completo", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(CalculateRaciSectionStatus(project), "Completo", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(CalculateRequirementsSectionStatus(project), "Completo", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(CalculateIntegrationsSectionStatus(project), "Completo", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(CalculateRiskSectionStatus(project), "Completo", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(CalculateTestCaseSectionStatus(project), "Completo", StringComparison.OrdinalIgnoreCase);
        }

        private static string CalculateGeneralSectionStatus(ProjectDocumentCompletionSnapshot project)
        {
            var filledFields = 0;

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
            if (filledFields == 10) return "Completo";
            return "Incompleto";
        }

        private static string CalculateArchitectureSectionStatus(ProjectDocumentCompletionSnapshot project)
        {
            var filledFields = 0;

            if (!string.IsNullOrWhiteSpace(project.SolutionDescription)) filledFields++;
            if (!string.IsNullOrWhiteSpace(project.SoftwareStack)) filledFields++;
            if (!string.IsNullOrWhiteSpace(project.SecurityControl)) filledFields++;
            if (!string.IsNullOrWhiteSpace(project.HardwareArchitecture) || project.HasArchitectureAttachment) filledFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields >= 4) return "Completo";
            return "Incompleto";
        }

        private static string CalculateUxCasesSectionStatus(ProjectDocumentCompletionSnapshot project)
        {
            var filledFields = 0;

            if (project.HasUseCasesAttachment) filledFields++;
            if (project.HasUxAttachment) filledFields++;

            if (filledFields == 0) return "Pendiente";
            if (filledFields >= 2) return "Completo";
            return "Incompleto";
        }

        private static string CalculateConstraintsSectionStatus(ProjectDocumentCompletionSnapshot project)
        {
            var filledRequiredFields = 0;

            if (HasMeaningfulText(project.TechnicalConstraints)) filledRequiredFields++;
            if (HasMeaningfulText(project.BusinessConstraints)) filledRequiredFields++;
            if (HasMeaningfulText(project.RegulationsCompliance)) filledRequiredFields++;

            var hasOptionalData = HasMeaningfulText(project.EstimatedBudget) || project.TargetDate.HasValue;

            if (filledRequiredFields == 0 && !hasOptionalData) return "Pendiente";
            if (filledRequiredFields == 3) return "Completo";
            return "Incompleto";
        }

        private static string CalculateRaciSectionStatus(ProjectDocumentCompletionSnapshot project)
        {
            return project.HasCompleteRaciActor ? "Completo" : "Pendiente";
        }

        private static string CalculateRequirementsSectionStatus(ProjectDocumentCompletionSnapshot project)
        {
            if (!project.HasAnyRequirementData) return "Pendiente";
            return project.HasCompleteRequirement ? "Completo" : "Incompleto";
        }

        private static string CalculateIntegrationsSectionStatus(ProjectDocumentCompletionSnapshot project)
        {
            if (!project.HasAnyIntegrationData) return "Pendiente";
            return project.HasCompleteIntegration ? "Completo" : "Incompleto";
        }

        private static string CalculateRiskSectionStatus(ProjectDocumentCompletionSnapshot project)
        {
            return project.HasCompleteRisk ? "Completo" : "Pendiente";
        }

        private static string CalculateTestCaseSectionStatus(ProjectDocumentCompletionSnapshot project)
        {
            return project.HasCompleteTestCase ? "Completo" : "Pendiente";
        }

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
    }
}
