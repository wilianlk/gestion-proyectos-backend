using System.ComponentModel.DataAnnotations;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.DTO
{
    public class MenuParentViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }        
        public string Url { get; set; }
    
    }
}