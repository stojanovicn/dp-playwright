using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace SpaPerfTests
{
	public sealed class StepCatalog
	{
		private readonly IPage _page;

		public StepCatalog(IPage page)
		{
			_page = page;
		}

		public async Task OpenHomeAsync()
		{
			await _page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
			await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
			await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/?$"));
		}

		public async Task SignInAsync(string username, string password)
		{
			await _page.GetByRole(AriaRole.Link, new() { Name = "Sign In" }).ClickAsync();
			await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

			await _page.GetByLabel("Username").FillAsync(username);
			await _page.GetByLabel("Password").FillAsync(password);
			await _page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

			await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
			await Expect(_page.GetByRole(AriaRole.Link, new() { Name = "Sign Out" })).ToBeVisibleAsync();
		}

		public async Task OpenStorybookIntroDocsAsync()
		{
			await _page.GotoAsync("/?path=/docs/1-intro--docs", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
			await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
			var url = _page.Url ?? string.Empty;
			if (url.Contains("sso.asee.io", System.StringComparison.OrdinalIgnoreCase))
			{
				// Redirected to SSO. Let the caller perform SSO steps next.
				return;
			}
			await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*\\?path=/docs/1-intro--docs$"));
		}

		public async Task SeeHeadingAsync(string text, int? level = null)
		{
			var locator = level is null
				? _page.GetByRole(AriaRole.Heading, new() { Name = text })
				: _page.GetByRole(AriaRole.Heading, new() { Name = text, Level = level });
			await Expect(locator).ToBeVisibleAsync();
		}

		public async Task SeeUrlContainsAsync(string expected)
		{
			await Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(expected)));
		}

		public async Task OpenAllSidebarDocsAsync()
		{
			var tree = _page.Locator("#storybook-explorer-tree");
			await Expect(tree).ToBeVisibleAsync();

			// Prepare CSV metrics file under artifacts/
			var stamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
			var csvPath = Path.Combine("artifacts", "metrics", $"storybook_metrics_{stamp}.csv");
			PlaywrightFixture.EnsureArtifactsDirectory(csvPath);
			await WriteCsvLineAsync(csvPath, "timestamp,href,text,jsHeapUsedBytes,jsHeapTotalBytes,taskDurationMs,deltaHeapUsedBytes");
			double? previousHeapUsed = null;

			// Expand all collapsible groups
			for (int i = 0; i < 10; i++)
			{
				var collapsed = tree.Locator("button[aria-expanded='false']");
				if (await collapsed.CountAsync() == 0) break;
				var toExpand = await collapsed.ElementHandlesAsync();
				foreach (var handle in toExpand)
				{
					await handle.ClickAsync();
				}
			}

			// Visit every doc link in the tree
			var selector = "a[href^='/adaptive-components/?path=']";
			var total = await tree.Locator(selector).CountAsync();
			// Limit to first 100 links to prevent extremely long test runs
			var maxLinks = Math.Min(total, 100);
			for (int i = 0; i < maxLinks; i++)
			{
				try
				{
					var links = tree.Locator(selector);
					var link = links.Nth(i);
					await link.ScrollIntoViewIfNeededAsync();
					var href = await link.GetAttributeAsync("href") ?? string.Empty;
					var text = await link.InnerTextAsync();
					await link.ClickAsync();
					// Wait for navigation to complete, but don't wait for network idle (can timeout on heavy pages)
					await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
					// Give a longer time for any lazy loading to complete
					await _page.WaitForTimeoutAsync(1000);
					await CdpHelper.ForceGcAsync(_page);
					var metrics = await CdpHelper.GetPerformanceMetricsAsync(_page);
					var delta = previousHeapUsed.HasValue ? metrics.JsHeapUsedSizeBytes - previousHeapUsed.Value : 0d;
					previousHeapUsed = metrics.JsHeapUsedSizeBytes;
					var ts = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
					var line = string.Join(',', new[]
					{
						ts,
						EscapeCsv(href),
						EscapeCsv(text),
						metrics.JsHeapUsedSizeBytes.ToString(CultureInfo.InvariantCulture),
						metrics.JsHeapTotalSizeBytes.ToString(CultureInfo.InvariantCulture),
						metrics.TaskDurationMs.ToString(CultureInfo.InvariantCulture),
						delta.ToString(CultureInfo.InvariantCulture)
					});
					await WriteCsvLineAsync(csvPath, line);
				}
				catch (Exception ex)
				{
					// Log error but continue with next link
					var errorLine = $"{DateTime.UtcNow:o},ERROR,{EscapeCsv(ex.Message)},0,0,0,0";
					await WriteCsvLineAsync(csvPath, errorLine);
				}
			}
		}

		private static async Task WriteCsvLineAsync(string path, string line)
		{
			await using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
			await using var writer = new StreamWriter(stream, new UTF8Encoding(false));
			await writer.WriteLineAsync(line);
		}

		private static string EscapeCsv(string input)
		{
			if (input is null) return string.Empty;
			var needsQuotes = input.Contains(',') || input.Contains('"') || input.Contains('\n');
			var escaped = input.Replace("\"", "\"\"");
			return needsQuotes ? $"\"{escaped}\"" : escaped;
		}

		public async Task NavigateByRoleAsync(string roleName)
		{
			await _page.GetByRole(AriaRole.Link, new() { Name = roleName }).ClickAsync();
			await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
		}

		public async Task SsoSignInAsync(string tenantOption, string username, string password)
		{
			if (string.IsNullOrWhiteSpace(tenantOption) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				throw new System.InvalidOperationException("SSO credentials not provided. Ensure SSO_TENANT, SSO_USERNAME, SSO_PASSWORD are set in User Secrets.");
			}

			// Scope all selectors to the SSO frame if present
			var ssoFrameLocator = _page.FrameLocator("iframe[src*=\"sso.asee.io\"]");
			var hasSsoFrame = await _page.Locator("iframe[src*=\"sso.asee.io\"]").CountAsync() > 0;

			// Prefer explicit Directory combobox/select by accessible name or id/name
			var directoryByRole = (hasSsoFrame ? ssoFrameLocator.GetByRole(AriaRole.Combobox, new() { Name = "Directory" }) : _page.GetByRole(AriaRole.Combobox, new() { Name = "Directory" }));
			if (await directoryByRole.CountAsync() > 0)
			{
				await Microsoft.Playwright.Assertions.Expect(directoryByRole).ToBeVisibleAsync();
				await directoryByRole.SelectOptionAsync(new SelectOptionValue { Value = tenantOption });
			}
			else
			{
				var directorySelect = hasSsoFrame ? ssoFrameLocator.Locator("select#Directory, select[name=Directory]") : _page.Locator("select#Directory, select[name=Directory]");
				if (await directorySelect.CountAsync() > 0)
				{
					await Microsoft.Playwright.Assertions.Expect(directorySelect).ToBeVisibleAsync();
					await directorySelect.SelectOptionAsync(new SelectOptionValue { Value = tenantOption });
				}
			}

			// Select tenant/domain/org if a dropdown exists
			var dropdown = await TryGetByLabelAsync("Tenant", "Domain", "Organization");
			if (dropdown is not null)
			{
				try
				{
					await dropdown.SelectOptionAsync(new SelectOptionValue { Label = tenantOption });
				}
				catch
				{
					await dropdown.SelectOptionAsync(new SelectOptionValue { Value = tenantOption });
				}
			}

			// Username / Email
			var usernameByRole = hasSsoFrame ? ssoFrameLocator.GetByRole(AriaRole.Textbox, new() { Name = "Username or Email address" }) : _page.GetByRole(AriaRole.Textbox, new() { Name = "Username or Email address" });
			if (await usernameByRole.CountAsync() > 0)
			{
				await Microsoft.Playwright.Assertions.Expect(usernameByRole).ToBeVisibleAsync();
				await usernameByRole.FillAsync(username);
			}
			else
			{
				var userInput = await TryGetByLabelAsync("Username", "Email", "User name");
				if (userInput is not null)
				{
					await Microsoft.Playwright.Assertions.Expect(userInput).ToBeVisibleAsync();
					await userInput.FillAsync(username);
				}
			}

			// Password
			var passwordByRole = hasSsoFrame ? ssoFrameLocator.GetByRole(AriaRole.Textbox, new() { Name = "Password" }) : _page.GetByRole(AriaRole.Textbox, new() { Name = "Password" });
			if (await passwordByRole.CountAsync() > 0)
			{
				await Microsoft.Playwright.Assertions.Expect(passwordByRole).ToBeVisibleAsync();
				await passwordByRole.FillAsync(password);
			}
			else
			{
				var passwordInput = await TryGetByLabelAsync("Password");
				if (passwordInput is not null)
				{
					await Microsoft.Playwright.Assertions.Expect(passwordInput).ToBeVisibleAsync();
					await passwordInput.FillAsync(password);
				}
			}

			// Remember my login (checkbox)
			var rememberCheckbox = hasSsoFrame ? ssoFrameLocator.GetByRole(AriaRole.Checkbox, new() { Name = "Remember My Login" }) : _page.GetByRole(AriaRole.Checkbox, new() { Name = "Remember My Login" });
			if (await rememberCheckbox.CountAsync() > 0)
			{
				await rememberCheckbox.CheckAsync(new LocatorCheckOptions { Force = true });
			}

			// Submit
			var loginButton = hasSsoFrame ? ssoFrameLocator.GetByRole(AriaRole.Button, new() { Name = "Login" }) : _page.GetByRole(AriaRole.Button, new() { Name = "Login" });
			if (await loginButton.CountAsync() > 0)
			{
				await loginButton.ClickAsync();
			}
			else
			{
				await ClickFirstButtonByNamesAsync("Sign In", "Log in", "Continue");
			}
			// Await domain redirect back to docs then force navigation to the target intro URL
			await _page.WaitForURLAsync(new System.Text.RegularExpressions.Regex("docs\\.asee\\.io"), new PageWaitForURLOptions { Timeout = 15000 });
			await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
			var targetIntro = "https://docs.asee.io/adaptive-components/?path=/docs/1-intro--docs";
			if (!string.Equals(_page.Url, targetIntro, System.StringComparison.OrdinalIgnoreCase))
			{
				await _page.GotoAsync(targetIntro, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
				await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
			}
		}

		private async Task<ILocator?> TryGetByLabelAsync(params string[] labels)
		{
			foreach (var label in labels)
			{
				var loc = _page.GetByLabel(label);
				if (await loc.CountAsync() > 0)
				{
					return loc;
				}
			}
			return null;
		}

		private async Task ClickFirstButtonByNamesAsync(params string[] names)
		{
			foreach (var name in names)
			{
				var btn = _page.GetByRole(AriaRole.Button, new() { Name = name });
				if (await btn.CountAsync() > 0)
				{
					await btn.ClickAsync();
					return;
				}
			}
		}
	}
}


