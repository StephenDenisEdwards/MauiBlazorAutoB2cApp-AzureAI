using Foundation;

namespace MauiBlazorAutoB2cApp;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    // protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    protected override MauiApp CreateMauiApp() =>
	    MauiProgram.CreateMauiAppAsync().GetAwaiter().GetResult();

}
