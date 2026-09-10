namespace API.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly RequestDelegate _next;
        public ApiKeyAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            var configuredApiKey = configuration["Security:ApiKey"];

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey) ||
                !string.Equals(providedKey, configuredApiKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Unauthorized",
                    status = 401,
                    detail = "Missing or invalid API Key."
                });
                return;
            }

            await _next(context);
        }
    }
}
