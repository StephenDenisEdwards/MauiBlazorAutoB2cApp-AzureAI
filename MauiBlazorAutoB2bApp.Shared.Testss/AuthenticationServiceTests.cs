using MauiBlazorAutoB2bApp.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Moq;

namespace MauiBlazorAutoB2bApp.Shared.Tests
{
	public class AuthenticationServiceTests
	{
		// A fake builder for silent token acquisition.
		private class FakeSilentBuilder
		{
			private readonly Func<Task<AuthenticationResult>> _execute;
			public FakeSilentBuilder(Func<Task<AuthenticationResult>> execute)
			{
				_execute = execute;
			}

#if DEBUG
			public FakeSilentBuilder WithUseEmbeddedWebView(bool use)
			{
				return this;
			}
#endif
			public FakeSilentBuilder WithParentActivityOrWindow(object parent)
			{
				return this;
			}

			public Task<AuthenticationResult> ExecuteAsync()
			{
				return _execute();
			}
		}

		// A fake builder for interactive token acquisition.
		private class FakeInteractiveBuilder
		{
			private readonly Func<Task<AuthenticationResult>> _execute;
			public FakeInteractiveBuilder(Func<Task<AuthenticationResult>> execute)
			{
				_execute = execute;
			}

#if DEBUG
			public FakeInteractiveBuilder WithUseEmbeddedWebView(bool use)
			{
				return this;
			}
#endif
			public FakeInteractiveBuilder WithParentActivityOrWindow(object parent)
			{
				return this;
			}

			public Task<AuthenticationResult> ExecuteAsync()
			{
				return _execute();
			}
		}

		private IConfiguration CreateTestConfiguration()
		{
			var inMemorySettings = new Dictionary<string, string>
			{
				{ "AzureAd:Scopes:0", "scope1" },
				{ "AzureAd:Scopes:1", "scope2" },
				{ "AzureAd:SignUpAuthority", "https://dummy.authority" }
			};

			return new ConfigurationBuilder()
				.AddInMemoryCollection(inMemorySettings)
				.Build();
		}

		[Fact]
		public async Task SignIn_SilentSuccess_UpdatesState()
		{
			// Arrange
			var dummyAccount = new Mock<IAccount>().Object;
			var dummyToken = "dummy-token-silent";
			var dummyAuthResult = AuthenticationResultFactory(dummyAccount, dummyToken);
			var configuration = CreateTestConfiguration();
			var dummyParent = new Mock<IParentWindowProvider>();
			dummyParent.Setup(p => p.GetParentWindowOrActivity()).Returns(new object());

			var pcaMock = new Mock<IPublicClientApplication>();
			pcaMock.Setup(m => m.GetAccountsAsync())
				   .ReturnsAsync(new List<IAccount> { dummyAccount });
			// Return a fake builder which returns the dummy authentication result.
			pcaMock.Setup(m => m.AcquireTokenSilent(It.IsAny<string[]>(), It.IsAny<IAccount>()))
				   .Returns(() => new FakeSilentBuilder(() => Task.FromResult(dummyAuthResult)));

			var authService = new AuthenticationService(pcaMock.Object, configuration, dummyParent.Object);

			// Act
			var result = await authService.SignInAsync();

			// Assert
			Assert.NotNull(result);
			Assert.True(authService.IsSignedIn);
			Assert.True(authService.IsAuthenticated);
			Assert.Equal(dummyToken, authService.Token);
			Assert.Equal(dummyAccount, authService.User);
		}

		[Fact]
		public async Task SignIn_InteractiveSuccess_UpdatesState()
		{
			// Arrange
			var dummyAccount = new Mock<IAccount>().Object;
			var dummyToken = "dummy-token-interactive";
			var dummyAuthResult = AuthenticationResultFactory(dummyAccount, dummyToken);
			var configuration = CreateTestConfiguration();
			var dummyParent = new Mock<IParentWindowProvider>();
			var parentWindow = new object();
			dummyParent.Setup(p => p.GetParentWindowOrActivity()).Returns(parentWindow);

			var pcaMock = new Mock<IPublicClientApplication>();
			// Simulate silent failure by returning a builder that throws MsalUiRequiredException in ExecuteAsync.
			pcaMock.Setup(m => m.GetAccountsAsync())
				   .ReturnsAsync(new List<IAccount>()); // no accounts available

			pcaMock.Setup(m => m.AcquireTokenSilent(It.IsAny<string[]>(), It.IsAny<IAccount>()))
				   .Returns(() => new FakeSilentBuilder(() => throw new MsalUiRequiredException("dummy", "silent token not available")));
			// Setup interactive builder to return the dummy authentication result.
			pcaMock.Setup(m => m.AcquireTokenInteractive(It.IsAny<string[]>()))
				   .Returns(() => new FakeInteractiveBuilder(() => Task.FromResult(dummyAuthResult)));

			var authService = new AuthenticationService(pcaMock.Object, configuration, dummyParent.Object);

			// Act
			var result = await authService.SignInAsync();

			// Assert
			Assert.NotNull(result);
			Assert.True(authService.IsSignedIn);
			Assert.True(authService.IsAuthenticated);
			Assert.Equal(dummyToken, authService.Token);
			Assert.Equal(dummyAccount, authService.User);
		}

