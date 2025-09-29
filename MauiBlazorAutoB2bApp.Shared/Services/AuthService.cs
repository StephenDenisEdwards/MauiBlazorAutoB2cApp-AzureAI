//using Microsoft.Identity.Client;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.IdentityModel.Abstractions;

//namespace MauiBlazorAutoB2bApp.Shared.Services
//{
//	public class AuthService : IAuthService
//	{
//		private readonly IParentWindowProvider _parentProvider;
//		private IPublicClientApplication _pca; //= MSALClientHelper.PCA;
//		//private readonly string[] _scopes = { }; //{ "openid", "offline_access" };
//		private readonly string[] _scopes = { "openid", "offline_access" };
//		//private readonly string[] _scopes = { "openid" };

//		//static readonly string[] Scopes = { "openid", "offline_access" };




//		public const string Tenant = "tinglercustomers";
//		public const string AuthorityBase = $"https://{Tenant}.b2clogin.com/tfp/{Tenant}.onmicrosoft.com";
//		//public const string AuthorityBase = $"https://{Tenant}.ciamlogin.com/{Tenant}.onmicrosoft.com";

//		//public static readonly string SignUpAuthority = $"{AuthorityBase}/B2C_1_SignUp";
//		//public static readonly string SignInAuthority = $"{AuthorityBase}/B2C_1_SignIn";

//		//public static readonly string SignInAuthority = $"{AuthorityBase}/B2C_1_SignUpSignIn";

//		//
//		//
//		public static readonly string UserFlowId = "49c13c68-0e9e-46e7-a599-c9123cc0cd5b";

//		//public static readonly string UserFlowId = "Quickstart%20User%20Flow%20ti1fkm";
//		//public static readonly string UserFlowId = "Quickstart User Flow ti1fkm";


//		public static readonly string SignInAuthority = $"{AuthorityBase}/{UserFlowId}";

//		public static readonly string ClientId = "44d84416-03ea-4c42-8e3a-75a5a4439e5b";


//		public string GetClientId()
//		{
//			return ClientId;
//		}


//		public string GetSignInAuthority()
//		{
//			return SignInAuthority;
//		}

//		public AuthService(IParentWindowProvider parentProvider)
//		{
//			_parentProvider = parentProvider;

//			Debug.WriteLine("AuthService");
			
//			//_scopes = MSALClientHelper.Scopes;
//			// _pca = MSALClientHelper.PCA;


//			//_pca = PublicClientApplicationBuilder
//			//	.Create(ClientId)
//			//	// We can seed with SignInAuthority, but we'll override at call time.
//			//	.WithB2CAuthority(SignInAuthority)
//			//	.WithRedirectUri($"msal{ClientId}://auth")
//			//	.Build();
//		}
//		// existing fields...

//		public async Task<bool> SignInAsync()
//		{
//			try
//			{
//				var parentWindowOrActivity = _parentProvider.GetParentWindowOrActivity();

//				//_pca = PublicClientApplicationBuilder
//				//	.Create("44d84416-03ea-4c42-8e3a-75a5a4439e5b")
//				//	.WithB2CAuthority("https://tinglercustomers.b2clogin.com/tfp/tinglercustomers.onmicrosoft.com/49c13c68-0e9e-46e7-a599-c9123cc0cd5b")
//				//	.WithRedirectUri("msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth")
//				//	.Build();

//				//_pca = PublicClientApplicationBuilder
//				//	.Create("44d84416-03ea-4c42-8e3a-75a5a4439e5b")
//				//	.WithB2CAuthority("https://tinglercustomers.b2clogin.com/tfp/tinglercustomers.onmicrosoft.com/tingler-app-userflow")
//				//	.WithRedirectUri("msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth")
//				//	.Build();

//				//_pca = PublicClientApplicationBuilder
//				//	.Create("44d84416-03ea-4c42-8e3a-75a5a4439e5b")
//				//	.WithB2CAuthority("https://tinglercustomers.ciamlogin.com/tinglercustomers.onmicrosoft.com/tingler-app-userflow")
//				//	.WithRedirectUri("msal44d84416-03ea-442-8e3a-75a5a4439e5b://auth")
//				//	.Build();

