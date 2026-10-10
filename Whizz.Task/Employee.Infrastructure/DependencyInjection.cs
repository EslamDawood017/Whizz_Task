using Employee.Application.Common.Interfaces;
using Employee.Infrastructure.Common;
using Employee.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Employee.Infrastructure.Persistence.Interceptors;


namespace Employee.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddScoped<TenantSessionInterceptor>();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.UseNpgsql(connectionString)
                       .AddInterceptors(sp.GetRequiredService<TenantSessionInterceptor>());
            });

            services.AddScoped<ITenantContext, TenantContext>();

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            return services;
        }
    }
}
