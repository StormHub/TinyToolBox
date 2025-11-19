using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyToolBox.Configuration.Extensions;
using TinyToolBox.Configuration.Providers;
using Microsoft.AspNetCore.Http.Json;

namespace TinyToolBox.Configuration.AspNetCore.Extensions;

internal static class HttpContextExtensions
{
    extension(HttpContext httpContext)
    {
        public async Task WriteJsonResponse(
            string? path,
            string? style,
            JsonSerializerOptions? serializerOptions,
            CancellationToken cancellationToken)
        {
            if (httpContext.RequestServices
                    .GetRequiredService<IConfiguration>() is not IConfigurationRoot configurationRoot) return;

            var segments = path?.Split('/', StringSplitOptions.RemoveEmptyEntries);

            var formatStyle = FormatStyle.Default;
            if (Enum.TryParse<FormatStyle>(style, true, out var value)) formatStyle = value;

            serializerOptions ??= httpContext.RequestServices.GetService<IOptions<JsonOptions>>()?.Value.SerializerOptions
                                  ?? new JsonOptions().SerializerOptions;

            await httpContext
                .WriteJsonResponse(configurationRoot, formatStyle, segments, serializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task WriteJsonResponse(
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

                await httpContext.Response
                    .WriteAsJsonAsync(dictionary, serializerOptions, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            var jsonNode = configurationRoot.AsJsonNode(segments, options);

            await httpContext.Response
                .WriteAsJsonAsync(jsonNode, serializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}