using EconomyViewerWeb.Application.Exceptions;

namespace EconomyViewerWeb.Api.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
            switch (exception)
            {
                case ValidationException validationException:
                    await WriteErrorResponseAsync(
                        context,
                        StatusCodes.Status400BadRequest,
                        validationException.Message);

                    break;

                case NotFoundException notFoundException:
                    await WriteErrorResponseAsync(
                        context,
                        StatusCodes.Status404NotFound,
                        notFoundException.Message);

                    break;

                default:
                    _logger.LogError(
                        exception,
                        "An unexpected exception occurred while processing the request.");

                    await WriteErrorResponseAsync(
                        context,
                        StatusCodes.Status500InternalServerError,
                        "An unexpected error occurred.");

                    break;
            }
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode,
            message
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
