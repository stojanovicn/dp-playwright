using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace SpaPerfTests
{
	public sealed class PlaywrightFixture : IAsyncLifetime
	{
		private static readonly string[] ChromiumLaunchArgs = new[]
		{
			"--enable-precise-memory-info",
			"--js-flags=--expose-gc"
		};
		private IPlaywright _playwright = default!;
		private IBrowser _browser = default!;
		
		public string BaseUrl { get; }

		public PlaywrightFixture()
		{
			var cfg = TestConfig.Load();
			BaseUrl = (cfg.AppBaseUrl ?? Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "http://localhost:3000").TrimEnd('/');
		}

		public async Task InitializeAsync()
		{
			_playwright = await Microsoft.Playwright.Playwright.CreateAsync();

			var headed =
				string.Equals(Environment.GetEnvironmentVariable("HEADED"), "true", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(Environment.GetEnvironmentVariable("HEADLESS"), "false", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(Environment.GetEnvironmentVariable("PWDEBUG"), "1", StringComparison.OrdinalIgnoreCase);

			int? slowMo = null;
			var slow = Environment.GetEnvironmentVariable("SLOWMO_MS");
			if (int.TryParse(slow, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var slowParsed) && slowParsed > 0)
			{
				slowMo = slowParsed;
			}

			_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
			{
				Headless = !headed,
				Args = ChromiumLaunchArgs,
				SlowMo = slowMo
			});
		}

		public async Task DisposeAsync()
		{
			if (_browser != null)
			{
				await _browser.CloseAsync();
			}
			_playwright?.Dispose();
		}

		public async Task<IBrowserContext> CreateContextAsync(string? harPath = null, bool recordTrace = false)
		{
			var contextOptions = new BrowserNewContextOptions
			{
				BaseURL = BaseUrl,
			};

			if (!string.IsNullOrWhiteSpace(harPath))
			{
				EnsureArtifactsDirectory(harPath);
				contextOptions.RecordHarPath = harPath;
				contextOptions.RecordHarOmitContent = false;
			}

			var context = await _browser.NewContextAsync(contextOptions);

			if (recordTrace)
			{
				await context.Tracing.StartAsync(new TracingStartOptions
				{
					Screenshots = true,
					Snapshots = true,
					Sources = true
				});
			}

			return context;
		}

		public static async Task<IPage> NewPageAsync(IBrowserContext context)
		{
			var page = await context.NewPageAsync();
			return page;
		}

		public static void EnsureArtifactsDirectory(string path)
		{
			var fullPath = Path.GetFullPath(path);
			var artifactsRoot = Path.GetFullPath("artifacts");
			if (!fullPath.StartsWith(artifactsRoot, StringComparison.Ordinal))
			{
				throw new InvalidOperationException("File I/O must be placed under artifacts/ directory.");
			}

			string? dir = Path.GetDirectoryName(fullPath);
			if (!string.IsNullOrWhiteSpace(dir))
			{
				Directory.CreateDirectory(dir);
			}
		}
	}
}


