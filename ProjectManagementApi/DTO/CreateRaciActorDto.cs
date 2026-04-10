namespace ProjectManagementApi.DTO
{
    // DTOs for RACI Actors
    public class CreateRaciActorDto
    {
        public int ProjectDocumentId { get; set; }
        public List<RaciActorRowDto> Rows { get; set; }
    }
}