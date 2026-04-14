namespace ProjectManagementApi.DTO
{
    public class CreateTestCaseDto
    {
        public string ProjectCode { get; set; } = null!;
        public List<TestCaseItemDto> TestCases { get; set; } = null!;
    }

}
