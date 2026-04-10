namespace ProjectManagementApi.DTO
{
    public class CreateTestCaseDto
    {
        public string ProjectCode { get; set; }
        public List<TestCaseItemDto> TestCases { get; set; }
    }

}
