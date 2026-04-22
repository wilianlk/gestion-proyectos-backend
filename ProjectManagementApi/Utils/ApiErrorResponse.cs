using Microsoft.AspNetCore.Mvc;

namespace ProjectManagementApi.Utils
{
    public static class ApiErrorResponse
    {
        public static BadRequestObjectResult BadRequest(
            ControllerBase controller,
            Exception exception,
            string message)
        {
            return controller.BadRequest(new
            {
                message,
                detail = exception.GetBaseException().Message,
                traceId = controller.HttpContext?.TraceIdentifier
            });
        }
    }
}
