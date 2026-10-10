using Employee.API.Middleware;
using Employee.Application;
using Employee.Application.Common;
using Employee.Infrastructure;
using Employee.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employee.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {            
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .Where(msg => !string.IsNullOrWhiteSpace(msg))
                        .ToList();

                    var combinedError = string.Join("; ", errors);
                    if (string.IsNullOrWhiteSpace(combinedError))
                    {
                        combinedError = "Invalid request payload.";
                    }
                    var envelope = ApiResponse<object>.Failure(combinedError);
                    return new BadRequestObjectResult(envelope);
                };
            });

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

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseMiddleware<TenantMiddleware>();


            app.MapControllers();

            app.Run();
        }
    }
}
