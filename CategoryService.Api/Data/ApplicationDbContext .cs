using Microsoft.EntityFrameworkCore;
using System;
using CategoryService.Api.Models;
using System.Reflection;

namespace CategoryService.Api.Data
{
    public class ApplicationDbContext :DbContext
    {
        public DbSet<Category> Category { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(x => x.Name).IsUnique();
            });
        }
    }
}
