using MauiBlazorAutoB2cApp.Shared.Services;
using UIKit;

namespace MauiBlazorAutoB2cApp.iOS;

public class ParentWindowProvider : IParentWindowProvider
{
	public object GetParentWindowOrActivity()
		=> UIApplication.SharedApplication.KeyWindow; 
}