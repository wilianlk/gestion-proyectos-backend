namespace ProjectManagementApi.DTO
{
    // DTOs for RACI Actors
    public class CreateRaciActorDto
    {
        public string ProjectCode { get; set; }
        public List<RaciActorRowDto> Rows { get; set; }
    }
}