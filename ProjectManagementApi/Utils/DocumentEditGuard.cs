using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Utils
{
    public static class DocumentEditGuard
    {
        public static bool IsDocumentComplete(ProjectDocument project) =>
            string.Equals(project.DocumentStatus, "Completo", StringComparison.OrdinalIgnoreCase);

        public static ActionResult? EnsureEditable(ProjectDocument? project, string projectCode, bool isAdmin = false)
        {
            if (project == null)
            {
                return new NotFoundObjectResult(new { message = $"Project with code '{projectCode}' not found" });
            }

            if (IsDocumentComplete(project) && !isAdmin)
            {
                return new BadRequestObjectResult(new { message = "El documento está en estado Completo y solo un administrador puede editarlo." });
            }

            return null;
        }
    }
}
