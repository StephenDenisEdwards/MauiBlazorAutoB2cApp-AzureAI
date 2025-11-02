var builder = DistributedApplication.CreateBuilder(args);

// Orchestrate backends under Aspire App Host
var api = builder.AddProject("weatherapi", @"..\WebApiWeather\WebApiWeather.csproj");
var web = builder.AddProject("web", @"..\MauiBlazorAutoB2cApp.Web\MauiBlazorAutoB2cApp.Web.csproj");

// MAUI app is launched separately (device/emulator), but we include it for shared config visibility
// builder.AddProject<Projects.MauiBlazorAutoB2cApp>("mauiblazorautob2capp");

builder.Build().Run();
