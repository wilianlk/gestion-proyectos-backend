namespace ProjectManagementApi.DTO
{
    public class UpdateConstraintsSectionDto
    {
        public string EstimatedBudget { get; set; } = null!;
        public DateTime TargetDate { get; set; }
        public string TechnicalConstraints { get; set; } = null!;
        public string BusinessConstraints { get; set; } = null!;
        public string RegulationsCompliance { get; set; } = null!;
    }

}