using Employee.API.Middleware;
using Employee.Application;
using Employee.Infrastructure;
using Employee.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Employee.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                 await context.Database.MigrateAsync();
                 await DatabaseSeeder.SeedAsync(context);
            }


            // Configure the HTTP request pipeline.

            app.UseAuthorization();

            app.UseMiddleware<TenantMiddleware>();


            app.MapControllers();

            app.Run();
        }
    }
}
