using AutoMapper;
using Employee.Application.Common.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Employee.UnitTests.Common
{
    public abstract class TestBase
    {
        protected readonly IMapper Mapper;

        protected TestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
            var serviceProvider = services.BuildServiceProvider();
            Mapper = serviceProvider.GetRequiredService<IMapper>();
        }
    }
}
