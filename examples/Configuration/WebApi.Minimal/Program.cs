using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using TinyToolBox.Configuration.AspNetCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.MapGet("/api/weatherforecast", ([FromServices] IConfiguration configuration) =>
{
    var summaries = configuration.GetSection("WeatherForecast:Summaries").Get<string[]>();
    var activities = configuration.GetSection("WeatherForecast:Activities").Get<Dictionary<DayOfWeek, string>>();

    if (summaries == null || activities == null)
    {
        return Enumerable.Empty<WeatherForecast>();
    }

    var forecast = Enumerable
        .Range(1, 5)
        .Select(index => {
            var date = DateTime.Now.AddDays(index);
            return new WeatherForecast
                       (
                            date,
                            Random.Shared.Next(-20, 55),
                            summaries[Random.Shared.Next(summaries.Length)],
                            activities[date.DayOfWeek]
                        );
        })
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Map configuration builder route
app.MapConfigurationEndpoint()
   .ExcludeFromDescription();

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary, string? Activity)
{
}