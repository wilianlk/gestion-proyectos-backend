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

        public DbSet<Menu> Menus { get; set; }
        public DbSet<ProjectDocument> ProjectDocuments { get; set; }
        public DbSet<ProjectDocumentAttachment> ProjectDocumentAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // init entities
            modelBuilder.Entity<Menu>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Name).IsUnique();
                entity.HasOne(x => x.FirstParent).WithOne();
                entity.HasOne(x => x.SecondParent).WithOne();
            });

            modelBuilder.Entity<ProjectDocument>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.ProjectCode).IsUnique();
                entity.HasMany(x => x.Attachments).WithOne(a => a.ProjectDocument).HasForeignKey(x => x.ProjectDocumentId);
            });

            modelBuilder.Entity<ProjectDocumentAttachment>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            // init data
            // modelBuilder.Entity<Menu>().HasData(EntityData.GetMenus());

            base.OnModelCreating(modelBuilder);
        }
    }
}