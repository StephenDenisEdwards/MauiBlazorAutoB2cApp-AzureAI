using MauiBlazorAutoB2cApp.Shared.Services;

namespace MauiBlazorAutoB2cApp.Android;
public class ParentWindowProvider : IParentWindowProvider
{
	public object GetParentWindowOrActivity()
		=> Platform.CurrentActivity;    // the static you set in MainActivity.OnCreate


}


//public class ParentWindowProvider : IParentWindowProvider
//{
//	public Func<object> GetParentWindowOrActivity()
//		=> () => Platform.CurrentActivity;
//}