		[Fact]
		public async Task UpdateFromCache_SetsAuthState()
		{
			// Arrange
			var dummyAccount = new Mock<IAccount>().Object;
			var dummyToken = "cached-token";
			var dummyAuthResult = AuthenticationResultFactory(dummyAccount, dummyToken);
			var configuration = CreateTestConfiguration();
			var dummyParent = new Mock<IParentWindowProvider>();
			dummyParent.Setup(p => p.GetParentWindowOrActivity()).Returns(new object());

			var pcaMock = new Mock<IPublicClientApplication>();
			// Setup accounts are available.
			pcaMock.Setup(m => m.GetAccountsAsync())
				   .ReturnsAsync(new List<IAccount> { dummyAccount });
			// Setup AcquireTokenSilent to return the dummy auth result.
			pcaMock.Setup(m => m.AcquireTokenSilent(It.IsAny<string[]>(), It.IsAny<IAccount>()))
				   .Returns(() => new FakeSilentBuilder(() => Task.FromResult(dummyAuthResult)));

			var authService = new AuthenticationService(pcaMock.Object, configuration, dummyParent.Object);

			// Act
			await authService.UpdateFromCache();

			// Assert
			Assert.True(authService.IsSignedIn);
			Assert.True(authService.IsAuthenticated);
			Assert.Equal(dummyToken, authService.Token);
			Assert.Equal(dummyAccount, authService.User);
		}

		[Fact]
		public async Task SignOut_RemovesAccountsAndClearsState()
		{
			// Arrange
			var dummyAccount = new Mock<IAccount>().Object;
			var configuration = CreateTestConfiguration();
			var dummyParent = new Mock<IParentWindowProvider>();
			dummyParent.Setup(p => p.GetParentWindowOrActivity()).Returns(new object());

			var accounts = new List<IAccount> { dummyAccount };

			var pcaMock = new Mock<IPublicClientApplication>();
			// Setup GetAccountsAsync to return accounts initially.
			pcaMock.SetupSequence(m => m.GetAccountsAsync())
				   .ReturnsAsync(accounts)   // before sign out
				   .ReturnsAsync(new List<IAccount>());  // after removal

			// Setup RemoveAsync to simulate account removal.
			pcaMock.Setup(m => m.RemoveAsync(It.IsAny<IAccount>()))
				   .Returns(Task.CompletedTask)
				   .Callback<IAccount>(acct => accounts.Remove(acct));

			var authService = new AuthenticationService(pcaMock.Object, configuration, dummyParent.Object);

			// To simulate a signed in state, we set a dummy token and account via UpdateFromCache.
			await authService.UpdateFromCache();
			Assert.True(authService.IsSignedIn);

			// Act
			await authService.SignOutAsync();

			// Assert
			Assert.False(authService.IsAuthenticated);
			// Since UpdateFromCache resets the state and now there are no accounts.
			Assert.False(authService.IsSignedIn);
			Assert.Null(authService.User);
			Assert.Null(authService.Token);
		}

		// Helper to create a dummy AuthenticationResult.
		private AuthenticationResult AuthenticationResultFactory(IAccount account, string accessToken)
		{
			// MSAL's AuthenticationResult is typically internal to the library.
			// For testing purposes, you can use Moq to create a fake or use a custom dummy.
			var authResultMock = new Mock<AuthenticationResult>(
				accessToken,  // accessToken
				false,        // isExtendedLifeTimeToken
				account,      // account
				DateTimeOffset.Now,
				DateTimeOffset.Now.AddHours(1),
				"dummy-scope",
				null,         // idToken
				null,         // correlationId
				null,         // tokenType
				null          // additionalResponseParameters
			);
			authResultMock.CallBase = true;
			return authResultMock.Object;
		}
	}
}