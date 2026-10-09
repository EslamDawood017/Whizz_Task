using Employee.Application.Common;
using Employee.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Employee.IntegrationTests.Middleware
{
    public class TenantMiddlewareIntegrationTests : IntegrationTestBase
    {
        public TenantMiddlewareIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Request_WithoutTenantHeader_ShouldReturn400BadRequest()
        {
            // Act: send request without X-Tenant-Id header
            var response = await Client.GetAsync("/api/v1/employees");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            body.Should().NotBeNull();
            body!.Error.Should().Contain("X-Tenant-Id");
        }

        [Fact]
        public async Task Request_WithInvalidGuidTenantHeader_ShouldReturn400BadRequest()
        {
            // Arrange
            var client = Factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-Id", "not-a-valid-guid");

            // Act
            var response = await client.GetAsync("/api/v1/employees");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            body.Should().NotBeNull();
            body!.Error.Should().Contain("X-Tenant-Id");
        }

        [Fact]
        public async Task SwaggerEndpoint_WithoutTenantHeader_ShouldBeAccessible()
        {
            // Act: Swagger should bypass TenantMiddleware
            var response = await Client.GetAsync("/swagger/v1/swagger.json");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
