//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Util;
//using Microsoft.Identity.Client;

//namespace MauiBlazorAutoB2bApp
//{
//	/*
//		Why MsalActivity is needed:
//			MSAL triggers an external browser redirect for interactive sign-in on Android. The MsalActivity captures 
//			the redirect URI intent and forwards it back to the MSAL library. Without it, MSAL cannot resume and complete 
//			the authentication flow. On Windows, MAUI handles this for you via the WebView, so no custom activity is 
//			required.

//	*/

//	//[Activity(
//	//	Name = "com.tingler.tinglercustomers.MsalActivity",
//	//	Exported = true,
//	//	NoHistory = true,
//	//	LaunchMode = LaunchMode.SingleTask,
//	//	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

//	[Activity(Exported = true)]
//	[IntentFilter(
//		new[] { Intent.ActionView },
//		Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
//		DataScheme = "msal44d84416-03ea-4c42-8e3a-75a5a4439e5b", // msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth
//		DataHost = "auth")]
//	public class MsalActivity : Activity
//	{
//		public static MsalActivity Instance { get; private set; }

//		protected override void OnCreate(Bundle savedInstanceState)
//		{
//			// Inside a method, for example in OnCreate:
//			Log.Debug("MsalActivity", "Debug message: Start OnCreate() called.");

//			Instance = this;
//			base.OnCreate(savedInstanceState);

//			//// Pass the incoming intent to MSAL to complete authentication
//			//AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(
//			//	requestCode: 0,
//			//	resultCode: Result.Ok,
//			//	data: Intent);

//			//// Finish this activity
//			//Finish();
//			Log.Debug("MsalActivity", "Debug message: End OnCreate() called.");
//		}

//		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
//		{
//			Log.Debug("MsalActivity", "Debug message: Start OnActivityResult() called.");
//			base.OnActivityResult(requestCode, resultCode, data);
//			//AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(
//			//	requestCode,
//			//	resultCode,
//			//	data);
//			Log.Debug("MsalActivity", "Debug message: End OnActivityResult() called.");
//		}

//		protected override void OnNewIntent(Intent intent)
//		{
//			Log.Debug("MsalActivity", "Debug message: Start OnNewIntent() called.");
//			base.OnNewIntent(intent);

//			//// Pass the redirect back into MSAL
//			//AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(
//			//	requestCode: 0,
//			//	resultCode: Result.Ok,
//			//	data: intent);

//			//// Finish this activity so MAUI can resume
//			//Finish();
//			Log.Debug("MsalActivity", "Debug message: End OnNewIntent() called.");
//		}
//	}
//}
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Microsoft.Identity.Client;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;

//namespace MauiBlazorAutoB2bApp.Platforms.Android
//{

//		[Activity(Exported = true)]
//		[IntentFilter(new[] { Intent.ActionView },
//			Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
//			DataHost = "auth",
//			DataScheme = "msal952374f2-2386-4541-a5ba-62023c022f18")]
//		public class MsalActivity : BrowserTabActivity
//		{
//		}
//}
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Microsoft.Identity.Client;

//namespace MauiBlazorAutoB2bApp
//{
//	// This Activity must be exported so that the system can launch it.
//	[Activity(
//		Name = "com.tingler.tinglercustomers.MsalActivity",
//		Exported = true,
//		NoHistory = true,
//		LaunchMode = LaunchMode.SingleTask,
//		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
//	[IntentFilter(
//		new[] { Intent.ActionView },
//		Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
//		DataScheme = "msal44d84416-03ea-4c42-8e3a-75a5a4439e5b",  // msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth
//		DataHost = "auth")]
//	public class MsalActivity : Activity
//	{
//		public MsalActivity()
//		{
//			var x = 1;
//		}

//		public static MsalActivity Instance { get; private set; }
//		protected override void OnCreate(Bundle savedInstanceState)
//		{

//			Instance = this;       // capture the activity

//			base.OnCreate(savedInstanceState);


//			// Pass the incoming intent to MSAL so it can complete authentication
//			AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(
//				requestCode: 0,
//				resultCode: Result.Ok,
//				data: this.Intent);

//			// Finish this activity once MSAL has the data
//			Finish();

//			base.OnCreate(savedInstanceState);
//		}

//		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
//		{
//			base.OnActivityResult(requestCode, resultCode, data);

//			// Forward the result data to MSAL
//			AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(
//				requestCode,
//				resultCode,
//				data);
//		}
//	}
//}
