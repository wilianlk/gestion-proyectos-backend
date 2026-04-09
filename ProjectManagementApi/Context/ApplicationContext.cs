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

            // init data
            // modelBuilder.Entity<Menu>().HasData(EntityData.GetMenus());

            base.OnModelCreating(modelBuilder);
        }
    }
}
