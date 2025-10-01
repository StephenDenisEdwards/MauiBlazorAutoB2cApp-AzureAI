using MauiBlazorAutoB2cApp.Shared.Services;

namespace MauiBlazorAutoB2cApp.Services;

public class NativeNavigationService : INativeNavigationService
{
	public void NavigateToNativePage()
	{
		// Assuming the current MainPage supports navigation
		Application.Current.MainPage?.Navigation.PushAsync(new NativePage());
	}
}
