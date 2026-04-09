using System.Reflection;
using DatabasesLib.Contexts;
using Microsoft.OpenApi.Models;
using ProjectManagementApi.Context;
using ProjectManagementApi.Models;
using ProjectManagementApi.Repositories;
using ProjectManagementApi.Services;
using ProjectManagementApi.Services.Contracts;
using ProjectManagementApi.Utils.Helpers;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

var appSettingsSection = configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection.Get<AppSettings>();

// Application Context
builder.Services.AddDbContext<ApplicationContext>();

// Services
builder.Services.AddScoped<IMenuRepository<Menu>, MenuRepository>();
builder.Services.AddScoped<IProjectDocumentRepository<ProjectDocument>, ProjectDocumentRepository>();
builder.Services.AddScoped<IProjectDocumentAttachmentRepository<ProjectDocumentAttachment>, ProjectDocumentAttachmentRepository>();
builder.Services.AddScoped<IFileService, FileService>();

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

app.UseHttpLogging();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
