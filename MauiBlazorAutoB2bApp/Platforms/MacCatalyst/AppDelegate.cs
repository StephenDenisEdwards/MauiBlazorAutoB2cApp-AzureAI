using Foundation;

namespace MauiBlazorAutoB2bApp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    // protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    protected override MauiApp CreateMauiApp() =>
	    MauiProgram.CreateMauiAppAsync().GetAwaiter().GetResult();

}
