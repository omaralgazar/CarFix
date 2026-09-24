using CarFix.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace CarFix.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception exception)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogError(
                        exception,
                        "An exception occurred after the response started.");

                    throw;
                }

                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException => (HttpStatusCode.NotFound, exception.Message),
                BadRequestException => (HttpStatusCode.BadRequest, exception.Message),
                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized, "Unauthorized action."),
                ForbiddenException => (HttpStatusCode.Forbidden, exception.Message),
                ConflictException => (HttpStatusCode.Conflict, exception.Message),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (
                    HttpStatusCode.InternalServerError,
                    "Unexpected error occurred. Please try again later.")
            };

            _logger.LogError(
                exception,
                "Unhandled exception occurred. Status code: {StatusCode}",
                (int)statusCode);

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new { error = message };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }
}