//				_pca = PublicClientApplicationBuilder
//					.Create("44d84416-03ea-4c42-8e3a-75a5a4439e5b")
//					.WithExperimentalFeatures() // this is for upcoming logger
//					.WithAuthority("https://tinglercustomers.ciamlogin.com", "tinglercustomers.onmicrosoft.com")
//					.WithRedirectUri("msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth") // 
//					.WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false)    // This is the currently recommended way to log MSAL message. For more info refer to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging. Set Identity Logging level to Warning which is a middle ground
//					//.WithIosKeychainSecurityGroup("com.microsoft.adalcache")
//					.Build();


//				/*
//					WithIosKeychainSecurityGroup 
//					configures MSAL to store its token cache in a specific iOS Keychain security 
//					group. This is useful when you want to share credentials securely between your main app and any app 
//					extensions (or other related apps) that are part of the same keychain access group. Essentially, it 
//					tells MSAL which keychain group to use for persistent token storage on iOS, helping ensure both security 
//					and interoperability among your apps if needed.
//				 */


//				var builder = _pca.AcquireTokenInteractive(_scopes)
//					.WithParentActivityOrWindow(parentWindowOrActivity);

//				var result = await builder.ExecuteAsync();
//				return !string.IsNullOrEmpty(result.AccessToken);
//			}
//			catch (MsalException msalEx)
//			{
//				Debug.WriteLine($"MSAL Exception: {msalEx.Message}");
//				return false;
//			}
//			catch (Exception ex)
//			{
//				Debug.WriteLine($"General Exception: {ex.Message}");
//				return false;
//			}
//		}

//		public async Task<bool> SignInAsync_1()
//		{
////			var account = (await _pca.GetAccountsAsync()).FirstOrDefault();
//			try
//			{
//				// First attempt: interactive authentication

//				var parentWindowOrActivity = _parentProvider.GetParentWindowOrActivity();

//				//var builder = _pca
//				//	.AcquireTokenInteractive(_scopes)
//				//	.WithB2CAuthority(MSALClientHelper.SignInAuthority)
//				//	.WithParentActivityOrWindow(parentWindowOrActivity)
//				//	.WithUseEmbeddedWebView(true);


//				_pca = PublicClientApplicationBuilder
//					.Create(ClientId)
//					// We can seed with SignInAuthority, but we'll override at call time.
//					.WithB2CAuthority(SignInAuthority)
//					.WithRedirectUri($"msal{ClientId}://auth")
//					.Build();

//				var builder = _pca
//					.AcquireTokenInteractive(_scopes)
//					//.WithB2CAuthority(SignInAuthority)
//					.WithParentActivityOrWindow(parentWindowOrActivity);

//				/*
//					When we call await builder.ExecuteAsync(), the task will only complete after the browser-based sign-in 
//					process is finished. The method execution is suspended while the user interacts with the B2C sign-in 
//					UI (whether in a WebView or an external browser). Once the user completes the sign-in and control is 
//					returned (via the authentication continuation mechanism), the authentication result is processed, 
//					and ExecuteAsync() returns, allowing the SignInAsync() method to complete.

//					I get a URL that looks like this (and no sign-in window appears): https://tinglercustomers.b2clogin.com/tfp/tinglercustomers.onmicrosoft.com/B2C_1_SignIn/oauth2/v2.0/authorize?scope=openid%20offline_access%20profile&response_type=code&client_id=44d84416-03ea-4c42-8e3a-75a5a4439e5b&redirect_uri=msal44d84416-03ea-4c42-8e3a-75a5a4439e5b%3A%2F%2Fauth&client-request-id=37423ee3-6f2d-42c9-90c1-ab73ef82198b&x-client-SKU=MSAL.Xamarin.Android&x-client-Ver=4.72.1.0&x-client-OS=35&x-client-DM=sdk_gphone64_x86_64&prompt=select_account&code_challenge=8La4q7DurDkb2gJWCSWnw5hHqh0j1bohCelbp5MRy4o&code_challenge_method=S256&state=61c467d7-7244-47c7-ae3e-551ef923bc6af81d88c0-b2cf-42e1-854a-d22764168783&client_info=1&haschrome=1

