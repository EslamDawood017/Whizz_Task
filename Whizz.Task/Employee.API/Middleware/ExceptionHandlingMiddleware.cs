using Employee.Application.Common;
using FluentValidation;

namespace Employee.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            string errorMessage;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    
                    var errors = validationException.Errors
                        .Select(e => e.ErrorMessage)
                        .Where(msg => !string.IsNullOrWhiteSpace(msg));

                    errorMessage = string.Join("; ", errors);

                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        errorMessage = "One or more validation errors occurred.";
                    }
                    break;

                case KeyNotFoundException notFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    errorMessage = notFoundException.Message;
                    break;

                case ArgumentException argumentException:
                    statusCode = StatusCodes.Status400BadRequest;
                    errorMessage = argumentException.Message;
                    break;

                default:
                    
                    _logger.LogError(exception, "An unhandled exception occurred while processing request {Path}", context.Request.Path);
                    
                    statusCode = StatusCodes.Status500InternalServerError;
                    
                    errorMessage = "An unexpected error occurred. Please try again later.";
                    break;
            }

            context.Response.StatusCode = statusCode;
            var response = ApiResponse<object>.Failure(errorMessage);
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
