using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ObservabilityController : ControllerBase
    {
        private readonly IErrorMetricsService _errorMetricsService;

        public ObservabilityController(IErrorMetricsService errorMetricsService)
        {
            _errorMetricsService = errorMetricsService;
        }

        [HttpGet("ErrorDashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<object> ErrorDashboard()
        {
            return Ok(_errorMetricsService.GetDashboardSnapshot());
        }
    }
}
