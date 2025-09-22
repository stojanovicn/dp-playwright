using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace SpaPerfTests
{
	public sealed class PerfSpec : IClassFixture<PlaywrightFixture>
	{
		private readonly PlaywrightFixture _fx;

		public PerfSpec(PlaywrightFixture fx)
		{
			_fx = fx;
		}

		[Fact(DisplayName = "SPA navigation meets memory and CPU budgets (Chromium)")]
		public async Task SpaNavigationPerfBudgets()
		{
			var allowExternal = Environment.GetEnvironmentVariable("ALLOW_EXTERNAL_TESTS");
			if (!string.Equals(allowExternal, "true", StringComparison.OrdinalIgnoreCase))
			{
				return; // external perf disabled by default
			}
			// Run perf only on local hosts to avoid SSO/variability
			if (!_fx.BaseUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase) &&
				!_fx.BaseUrl.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
			var harPath = Path.Combine("artifacts", "perf", $"spa_{stamp}.har");
			var tracePath = Path.Combine("artifacts", "perf", $"spa_{stamp}_trace.zip");
			PlaywrightFixture.EnsureArtifactsDirectory(harPath);
			PlaywrightFixture.EnsureArtifactsDirectory(tracePath);

			var context = await _fx.CreateContextAsync(harPath: harPath, recordTrace: true);
			try
			{
				var page = await PlaywrightFixture.NewPageAsync(context);

				var throttleEnv = Environment.GetEnvironmentVariable("CPU_THROTTLE_RATE");
				if (double.TryParse(throttleEnv, NumberStyles.Float, CultureInfo.InvariantCulture, out var rate) && rate > 0)
				{
					await CdpHelper.SetCpuThrottlingRateAsync(page, rate);
				}

				await page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
				await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

				await CdpHelper.ForceGcAsync(page);

				var before = await CdpHelper.GetPerformanceMetricsAsync(page);

				await page.GetByRole(AriaRole.Link, new() { Name = "Dashboard" }).ClickAsync();
				await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

				await page.WaitForTimeoutAsync(50);

				await CdpHelper.ForceGcAsync(page);
				var after = await CdpHelper.GetPerformanceMetricsAsync(page);

				var heapBudgetBytes = 50 * 1024 * 1024;
				var taskDurationBudgetMs = 40.0;

				after.JsHeapUsedSizeBytes.Should().BeLessThan(heapBudgetBytes);
				after.TaskDurationMs.Should().BeLessThan(taskDurationBudgetMs);

				await Microsoft.Playwright.Assertions.Expect(
					page.GetByRole(AriaRole.Heading, new() { Name = "Dashboard" })
				).ToBeVisibleAsync();

				await context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
			}
			catch
			{
				try
				{
					await context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });
				}
				catch { }
				throw;
			}
			finally
			{
				await context.CloseAsync();
			}
		}
	}
}


