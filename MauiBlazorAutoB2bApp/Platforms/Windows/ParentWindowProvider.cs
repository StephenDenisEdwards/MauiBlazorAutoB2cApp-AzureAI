using MauiBlazorAutoB2bApp.Shared.Services;

namespace MauiBlazorAutoB2bApp.Platforms.Windows;
public class ParentWindowProvider : IParentWindowProvider
{
	public object GetParentWindowOrActivity()
		=> Application
				.Current         // Microsoft.Maui.Controls.Application
				.Windows[0]      // your MAUI Window
				.Handler
				.PlatformView    // the underlying WinUI Window
			as Window;          // Microsoft.UI.Xaml.Window
};