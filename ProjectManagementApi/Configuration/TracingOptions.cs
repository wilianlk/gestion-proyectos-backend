namespace ProjectManagementApi.Configuration;

public sealed class TracingOptions
{
    public const string SectionName = "Tracing";

    public bool Enabled { get; set; } = true;
    public bool EnableConsoleExporter { get; set; }
    public string? OtlpEndpoint { get; set; }
    public string? OtlpHeaders { get; set; }
    public double SamplingRatio { get; set; } = 1d;
}
