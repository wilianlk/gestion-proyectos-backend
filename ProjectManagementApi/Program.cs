using System.Diagnostics;
using System.Reflection;
using System.Text;
using DatabasesLib.Contexts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils.Helpers;

var builder = WebApplication.CreateBuilder(args);

var logsPath = Path.Combine(AppContext.BaseDirectory, "Logs");
Directory.CreateDirectory(logsPath);

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
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenUserService, TokenUserService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IFileService, FileService>();

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

var app = builder.Build();
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
            logger.LogError(exception, "Unhandled exception for request {Method} {Path}", context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            message = "Ocurrió un error al procesar la solicitud.",
            detail = exception?.GetBaseException().Message ?? "Error no identificado.",
            traceId = context.TraceIdentifier
        });
    });
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

app.Run();
