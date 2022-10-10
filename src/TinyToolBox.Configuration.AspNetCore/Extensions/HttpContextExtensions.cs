using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyToolBox.Configuration.Extensions;
using TinyToolBox.Configuration.Providers;

#if (!NETSTANDARD2_0)
using Microsoft.AspNetCore.Http.Json;
#endif

namespace TinyToolBox.Configuration.AspNetCore.Extensions;

internal static class HttpContextExtensions
{
    public static async Task WriteJsonResponse(
        this HttpContext httpContext,
        string? path,
        string? style,
        JsonSerializerOptions? serializerOptions,
        CancellationToken cancellationToken)
    {
        if (httpContext.RequestServices.GetRequiredService<IConfiguration>() is not IConfigurationRoot configurationRoot)
        {
            return;
        }

#if NETSTANDARD2_0
        var segments = path?.Split('/').Where(x => !string.IsNullOrEmpty(x)).ToArray();
#else
        var segments = path?.Split('/', StringSplitOptions.RemoveEmptyEntries);
#endif

        var formatStyle = FormatStyle.Default;
        if (Enum.TryParse<FormatStyle>(style, ignoreCase: true, out var value))
        {
            formatStyle = value;
        }

#if (!NETSTANDARD2_0)
        serializerOptions ??= httpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value?.SerializerOptions
             ?? new JsonOptions().SerializerOptions;
#endif

        await httpContext
            .WriteJsonResponse(configurationRoot, formatStyle, segments, serializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task WriteJsonResponse(
        this HttpContext httpContext,
        IConfigurationRoot configurationRoot,
        FormatStyle formatStyle,
        string[]? segments,
        JsonSerializerOptions? serializerOptions,
        CancellationToken cancellationToken)
    {
        var options = httpContext.RequestServices.GetService<IOptions<ConfigurationProviderOptions>>()?.Value;

        if (formatStyle == FormatStyle.Flat)
        {
            var dictionary = configurationRoot.AsDictionary(segments, options);

#if NETSTANDARD2_0
            await JsonSerializer
                .SerializeAsync(httpContext.Response.Body, dictionary, serializerOptions, cancellationToken)
                .ConfigureAwait(false);
#else
            await httpContext.Response
                .WriteAsJsonAsync(dictionary, serializerOptions, cancellationToken)
                .ConfigureAwait(false);
#endif
            return;
        }

        var jsonNode = configurationRoot.AsJsonNode(segments, options);

#if NETSTANDARD2_0
        await JsonSerializer
            .SerializeAsync(httpContext.Response.Body, jsonNode, serializerOptions, cancellationToken)
            .ConfigureAwait(false);
#else
            await httpContext.Response
                .WriteAsJsonAsync(jsonNode, serializerOptions, cancellationToken)
                .ConfigureAwait(false);
#endif
    }
}
