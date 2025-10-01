using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Identity.Client;
using System.Diagnostics;

namespace MauiBlazorAutoB2cApp.Web.Client.Services
{
	public class MsalWebAssemblyAuthenticationService : MauiBlazorAutoB2cApp.Shared.Services.IAuthenticationService
	{
		private readonly IAccessTokenProvider _tokenProvider;
		private readonly IRemoteAuthenticationService<RemoteAuthenticationState> _remoteAuthenticationService;
		private readonly NavigationManager _navigation;

		public bool IsAuthenticated { get; private set; }
		public bool IsSignedIn { get; private set; }
		public IAccount? User { get; private set; }
		public string? Token { get; private set; }

		public MsalWebAssemblyAuthenticationService(IRemoteAuthenticationService<RemoteAuthenticationState> remoteAuthenticationService,/*IAccessTokenProvider tokenProvider,*/ NavigationManager navigation)
		{
			Debug.WriteLine("MsalWebAssemblyAuthenticationService cTor");
			// _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
			_remoteAuthenticationService = remoteAuthenticationService;
			_navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
		}

		public async Task<AuthenticationResult> SignInAsync()
		{
			Debug.WriteLine("MsalWebAssemblyAuthenticationService SignInAsync");

			//// If you have scaffolded the authentication endpoints, you can redirect to the login page.
			//_navigation.NavigateTo("authentication/login");
			//await Task.CompletedTask;
			//throw new OperationCanceledException("SignIn has been redirected; please wait for the authentication state to update.");

			//Debug.WriteLine("MsalWebAssemblyAuthenticationService SignInAsync");

			//// Create options for the sign-in process.
			//var options = new RemoteAuthenticationActionOptions<RemoteAuthenticationState>
			//{
			//	// Use the current URI as the return URL after sign-in.
			//	ReturnUrl = _navigation.Uri
			//};

			//// Initiate the sign-in process using the remote authentication service.
			//var result = await _remoteAuthenticationService.SignInAsync(options);

			//if (result.Status == RemoteAuthenticationStatus.Success)
			//{
			//	// Optionally update your service’s internal state here.
			//	IsAuthenticated = true;
			//	// You may extract a token or user info from result if needed.
			//}
			//else
			//{
			//	throw new Exception("Sign-in failed: " + result.ErrorMessage);
			//}

			//// Adjust the returned AuthenticationResult as needed.
			//return new AuthenticationResult();


			// Construct a RemoteAuthenticationState with the current URL as ReturnUrl.
			var state = new RemoteAuthenticationState
			{
				// Assuming RemoteAuthenticationState has a property ReturnUrl
				ReturnUrl = _navigation.Uri
			};


			// Create a RemoteAuthenticationContext using the parameterless constructor.
			var authContext = new RemoteAuthenticationContext<RemoteAuthenticationState>
			{
				State = new RemoteAuthenticationState
				{
					// Set the ReturnUrl property using the current URI.
					ReturnUrl = _navigation.Uri
				}
				// Note: If no Action property is available, a default action (usually "login") is assumed.
				//,
				//Action = "login" // Specify the authentication action.
			};

			var result = await _remoteAuthenticationService.SignInAsync(authContext);

			if (result.Status == RemoteAuthenticationStatus.Success)
			{
				IsAuthenticated = true;
			}
			else
			{
				throw new Exception("Sign-in failed: " + result.ErrorMessage);
			}

			return null;
		}

		public async Task SignOutAsync()
		{
			Debug.WriteLine("MsalWebAssemblyAuthenticationService SignOutAsync");
			// Similarly redirect to the logout page if you have one.
			//_navigation.NavigateTo("authentication/logout", true);
			//await Task.CompletedTask;
		}

		public async Task UpdateFromCache()
		{
			Debug.WriteLine("MsalWebAssemblyAuthenticationService UpdateFromCache");
			//var tokenResult = await _tokenProvider.RequestAccessToken(new AccessTokenRequestOptions
			//{
			//    Scopes = new[] { "https://graph.microsoft.com/User.Read" },
			//    ReturnUrl = _navigation.Uri
			//});

			//if (tokenResult.TryGetToken(out var accessToken))
			//{
			//    Token = accessToken.Value;
			//    IsAuthenticated = true;
			//    IsSignedIn = true;
			//}
			//else
			//{
			//    Token = null;
			//    IsAuthenticated = false;
			//    IsSignedIn = false;
			//}
		}
	}
}
