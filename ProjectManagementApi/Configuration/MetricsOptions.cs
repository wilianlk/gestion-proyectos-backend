namespace ProjectManagementApi.Configuration;

public sealed class MetricsOptions
{
    public const string SectionName = "Metrics";

    public bool Enabled { get; set; }
    public bool EnableConsoleExporter { get; set; }
    public bool EnablePrometheusExporter { get; set; }
    public string? OtlpEndpoint { get; set; }
    public string? OtlpHeaders { get; set; }
}
