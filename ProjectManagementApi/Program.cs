using System.Diagnostics;
using System.Reflection;
using System.Text;
using DatabasesLib.Contexts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using ProjectManagementApi.Configuration;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils;
using ProjectManagementApi.Utils.Helpers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var tracingOptions = builder.Configuration.GetSection(TracingOptions.SectionName).Get<TracingOptions>() ?? new TracingOptions();
var metricsOptions = builder.Configuration.GetSection(MetricsOptions.SectionName).Get<MetricsOptions>() ?? new MetricsOptions();
var enableConsoleTracingExporter = tracingOptions.EnableConsoleExporter;
var hasOtlpTracingEndpoint = !string.IsNullOrWhiteSpace(tracingOptions.OtlpEndpoint);
var enableConsoleMetricsExporter = metricsOptions.EnableConsoleExporter;
var hasOtlpMetricsEndpoint = !string.IsNullOrWhiteSpace(metricsOptions.OtlpEndpoint);

var logsPath = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(logsPath);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Logging.AddConsole();
ConfigurationManager configuration = builder.Configuration;

var appSettingsSection = configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>();

string[] ParseConfigList(string key) =>
    (configuration.GetValue<string>(key) ?? string.Empty)
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

var allowedOrigins = ParseConfigList("AllowedOrigins");
var allowedHeaders = ParseConfigList("AllowedHeaders");
var allowedMethods = ParseConfigList("AllowedMethods");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var metrics = context.HttpContext.RequestServices.GetService<IErrorMetricsService>();
                var category = ErrorCategoryClassifier.Classify(context.Exception, StatusCodes.Status401Unauthorized);
                metrics?.Register(
                    category,
                    "JwtBearer.OnAuthenticationFailed",
                    context.HttpContext.Request.Path,
                    context.HttpContext.Request.Method,
                    context.HttpContext.TraceIdentifier,
                    StatusCodes.Status401Unauthorized,
                    context.Exception.GetBaseException().Message);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var metrics = context.HttpContext.RequestServices.GetService<IErrorMetricsService>();
                metrics?.Register(
                    "AUTH",
                    "JwtBearer.OnChallenge",
                    context.HttpContext.Request.Path,
                    context.HttpContext.Request.Method,
                    context.HttpContext.TraceIdentifier,
                    StatusCodes.Status401Unauthorized,
                    context.ErrorDescription ?? context.Error ?? "Unauthorized");
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = appSettings?.Jwt.Issuer ?? "",
            ValidAudience = appSettings?.Jwt.Audience ?? "",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings?.Jwt.Key ?? ""))
        };
    });


// Application Context
builder.Services.AddDbContext<ApplicationContext>();

// Services
builder.Services.AddScoped<IProjectDocumentRepository<ProjectDocument>, ProjectDocumentRepository>();
builder.Services.AddScoped<IAttachmentRepository<Attachment>, AttachmentRepository>();
builder.Services.AddScoped<IProjectDocumentRequirementRepository<ProjectDocumentRequirement>, ProjectDocumentRequirementRepository>();
builder.Services.AddScoped<IProjectDocumentIntegrationRepository<ProjectDocumentIntegration>, ProjectDocumentIntegrationRepository>();
builder.Services.AddScoped<IProjectDocumentRaciActorRepository<ProjectDocumentRaciActor>, ProjectDocumentRaciActorRepository>();
builder.Services.AddScoped<IProjectDocumentRiskRepository<ProjectDocumentRisk>, ProjectDocumentRiskRepository>();
builder.Services.AddScoped<IProjectDocumentTestCaseRepository<ProjectDocumentTestCase>, ProjectDocumentTestCaseRepository>();
builder.Services.AddScoped<ISolutionTypeRepository, SolutionTypeRepository>();
builder.Services.AddScoped<IDeploymentModelRepository, DeploymentModelRepository>();
builder.Services.AddScoped<IVicepresidencyRepository, VicepresidencyRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISsoAuthService, SsoAuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenUserService, TokenUserService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSingleton<IErrorMetricsService, ErrorMetricsService>();

builder.Services.AddAuthorization();

builder.Services.AddHttpLogging(o => { });
builder.Services.AddScoped<IDatabaseParametersService>(ServiceProvider => new DatabaseParametersService(
    DatabaseType.Informix,
    configuration.GetValue<string>("ConnectionStrings:ConnectionString")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Project Management API",
            Description = "Service for managing projects, tasks, and related entities.",
            Version = "v1"
        });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // generate xml docs that'll drive the swagger docs
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddOptions<TracingOptions>().Bind(builder.Configuration.GetSection(TracingOptions.SectionName));
builder.Services.AddOptions<MetricsOptions>().Bind(builder.Configuration.GetSection(MetricsOptions.SectionName));

