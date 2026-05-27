using System.Net;
using Backend.Application.Common.Results;
using Microsoft.AspNetCore.Http;

namespace Backend.Api.Middleware;

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
        if (context.Response.HasStarted)
        {
            _logger.LogError(exception, "An unhandled exception occurred after the response had already started.");
            throw exception;
        }

        _logger.LogError(exception, "An unhandled exception occurred while processing the request. RequestId={RequestId}", context.TraceIdentifier);

        context.Response.Clear();
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var result = new BaseResult(
            statusCode: HttpStatusCode.InternalServerError,
            message: "An error occurred; please try again later"
            
        );

        context.Response.Headers["X-Request-Id"] = result.RequestId;

        await context.Response.WriteAsJsonAsync(result);
    }
}
