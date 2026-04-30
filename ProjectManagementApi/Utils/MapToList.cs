using ProjectManagementApi.DTO;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Utils
{
    public static class MapToList
    {
        public static ProjectDocumentListDto MapToListDto(ProjectDocument project)
        {
            return new ProjectDocumentListDto
            {
                Id = project.Id,
                ProjectCode = project.ProjectCode,
                ProjectName = project.ProjectName,
                Sponsor = project.Sponsor,
                TechnicalLead = project.TechnicalLead,
                DocumentStatus = project.DocumentStatus,
                CreatedAt = project.CreatedAt
            };
        }
    }
    
}
