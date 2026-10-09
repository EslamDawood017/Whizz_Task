using Employee.Application.Common.Interfaces;
using Employee.Application.Common;

namespace Employee.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            var path = context.Request.Path;

            if (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/favicon.ico"))
            {
                await _next(context);
                return;
            }


            //  Extract and Validate the Header
            if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeaderValues) ||
            !Guid.TryParse(tenantHeaderValues.FirstOrDefault(), out var tenantId) ||
            tenantId == Guid.Empty)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                
                var errorResponse = ApiResponse<object>.Failure("A valid 'X-Tenant-Id' header is required.");
                await context.Response.WriteAsJsonAsync(errorResponse);
                return;
            }

            // Set current tenant into scoped context
            tenantContext.SetTenantId(tenantId);

            //Continue down the pipeline
            await _next(context);
        }
    }
}
