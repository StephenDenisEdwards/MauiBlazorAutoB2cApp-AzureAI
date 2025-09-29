using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Maui.ApplicationModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBlazorAutoB2bApp.Shared.Services
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly IPublicClientApplication _pca;
		private readonly string[] _scopes;
		private readonly IParentWindowProvider _parent;
		private readonly string? _signUpAuthority;  // New field for sign-up

		public bool IsAuthenticated { get; private set; }
		public bool IsSignedIn { get; private set; }
		public IAccount? User { get; private set; }
		public string? Token { get; private set; }

		public AuthenticationService(
			IPublicClientApplication pca,
			IConfiguration config,
			IParentWindowProvider parent)
		{
			_pca = pca;
			_parent = parent;
			//			_scopes = config.GetSection("AzureAd:Scopes").Get<string[]>(); // Ensure 'Microsoft.Extensions.DependencyInjection' is referenced  
			//_cacheHelper = cacheHelper;

			_scopes = config.GetSection("AzureAd:Scopes")
				.GetChildren()
				.Select(section => section.Value)
				.ToArray();  // Ensure 'Microsoft.Extensions.DependencyInjection' is referenced  

			// Load the sign-up authority if available
			_signUpAuthority = config["AzureAd:SignUpAuthority"];

			// Register the cache helper with the token cache.
			//_cacheHelper.RegisterCache(_pca.UserTokenCache);

			//_cacheHelper.
		}

		// Async factory method that initializes the cache asynchronously.
		//public static async Task<AuthenticationService> CreateAsync(
		//	IPublicClientApplication pca,
		//	IConfiguration config,
		//	IParentWindowProvider parent)
		//{
		//	var instance = new AuthenticationService(pca, config, parent);
		//	await instance.InitializeCacheAsync();
		//	return instance;
		//}

		//private async Task InitializeCacheAsync()
		//{
		//	var cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "msal_cache");
		//	var storageProperties = new StorageCreationPropertiesBuilder("msalcache.dat", cacheDir)
		//		.Build();
		//	_cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
		//	_cacheHelper.RegisterCache(_pca.UserTokenCache);
		//}

		public async Task<AuthenticationResult> SignInAsync()
		{


			try
			{
				var accounts = await _pca.GetAccountsAsync();
				var result = await _pca.AcquireTokenSilent(_scopes, accounts.FirstOrDefault())
					.ExecuteAsync();

				//result.Account

				SetState(result);

				return result;
				//throw new MsalUiRequiredException("Test", "Test exception for testing purposes");

			}
			catch (MsalUiRequiredException me)
			{
				var parent = _parent.GetParentWindowOrActivity();

				AcquireTokenInteractiveParameterBuilder? aquireFunction = _pca
					.AcquireTokenInteractive(_scopes)

#if DEBUG
					// .WithUseEmbeddedWebView(true)
					.WithUseEmbeddedWebView(false)
#endif

					.WithParentActivityOrWindow(parent);

				//var result = MainThread.InvokeOnMainThreadAsync(async () =>
				//{
				//	result = await aquireFunction
				//		.ExecuteAsync();

				//});

				AuthenticationResult result = null!;
				// Ensure interactive call runs on UI thread
				//await MainThread.InvokeOnMainThreadAsync(async () =>
				//{
					result = await aquireFunction
						.ExecuteAsync()
						//.ConfigureAwait(false);
						.ConfigureAwait(true);
				//});


				SetState(result);

				return result;




				//AuthenticationResult? result = await aquireFunction
				//	.ExecuteAsync();

				//return result;

				/*

				Calling from a non-UI thread ???
				Always invoke AcquireTokenInteractive on the main thread. You can wrap it in:

				MainThread.InvokeOnMainThreadAsync(async () =>
				   {
				       await _pca.AcquireTokenInteractive(_scopes)
				                 .WithParentActivityOrWindow(...)
				                 .ExecuteAsync();
				   });

				*/

			}
		}

		public async Task UpdateFromCache()
		{
			IsAuthenticated = false;
			IsSignedIn = false;

			// Attempt to restore the user from the persistent cache.
			User = await GetCachedAccountAsync();
			Token = await GetCachedTokenAsync();

			SetAuthSignedInState();
		}

		private void SetState(AuthenticationResult result)
		{
			IsAuthenticated = false;
			IsSignedIn = false;

			User = result.Account; 
			Token = result.AccessToken; 

			SetAuthSignedInState();
		}

		private void SetAuthSignedInState()
		{
			if (User != null)
			{
				if (!string.IsNullOrEmpty(Token))
				{
					IsAuthenticated = true;
				}
				IsSignedIn = true;
			}
		}


		// MauiBlazorAutoB2bApp.Shared\Services\AuthenticationService.cs
		private  async Task<IAccount?> GetCachedAccountAsync()
		{
			var accounts = await _pca.GetAccountsAsync(); // _pca represents your PublicClientApplication instance
			return accounts.FirstOrDefault();
		}

		private async Task<string?> GetCachedTokenAsync()
		{
			try
			{
				var firstAccount = await GetCachedAccountAsync();

				if (firstAccount is not null)
				{
					var result = await _pca.AcquireTokenSilent(_scopes, firstAccount)
						.ExecuteAsync();
					// Cached token retrieved successfully
					string accessToken = result.AccessToken;

					return accessToken;
				}

				return null;
			}
			catch (MsalUiRequiredException msalUiRequiredException)
			{
				Debug.WriteLine(msalUiRequiredException.Message);

			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception.Message);
			}

			return null;
		}

		public async Task SignOutAsync()
		{
			var accounts = await _pca.GetAccountsAsync();
			foreach (var acct in accounts)
				await _pca.RemoveAsync(acct);

			await UpdateFromCache();
		}


	}
}
