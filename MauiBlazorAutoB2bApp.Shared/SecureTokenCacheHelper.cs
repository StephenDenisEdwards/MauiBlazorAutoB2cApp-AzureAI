using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Maui.Storage;

namespace MauiBlazorAutoB2bApp
{
	public class SecureTokenCacheHelper
	{
		private readonly IPublicClientApplication _clientApp;
		private readonly string _cacheKey;

		public SecureTokenCacheHelper(IPublicClientApplication clientApp, string cacheKey = "msal_token_cache")
		{
			_clientApp = clientApp;
			_cacheKey = cacheKey;
			RegisterCacheCallbacks();
		}

		private void RegisterCacheCallbacks()
		{
			_clientApp.UserTokenCache.SetBeforeAccessAsync(BeforeAccessNotificationAsync);
			_clientApp.UserTokenCache.SetAfterAccessAsync(AfterAccessNotificationAsync);
		}

		private async Task BeforeAccessNotificationAsync(TokenCacheNotificationArgs args)
		{
			try
			{
				string cacheData = await SecureStorage.Default.GetAsync(_cacheKey);
				if (!string.IsNullOrEmpty(cacheData))
				{
					byte[] data = Convert.FromBase64String(cacheData);
					args.TokenCache.DeserializeMsalV3(data);
				}
			}
			catch (Exception ex)
			{
				// Handle exceptions, e.g. log the error.
			}
		}

		private async Task AfterAccessNotificationAsync(TokenCacheNotificationArgs args)
		{
			if (args.HasStateChanged)
			{
				try
				{
					byte[] data = args.TokenCache.SerializeMsalV3();
					string cacheData = Convert.ToBase64String(data);
					await SecureStorage.Default.SetAsync(_cacheKey, cacheData);
				}
				catch (Exception ex)
				{
					// Handle exceptions, e.g. log the error.
				}
			}
		}
	}
}