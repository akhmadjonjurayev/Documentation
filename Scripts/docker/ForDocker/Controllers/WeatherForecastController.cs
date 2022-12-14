using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ForDocker.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    private readonly PersonDbContext _person;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, PersonDbContext person)
    {
        _logger = logger;
        this._person = person;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet]
    public async Task<IActionResult> GetPersons()
    {
        return Ok(await _person.Persons.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> CreatePerson([FromBody] Person person)
    {
        if (person == null || string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
            return BadRequest("error-invalid-data");
        await _person.Persons.AddAsync(person);
        if (await _person.SaveChangesAsync() > 0)
            return Ok("success-add-data");
        return Ok("error-add-data");
    }
}
