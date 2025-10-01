using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBlazorAutoB2cApp.Shared.Services
{
	public interface IAuthenticationService
	{

		public bool IsAuthenticated { get; }
		public bool IsSignedIn { get;}
		public IAccount? User { get; }
		public string? Token { get; }
		Task<AuthenticationResult> SignInAsync();
		Task SignOutAsync();
		// Task<IAccount?> GetCachedAccountAsync();
		// Task<string?> GetCachedTokenAsync();
		Task UpdateFromCache();
	}
}
