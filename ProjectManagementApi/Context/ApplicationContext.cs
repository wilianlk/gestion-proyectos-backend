using DatabasesLib.Contexts;
using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.Models;
using ProjectManagementApi.Services.Contracts;

namespace ProjectManagementApi.Context
{
    public class ApplicationContext : DatabasesContext
    {
        public ApplicationContext(DbContextOptions options, IDatabaseParametersService databaseParametersService)
        : base(
             databaseParametersService.GetDatabaseType(),
              databaseParametersService.GetDbConnectionString(),
              databaseParametersService.GetTimeout()
         )
        { }

        public DbSet<ProjectDocument> ProjectDocuments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<ProjectDocumentRequirement> ProjectDocumentRequirements { get; set; }
        public DbSet<ProjectDocumentIntegration> ProjectDocumentIntegrations { get; set; }
        public DbSet<ProjectDocumentRaciActor> ProjectDocumentRaciActors { get; set; }
        public DbSet<ProjectDocumentRisk> ProjectDocumentRisks { get; set; }
        public DbSet<ProjectDocumentTestCase> ProjectDocumentTestCases { get; set; }
        public DbSet<SolutionType> SolutionTypes { get; set; }
        public DbSet<DeploymentModel> DeploymentModels { get; set; }
        public DbSet<Vicepresidency> Vicepresidencies { get; set; }
        public DbSet<Area> Areas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // init entities

            modelBuilder.Entity<ProjectDocument>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.ProjectCode).IsUnique();
                entity.HasMany(x => x.Attachments).WithOne(a => a.ProjectDocument).HasForeignKey(x => x.ProjectDocumentId);
                entity.HasMany(x => x.Requirements).WithOne(a => a.ProjectDocument).HasForeignKey(x => x.ProjectDocumentId);
                entity.HasMany(x => x.Integrations).WithOne(a => a.ProjectDocument).HasForeignKey(x => x.ProjectDocumentId);
                entity.HasMany(x => x.RaciActors).WithOne(a => a.ProjectDocument).HasForeignKey(x => x.ProjectDocumentId);
            });

            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<ProjectDocumentRequirement>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<ProjectDocumentIntegration>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<ProjectDocumentRaciActor>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<ProjectDocumentRisk>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<ProjectDocumentTestCase>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<Area>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<SolutionType>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<DeploymentModel>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<Vicepresidency>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
