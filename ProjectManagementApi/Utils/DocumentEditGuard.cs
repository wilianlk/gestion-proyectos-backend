using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.Models;

namespace ProjectManagementApi.Utils
{
    public static class DocumentEditGuard
    {
        public static bool IsDocumentComplete(ProjectDocument project) =>
            string.Equals(project.DocumentStatus, "Completo", StringComparison.OrdinalIgnoreCase);

        public static ActionResult? EnsureEditable(ProjectDocument? project, string projectCode)
        {
            if (project == null)
            {
                return new NotFoundObjectResult(new { message = $"Project with code '{projectCode}' not found" });
            }

            if (IsDocumentComplete(project))
            {
                return new BadRequestObjectResult(new { message = "El documento está en estado Completo y no permite más ediciones." });
            }

            return null;
        }
    }
}
