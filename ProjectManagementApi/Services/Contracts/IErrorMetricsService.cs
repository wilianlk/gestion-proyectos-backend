namespace ProjectManagementApi.Services.Contracts
{
    public interface IErrorMetricsService
    {
        void Register(
            string category,
            string source,
            string? path,
            string? method,
            string? traceId,
            int? statusCode,
            string? message);

        object GetDashboardSnapshot();
    }
}
