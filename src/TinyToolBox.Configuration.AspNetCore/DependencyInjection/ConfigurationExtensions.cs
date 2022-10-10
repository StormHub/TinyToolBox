using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

#if (!NETSTANDARD2_0)
using Microsoft.AspNetCore.Routing.Patterns;
#endif

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TinyToolBox.Configuration.AspNetCore.Extensions;
using TinyToolBox.Configuration.Providers;

namespace TinyToolBox.Configuration.AspNetCore.DependencyInjection;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddConfigurationProviderOptions(
        this IServiceCollection services,
        Action<ConfigurationProviderOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        return services;
    }

    public static ConfigurationProviderOptions Map<T>(
        this ConfigurationProviderOptions options,
        Func<T, string> formatter)
        where T : IConfigurationProvider => options.AddFormatter(formatter);

    public static ConfigurationProviderOptions Map<T>(
        this ConfigurationProviderOptions options,
        string name)
        where T : IConfigurationProvider => options.AddFormatter<T>(_ => name);

#if (!NETSTANDARD2_0)
    private const string DefaultRoutePattern = "/api/configuration/{*path}";

    public static RouteHandlerBuilder MapConfigurationEndpoint(
        this IEndpointRouteBuilder endpoints,
        string pattern = DefaultRoutePattern)
    {
        var routePattern = RoutePatternFactory.Parse(pattern);
        var parameter = routePattern.Parameters.Count == 1
            ? routePattern.Parameters[0] : null;
        if (parameter == null
            || parameter.Name != "path"
            || !parameter.IsCatchAll)
        {
            throw new ArgumentException("Pattern must contain exactly one catch all '{*path}' parameter", nameof(pattern));
        }

        return endpoints.MapGet(pattern, RequestHandler);
    }

    private static Task RequestHandler(
        HttpContext httpContext, 
        [FromRoute] string? path, 
        [FromQuery] string? style, 
        CancellationToken cancellationToken) => httpContext.WriteJsonResponse(path, style, null, cancellationToken);
#endif
}
