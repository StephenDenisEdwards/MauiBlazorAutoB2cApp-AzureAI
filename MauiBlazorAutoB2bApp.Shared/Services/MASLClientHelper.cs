using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace MauiBlazorAutoB2bApp.Shared.Services
//{
//	public static class MSALClientHelper
//	{
//		// Initialize once at startup
//		public static IPublicClientApplication PCA = PublicClientApplicationBuilder
//			.Create("YOUR-CLIENT-ID")
//			.WithB2CAuthority("https://<your-tenant>.b2clogin.com/tfp/<your-tenant>.onmicrosoft.com/B2C_1_SignUpSignIn")
//			.WithRedirectUri($"msal{"YOUR-CLIENT-ID"}://auth")
//			.Build();

//		// Scopes your app needs (e.g. openid + any API scopes)
//		public static string[] Scopes = { "openid", "offline_access" };
//	}
//}


public static class MSALClientHelper
{
	/*
		ClientId = 44d84416-03ea-4c42-8e3a-75a5a4439e5b
		TenantName = tinglercustomers

	*/
	//public const string Tenant = "tinglercustomers.onmicrosoft.com";
	//public const string AuthorityBase = $"https://tinglercustomers.b2clogin.com/tfp/{Tenant}";

	//public static readonly string SignUpAuthority = $"{AuthorityBase}/B2C_1_SignUp";
	//public static readonly string SignInAuthority = $"{AuthorityBase}/B2C_1_SignIn";

	//public static IPublicClientApplication PCA = PublicClientApplicationBuilder
	//	.Create("44d84416-03ea-4c42-8e3a-75a5a4439e5b")
	//	// We can seed with SignInAuthority, but we'll override at call time.
	//	.WithB2CAuthority(SignInAuthority)
	//	.WithRedirectUri($"msal{"44d84416-03ea-4c42-8e3a-75a5a4439e5b"}://auth")
	//	.Build();

	//public static readonly string[] Scopes = { "openid", "offline_access" };
}
