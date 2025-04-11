using System.Diagnostics;

namespace ECommerce.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"Request completed: {context.Request.Method} {context.Request.Path} - Status: {context.Response.StatusCode} - Duration: {stopwatch.ElapsedMilliseconds}ms");
            }
        }
    }
}
