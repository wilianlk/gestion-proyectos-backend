namespace ProjectManagementApi.DTO
{
    public class UpdateConstraintsSectionDto
    {
        public string? EstimatedBudget { get; set; }
        public DateTime? TargetDate { get; set; }
        public string? TechnicalConstraints { get; set; }
        public string? BusinessConstraints { get; set; }
        public string? RegulationsCompliance { get; set; }
    }

}