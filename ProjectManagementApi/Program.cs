using System.Diagnostics;
using System.Reflection;
using System.Text;
using DatabasesLib.Contexts;
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
builder.Logging.AddConsole();
ConfigurationManager configuration = builder.Configuration;

var appSettingsSection = configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>();


Console.WriteLine("AppSettings:");
Console.WriteLine($"Jwt: Issuer={appSettings?.Jwt.Issuer}, Audience={appSettings?.Jwt.Audience}, SecretKey={appSettings?.Jwt.Key}, Expired={appSettings?.Jwt.Expired}");

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
builder.Services.AddScoped<IProjectDocumentAttachmentRepository<ProjectDocumentAttachment>, ProjectDocumentAttachmentRepository>();
builder.Services.AddScoped<IProjectDocumentRequirementRepository<ProjectDocumentRequirement>, ProjectDocumentRequirementRepository>();
builder.Services.AddScoped<IProjectDocumentIntegrationRepository<ProjectDocumentIntegration>, ProjectDocumentIntegrationRepository>();
builder.Services.AddScoped<IProjectDocumentRaciActorRepository<ProjectDocumentRaciActor>, ProjectDocumentRaciActorRepository>();
builder.Services.AddScoped<IProjectDocumentRiskRepository<ProjectDocumentRisk>, ProjectDocumentRiskRepository>();
builder.Services.AddScoped<IProjectDocumentTestCaseRepository<ProjectDocumentTestCase>, ProjectDocumentTestCaseRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenUserService, TokenUserService>();
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectManagementApi v1"));
}

app.UseCors(x =>
    x.WithOrigins((builder.Configuration.GetSection("AllowedOrigins").Value ?? string.Empty).Split(";"))
        .AllowCredentials().WithHeaders((builder.Configuration.GetSection("AllowedHeaders").Value ?? string.Empty).Split(";"))
        .WithMethods((builder.Configuration.GetSection("AllowedMethods").Value ?? string.Empty).Split(";"))
        .WithExposedHeaders("Content-Disposition"));

app.UseHttpLogging();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