//					The URL you’re seeing is correct—it’s the authorization endpoint that MSAL constructs for initiating the B2C sign‑in flow. When using system browsers (or custom tabs), MSAL opens this URL in an external app rather than embedding the UI within your app. If no sign‑in window appears, consider that:
//					   •	System Browser Behavior: On Android, MSAL launches the system browser or a custom tab (if available). The URL parameters (like haschrome=1) indicate that a custom tab is attempted.
//					   •	Parent Window/Activity: Ensure your implementation of IParentWindowProvider on Android returns a valid activity (typically the current MainActivity). An invalid or missing activity might cause the sign‑in UI not to appear.
//					   •	Redirect URI & MsalActivity Setup: Verify that your redirect URI (e.g., msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth) in both your MSAL configuration and your Android manifest (via MsalActivity) matches exactly. Any discrepancy can prevent the callback from being handled correctly.
//					   Double-check these configurations to ensure that the browser (or custom tab) is launched as expected and is able to return control to your app.

//					So that URL results in the message: The resource you are looking for has been removed, had its name changed, or is temporarily unavailable.

//					This error typically means that the URL being hit isn't correctly configured in Azure B2C. Here are some steps to troubleshoot:
//					   •	Verify the Policy Name:
//					   Ensure that the sign‑in policy "B2C_1_SignIn" exists in your Azure AD B2C tenant and that its name is used exactly (including case) in your authority URL.
//					   •	Confirm the Authority URL:
//					   Your configured authority is
//					   https://tinglercustomers.b2clogin.com/tfp/tinglercustomers.onmicrosoft.com/B2C_1_SignIn
//					   Double‑check in your Azure portal under your B2C policies that this URL is correct. Note that some tenants might use an updated base URL (e.g., without /tfp/ in newer configurations).
//					   •	Redirect URI Settings:
//					   Verify that the redirect URI (msal44d84416-03ea-4c42-8e3a-75a5a4439e5b://auth) is registered correctly in the Azure portal. A mismatch can sometimes lead to errors.
//					   •	Manually Test the URL:
//					   Copy the URL (without the dynamic query parameters) and paste it into a browser. If it returns the same error, the issue is on the service side rather than in the app.


//				 */
//				var result = await builder.ExecuteAsync();

//				return !string.IsNullOrEmpty(result.AccessToken);
//			}
//			catch (MsalUiRequiredException)
//			{
//				// Fallback: attempt interactive flow with parent window configuration
//				try
//				{
//					var builder = _pca.AcquireTokenInteractive(_scopes)
//						.WithB2CAuthority(SignInAuthority);
//					builder = ConfigureParentWindow(builder);
//					var result = await builder.ExecuteAsync();
//					return !string.IsNullOrEmpty(result.AccessToken);
//				}
//				catch (MsalException msalEx)
//				{
//					Debug.WriteLine($"MSAL Exception in fallback: {msalEx.Message}");
//					// Optionally report to the UI via a notification service
//					return false;
//				}
//				catch (Exception ex)
//				{
//					Debug.WriteLine($"General Exception in fallback: {ex.Message}");
//					return false;
//				}
//			}
//			catch (MsalException msalEx)
//			{
//				Debug.WriteLine($"MSAL Exception: {msalEx.Message}");
//				return false;
//			}
//			catch (Exception ex)
//			{
//				Debug.WriteLine($"General Exception: {ex.Message}");
//				return false;
//			}
//		}

//		public async Task<bool> SignUpAsync()
//		{
//			// Always interactive for sign-up
//			var builder = _pca.AcquireTokenInteractive(_scopes)
//				.WithB2CAuthority(SignInAuthority);
//			builder = ConfigureParentWindow(builder);
//			var result = await builder.ExecuteAsync();
//			return !string.IsNullOrEmpty(result.AccessToken);
//		}

//		public async Task SignOutAsync()
//		{
//			var accounts = await _pca.GetAccountsAsync();
//			foreach (var acct in accounts)
//				await _pca.RemoveAsync(acct);
//		}

//		private AcquireTokenInteractiveParameterBuilder ConfigureParentWindow(
//			AcquireTokenInteractiveParameterBuilder builder)
//		{
//#if ANDROID
//        return builder;
//#elif IOS
//        return builder.WithParentActivityOrWindow(UIKit.UIApplication.SharedApplication.KeyWindow);
//#elif WINDOWS
//        var window = Application.Current.Windows[0].Handler.PlatformView as global::Microsoft.UI.Xaml.Window;
//        return builder.WithParentActivityOrWindow(window);
//#else
//			return builder;
//#endif
//		}
//	}
//}


