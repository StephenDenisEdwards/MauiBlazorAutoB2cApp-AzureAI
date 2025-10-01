using System.Diagnostics;
using MauiBlazorAutoB2cApp.Shared.Services;

namespace MauiBlazorAutoB2bApp.Web.Services;

public class FormFactor : IFormFactor
{
	public FormFactor()
	{
		Debug.WriteLine("FormFactor cTor");
	}
	public string GetFormFactor()
    {
        return "Web";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }
}