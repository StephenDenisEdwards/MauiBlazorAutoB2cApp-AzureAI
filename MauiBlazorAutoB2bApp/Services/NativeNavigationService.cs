using MauiBlazorAutoB2bApp;
using MauiBlazorAutoB2bApp.Services;
using MauiBlazorAutoB2bApp.Shared.Services;

namespace MauiBlazorAutoB2bApp.Services;

public class NativeNavigationService : INativeNavigationService
{
	public void NavigateToNativePage()
	{
		// Assuming the current MainPage supports navigation
		Application.Current.MainPage?.Navigation.PushAsync(new NativePage());
	}
}
