// /MyApp.Shared/Services/IParentWindowProvider.cs
namespace MauiBlazorAutoB2cApp.Shared.Services
{
	/// <summary>
	/// Returns the current host on each platform that MSAL needs
	/// (.NET object: on Android an Activity, on Windows a Window, on iOS an NSObject).
	/// </summary>
	public interface IParentWindowProvider
	{
		object GetParentWindowOrActivity();
	}
}