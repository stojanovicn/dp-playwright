using System;
using Microsoft.Extensions.Configuration;

namespace SpaPerfTests
{
	public sealed class TestConfig
	{
		public string? AppBaseUrl { get; init; }
		public string? SsoTenant { get; init; }
		public string? SsoUsername { get; init; }
		public string? SsoPassword { get; init; }
		public bool AllowExternalTests { get; init; }

		public static TestConfig Load()
		{
			var builder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: true)
				.AddJsonFile("appsettings.Development.json", optional: true)
				.AddUserSecrets(typeof(TestConfig).Assembly, optional: true)
				.AddEnvironmentVariables();

			var cfg = builder.Build();
			return new TestConfig
			{
				AppBaseUrl = cfg["APP_BASE_URL"],
				SsoTenant = cfg["SSO_TENANT"],
				SsoUsername = cfg["SSO_USERNAME"],
				SsoPassword = cfg["SSO_PASSWORD"],
				AllowExternalTests = string.Equals(cfg["ALLOW_EXTERNAL_TESTS"], "true", StringComparison.OrdinalIgnoreCase)
			};
		}
	}
}
