using Android.App;
using Android.Runtime;

namespace MauiBlazorAutoB2cApp;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    public override void OnCreate()
    {
	    base.OnCreate();

	    // Subscribe to unhandled exception events:
	    AppDomain.CurrentDomain.UnhandledException += (s, e) =>
	    {
		    var exception = e.ExceptionObject as Exception;
		    // Log exception details or send them to a logging service.
		    System.Diagnostics.Debug.WriteLine("Unhandled exception: " + exception?.Message);
	    };

	    TaskScheduler.UnobservedTaskException += (s, e) =>
	    {
		    // Log and mark as observed to avoid application termination.
		    System.Diagnostics.Debug.WriteLine("Unobserved task exception: " + e.Exception.Message);
		    e.SetObserved();
	    };

	    AndroidEnvironment.UnhandledExceptionRaiser += (sender, e) =>
	    {
		    // Log exception details.
		    System.Diagnostics.Debug.WriteLine("AndroidEnvironment Unhandled exception: " + e.Exception.Message);
		    e.Handled = true;
	    };
    }
	//protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	protected override MauiApp CreateMauiApp() =>
	    MauiProgram.CreateMauiAppAsync().GetAwaiter().GetResult();
}
