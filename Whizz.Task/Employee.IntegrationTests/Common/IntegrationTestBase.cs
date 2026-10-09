using System.Net.Http.Headers;

namespace Employee.IntegrationTests.Common
{
    public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly CustomWebApplicationFactory Factory;
        protected readonly HttpClient Client;

        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
        }

        protected HttpClient CreateClientWithTenant(Guid tenantId)
        {
            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
            return client;
        }
    }
}
