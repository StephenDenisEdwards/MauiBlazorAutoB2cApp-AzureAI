using MauiBlazorAutoB2bApp.Shared.Services;
using Foundation;
using UIKit;

namespace MauiBlazorAutoB2bApp.iOS;

public class ParentWindowProvider : IParentWindowProvider
{
	public object GetParentWindowOrActivity()
		=> UIApplication.SharedApplication.KeyWindow; 
}