using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.OS;
using Android.Util;
using Microsoft.Identity.Client;

namespace MauiBlazorAutoB2bApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
	                       ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	private const string TAG = "MainActivity";
	public static MainActivity Instance { get; private set; }

	protected override void OnCreate(Bundle savedInstanceState)
	{
		Log.Debug(TAG, "Debug message: Start OnCreate() called.");

		Instance = this;       // capture the activity

		//Console.WriteLine("MainActivity.OnCreate called");
		base.OnCreate(savedInstanceState);

		// configure platform specific params
		//PlatformConfig.Instance.RedirectUri = $"msal{PublicClientSingleton.Instance.MSALClientHelper.AzureAdConfig.ClientId}://auth";
		//PlatformConfig.Instance.ParentWindow = this;
		Log.Debug(TAG, "Debug message: End OnCreate() called.");
	}

	protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
	{
		Log.Debug(TAG, "Debug message: Start OnActivityResult() called.");
		base.OnActivityResult(requestCode, resultCode, data);

		// Log the raw values
		Log.Debug(TAG, $"OnActivityResult → requestCode={requestCode}");
		Log.Debug(TAG, $"OnActivityResult → resultCode={resultCode}");

		if (data != null)
		{
			// If you want the URI that came back:
			var returnedUri = data.Data?.ToString() ?? "<no Data URI>";
			Log.Debug(TAG, $"OnActivityResult → Intent.Data = {returnedUri}");

			// If you want to dump all extras:
			var extras = data.Extras;
			if (extras != null && extras.KeySet().Count > 0)
			{
				foreach (var key in extras.KeySet())
				{
					var value = extras.Get(key)?.ToString() ?? "<null>";
					Log.Debug(TAG, $"OnActivityResult → extra[{key}] = {value}");
				}
			}
			else
			{
				Log.Debug(TAG, "OnActivityResult → Intent.Extras = <none>");
			}
		}
		else
		{
			Log.Debug(TAG, "OnActivityResult → Intent data = null");
		}


		AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
		Log.Debug("MainActivity", "Debug message: End OnActivityResult() called.");
	}
}


// /MyApp.Android/Services/ParentWindowProvider.cs