using CarFix.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace CarFix.API.Middleware
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



        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, message) = ex switch
            {
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UnauthorizedAction"),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                ForbiddenException => (HttpStatusCode.Forbidden, ex.Message),
                BadRequestException => (HttpStatusCode.BadRequest, ex.Message),
                ConflictException => (HttpStatusCode.Conflict, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Unexpected error occurred , Please try again later")
            };

            
            _logger.LogError(ex, "Unhandled exception occurred");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new { error = message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