var shouldConfigureTracing = tracingOptions.Enabled && (enableConsoleTracingExporter || hasOtlpTracingEndpoint);
var shouldConfigureMetrics = metricsOptions.Enabled && (enableConsoleMetricsExporter || hasOtlpMetricsEndpoint);

if (shouldConfigureTracing || shouldConfigureMetrics)
{
    var telemetryBuilder = builder.Services
        .AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(
                serviceName: "GestionProyectos.Api",
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(
            [
                new KeyValuePair<string, object>("service.namespace", "RECAMIER"),
                new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)
            ]));

    if (shouldConfigureTracing)
    {
        telemetryBuilder.WithTracing(tracing =>
        {
            tracing
                .SetSampler(new TraceIdRatioBasedSampler(ClampSamplingRatio(tracingOptions.SamplingRatio)))
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);
                })
                .AddHttpClientInstrumentation(options => options.RecordException = true);

            if (enableConsoleTracingExporter)
            {
                tracing.AddConsoleExporter();
            }

            if (hasOtlpTracingEndpoint)
            {
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(tracingOptions.OtlpEndpoint!);
                    if (!string.IsNullOrWhiteSpace(tracingOptions.OtlpHeaders))
                    {
                        options.Headers = tracingOptions.OtlpHeaders;
                    }
                });
            }
        });
    }

    if (shouldConfigureMetrics)
    {
        telemetryBuilder.WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation().AddRuntimeInstrumentation();

            if (enableConsoleMetricsExporter)
            {
                metrics.AddConsoleExporter();
            }

            if (hasOtlpMetricsEndpoint)
            {
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(metricsOptions.OtlpEndpoint!);
                    if (!string.IsNullOrWhiteSpace(metricsOptions.OtlpHeaders))
                    {
                        options.Headers = metricsOptions.OtlpHeaders;
                    }
                });
            }
        });
    }
}

var app = builder.Build();

app.UseSerilogRequestLogging();

var hasSpaBuild = File.Exists(Path.Combine(app.Environment.WebRootPath ?? string.Empty, "index.html"));

// Configure the HTTP request pipeline.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;
        var logger = context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("GlobalExceptionHandler");

        if (exception != null)
        {
            var category = ErrorCategoryClassifier.Classify(exception, StatusCodes.Status500InternalServerError);
            logger.LogError(exception, "Unhandled exception [{Category}] for request {Method} {Path} TraceId={TraceId}", category, context.Request.Method, context.Request.Path, context.TraceIdentifier);

            var metrics = context.RequestServices.GetService<IErrorMetricsService>();
            metrics?.Register(
                category,
                "GlobalExceptionHandler",
                context.Request.Path,
                context.Request.Method,
                context.TraceIdentifier,
                StatusCodes.Status500InternalServerError,
                exception.GetBaseException().Message);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            message = "Ocurrió un error al procesar la solicitud.",
            detail = exception?.GetBaseException().Message ?? "Error no identificado.",
            traceId = context.TraceIdentifier,
            category = ErrorCategoryClassifier.Classify(exception, StatusCodes.Status500InternalServerError)
        });
    });
});

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;

    using var scope = app.Logger.BeginScope(new Dictionary<string, object?>
    {
        ["TraceId"] = context.TraceIdentifier,
        ["Path"] = context.Request.Path.Value,
        ["Method"] = context.Request.Method
    });

    await next();

    if (context.Response.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
    {
        var metrics = context.RequestServices.GetService<IErrorMetricsService>();
        metrics?.Register(
            "AUTH",
            "StatusCodeObserver",
            context.Request.Path,
            context.Request.Method,
            context.TraceIdentifier,
            context.Response.StatusCode,
            "Authorization failure");
    }
});

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectManagementApi v1"));

app.UseCors(policy =>
{
    var allowAnyOrigin = allowedOrigins.Length == 0 || allowedOrigins.Contains("*");
    var allowAnyHeader = allowedHeaders.Length == 0 || allowedHeaders.Contains("*");
    var allowAnyMethod = allowedMethods.Length == 0 || allowedMethods.Contains("*");

    if (allowAnyOrigin)
    {
        policy.AllowAnyOrigin();
    }
    else
    {
        policy.WithOrigins(allowedOrigins)
            .AllowCredentials();
    }

    if (allowAnyHeader)
    {
        policy.AllowAnyHeader();
    }
    else
    {
        policy.WithHeaders(allowedHeaders);
    }

    if (allowAnyMethod)
    {
        policy.AllowAnyMethod();
    }
    else
    {
        policy.WithMethods(allowedMethods);
    }

    policy.WithExposedHeaders("Content-Disposition");
});

app.UseHttpLogging();
if (hasSpaBuild)
{
    app.UseDefaultFiles();
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (hasSpaBuild)
{
    app.MapFallbackToFile("index.html");
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" })).AllowAnonymous();

app.Run();

static double ClampSamplingRatio(double ratio)
{
    return double.IsFinite(ratio) ? Math.Clamp(ratio, 0d, 1d) : 1d;
}
