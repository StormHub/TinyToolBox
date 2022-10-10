using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(IConfiguration configuration, ILogger<WeatherForecastController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation($"{nameof(Get)} WeatherForecast");

        var summaries = _configuration.GetSection("WeatherForecast:Summaries").Get<string[]>();
        var activities = _configuration.GetSection("WeatherForecast:Activities").Get<Dictionary<DayOfWeek, string>>();
        if (summaries == null || activities == null)
        {
            return Enumerable.Empty<WeatherForecast>();
        }

        return Enumerable
            .Range(1, 5)
            .Select(index =>
            {
                var date = DateTime.Now.AddDays(index);
                return new WeatherForecast
                {
                    Date = date,
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = summaries[Random.Shared.Next(summaries.Length)],
                    Activity = activities[date.DayOfWeek]
                };
        })
        .ToArray();
    }
}