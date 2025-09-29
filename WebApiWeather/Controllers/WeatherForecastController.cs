using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

namespace WebApiWeather.Controllers
{
	//[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		//[Authorize(Policy = "ReadScope")]
		//[RequiredScopeOrAppPermission(
		//	RequiredScopesConfigurationKey = "AzureAD:Scopes:Read",
		//	RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Read"
		//)]
		public IEnumerable<WeatherForecast> Get()
		{
			//var userId = GetUserId(); // Ensure user ID is valid

			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		private bool IsAppMakingRequest()
		{
			if (HttpContext.User.Claims.Any(c => c.Type == "idtyp"))
			{
				return HttpContext.User.Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
			}
			else
			{
				return HttpContext.User.Claims.Any(c => c.Type == "roles") && !HttpContext.User.Claims.Any(c => c.Type == "scp");
			}
		}

		private bool RequestCanAccessToDo(Guid userId)
		{
			return IsAppMakingRequest() || (userId == GetUserId());
		}

		private Guid GetUserId()
		{
			Guid userId;

			if (!Guid.TryParse(HttpContext.User.GetObjectId(), out userId))
			{
				throw new Exception("User ID is not valid.");
			}
			return userId;
		}
	}
}
