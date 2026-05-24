using System.Collections.Concurrent;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Services
{
    public class ErrorMetricsService : IErrorMetricsService
    {
        private sealed record ErrorEvent(
            DateTime TimestampUtc,
            string Category,
            string Source,
            string Path,
            string Method,
            string TraceId,
            int StatusCode,
            string Message);

        private readonly ConcurrentQueue<ErrorEvent> _events = new();
        private const int MaxEvents = 2000;

        public void Register(
            string category,
            string source,
            string? path,
            string? method,
            string? traceId,
            int? statusCode,
            string? message)
        {
            var item = new ErrorEvent(
                DateTime.UtcNow,
                string.IsNullOrWhiteSpace(category) ? "UNKNOWN" : category.Trim().ToUpperInvariant(),
                source?.Trim() ?? "UNKNOWN",
                path?.Trim() ?? "-",
                method?.Trim() ?? "-",
                traceId?.Trim() ?? "-",
                statusCode ?? 0,
                message?.Trim() ?? string.Empty);

            _events.Enqueue(item);
            while (_events.Count > MaxEvents && _events.TryDequeue(out _))
            {
            }
        }

        public object GetDashboardSnapshot()
        {
            var now = DateTime.UtcNow;
            var lastHour = now.AddHours(-1);
            var data = _events.ToArray().Where(x => x.TimestampUtc >= lastHour).ToArray();

            return new
            {
                generatedAtUtc = now,
                windowMinutes = 60,
                totalErrors = data.Length,
                byCategory = data
                    .GroupBy(x => x.Category)
                    .Select(g => new { category = g.Key, count = g.Count() })
                    .OrderByDescending(x => x.count),
                topRoutes = data
                    .GroupBy(x => $"{x.Method} {x.Path}")
                    .Select(g => new { route = g.Key, count = g.Count() })
                    .OrderByDescending(x => x.count)
                    .Take(10),
                recent = data
                    .OrderByDescending(x => x.TimestampUtc)
                    .Take(20)
                    .Select(x => new
                    {
                        timestampUtc = x.TimestampUtc,
                        x.Category,
                        x.Source,
                        x.Path,
                        x.Method,
                        x.TraceId,
                        x.StatusCode,
                        x.Message
                    })
            };
        }
    }
}
