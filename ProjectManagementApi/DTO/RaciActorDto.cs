namespace ProjectManagementApi.DTO
{
    public class RaciActorDto
    {
        public int Id { get; set; }
        public int ProjectDocumentId { get; set; }
        public string? Activity { get; set; }
        public string? Type { get; set; }
        public string? Area { get; set; }
        public string? Role { get; set; }
    }
}