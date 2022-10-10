using Microsoft.Extensions.Configuration.EnvironmentVariables;
using TinyToolBox.Configuration.AspNetCore.DependencyInjection;

namespace WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add configuration information builder
        builder.Services.AddConfigurationProviderOptions(options =>
        {
            options.Map<EnvironmentVariablesConfigurationProvider>("Environment Variable");
            // options.Map<EnvironmentVariablesConfigurationProvider>(provider => provider.ToString());
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();

        // Map configuration builder route
        app.MapConfigurationEndpoint()
            .ExcludeFromDescription();

        app.Run();
    }
}