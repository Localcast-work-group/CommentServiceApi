using CategoryService.Api.Data;
using CategoryService.Api.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace CategoryService.Api.Services
{
    public static class DatabaseManagmentService
    {
        public static async void MigrationInitialisation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var service = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                await service.Database.MigrateAsync();
                var categoryService = serviceScope.ServiceProvider.GetService<ICategoryService>();
                await categoryService.Seed();
            }
        }
    }
}
