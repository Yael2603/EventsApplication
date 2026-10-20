using System.Diagnostics;

namespace EventsApp.API.Middleware;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers[HeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(id)
            || id.Length > 64
            || !id.All(c => char.IsLetterOrDigit(c) || c == '-'))
        {
            id = Guid.NewGuid().ToString();
        }

        context.Items[ItemKey] = id;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = id;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id }))
        {
            _logger.LogInformation("Incoming request {Method} {Path}{Query}",      // לוג לכל בקשה נכנסת; ה-CorrelationId נכנס לשורה דרך ה-Scope
                context.Request.Method, context.Request.Path, context.Request.QueryString);

            var stopwatch = Stopwatch.StartNew();       // מודד כמה זמן הבקשה לקחה

            await _next(context);                       // ממשיך לשאר ה-pipeline (אם נזרקת חריגה, ה-ExceptionMiddleware שלפנינו מטפל ומתעד אותה)

            _logger.LogInformation("Request finished {Method} {Path} with {StatusCode} in {ElapsedMs} ms",   // לוג סיום עם קוד התשובה
                context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
