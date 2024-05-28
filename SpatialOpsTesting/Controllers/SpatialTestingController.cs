using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using SpatialOpsTesting.Operations;

namespace SpatialOpsTesting.Controllers
{
    [ApiController]
    public class SpatialTestingController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<SpatialTestingController> _logger;

        public SpatialTestingController(ILogger<SpatialTestingController> logger)
        {
            _logger = logger;

        }

        [HttpGet("[controller]/GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPut("[controller]/UnionPolygons")]
        public string PutPolys([FromBody] IEnumerable<string> requestedWKTs)
        {
            var converter = new ExternalFormatConverter();
            var operations = new PolygonCombiner();

            var geometries = (from p in requestedWKTs
                              select converter.FromWKT(p)).ToList();

            var result = operations.CombinePolygons(geometries, AreaType.WKT);

            return result;
        }
    }
}
