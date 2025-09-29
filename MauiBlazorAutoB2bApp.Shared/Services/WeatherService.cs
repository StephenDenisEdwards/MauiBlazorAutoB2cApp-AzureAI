// MauiBlazorAutoB2bApp.Web/Services/WeatherService.cs

using System.Net.Http.Json;
using MauiBlazorAutoB2bApp.Shared.Models;

public class WeatherService
{
    private readonly HttpClient _http;

    public WeatherService(HttpClient http)
    {
        _http = http;
    }

    public async Task<WeatherForecast[]> GetForecastsAsync()
    {
	    try
	    {
		    var resultString = await _http.GetStringAsync("WeatherForecast");

		}
		catch (Exception e)
	    {
		    Console.WriteLine(e);
	    }

	    try
	    {
		    var result = await _http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
		    return result;
		}
		catch (Exception e)
	    {
		    Console.WriteLine(e);
		    throw;
	    }
    }
}

