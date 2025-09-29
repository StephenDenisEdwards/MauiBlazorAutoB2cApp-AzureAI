using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiBlazorAutoB2bApp.Shared.Services;

namespace MauiBlazorAutoB2bApp.Web.Services
{
	public class MsalWebAuthenticationService : IAuthenticationService
	{
		public bool IsAuthenticated { get; private set; }
		public bool IsSignedIn { get; private set; }
		public IAccount? User { get; private set; }
		public string? Token { get; private set; }

		public MsalWebAuthenticationService()
		{
			Debug.WriteLine("MsalWebAuthenticationService cTor");
		}

		public async Task<AuthenticationResult> SignInAsync()
		{
			Debug.WriteLine("MsalWebAuthenticationService SignInAsync");
			await Task.CompletedTask;
			throw new OperationCanceledException(
				"SignIn has been redirected; please wait for the authentication state to update.");
		}

		public async Task SignOutAsync()
		{
			Debug.WriteLine("MsalWebAuthenticationService SignOutAsync");
			await Task.CompletedTask;
		}

		public async Task UpdateFromCache()
		{
			Debug.WriteLine("MsalWebAuthenticationService UpdateFromCache");
		}
	}
}
