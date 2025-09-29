using MauiBlazorAutoB2bApp.Shared.Services;

namespace MauiBlazorAutoB2bApp.Web.Client.Services;

public class FormFactor : IFormFactor
{
    public string GetFormFactor()
    {
        return "WebAssembly";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }
}
