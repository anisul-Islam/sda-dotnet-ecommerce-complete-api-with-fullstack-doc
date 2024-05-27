public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation($"Handling request: {context.Request.Method} {context.Request.Path}");
        try
        {
            await _next(context);
        }
        finally
        {
            _logger.LogInformation($"Finished handling request. Response Status: {context.Response.StatusCode}");
        }
    }
}
