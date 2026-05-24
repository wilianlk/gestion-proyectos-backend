using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Utils
{
    public static class ApiErrorResponse
    {
        public static BadRequestObjectResult BadRequest(
            ControllerBase controller,
            Exception exception,
            string message)
        {
            var category = ErrorCategoryClassifier.Classify(exception, StatusCodes.Status400BadRequest);
            var metrics = controller.HttpContext?.RequestServices.GetService<IErrorMetricsService>();
            metrics?.Register(
                category,
                "Controller",
                controller.HttpContext?.Request?.Path.Value,
                controller.HttpContext?.Request?.Method,
                controller.HttpContext?.TraceIdentifier,
                StatusCodes.Status400BadRequest,
                exception.GetBaseException().Message);

            return controller.BadRequest(new
            {
                message,
                detail = exception.GetBaseException().Message,
                traceId = controller.HttpContext?.TraceIdentifier,
                category
            });
        }
    }
}
