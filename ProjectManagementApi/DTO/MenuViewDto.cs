using System.ComponentModel.DataAnnotations;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.DTO
{
    public class MenuViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public bool IsNavigate { get; set; }
        public string IconMenu { get; set; }
        public bool IsActive { get; set; }
        public bool IsErrorDetails { get; set; }
        public string Url { get; set; }
        public MenuParentViewDto FirstParent { get; set; }
        public MenuParentViewDto SecondParent { get; set; }
        public bool IsParent { get; set; }
        public string OriginalUrl { get; set; }
    }
}