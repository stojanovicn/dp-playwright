using System.Threading.Tasks;
using Microsoft.Playwright;
using TechTalk.SpecFlow;

namespace SpaPerfTests.SpecFlow.Steps
{
	[Binding]
	public sealed class StorybookSsoSteps
	{
		private readonly IPage _page;
		private readonly StepCatalog _steps;
		private readonly TestConfig _cfg;

		public StorybookSsoSteps(IPage page)
		{
			_page = page;
			_steps = new StepCatalog(_page);
			_cfg = TestConfig.Load();
		}

		[Given("I open Storybook intro docs")]
		public async Task GivenOpenStorybook() => await _steps.OpenStorybookIntroDocsAsync();

		[Then("I should be redirected to SSO")]
		public async Task ThenRedirectedToSso() => await _steps.SeeUrlContainsAsync("sso.asee.io");

		[When("I login via SSO")]
		public async Task WhenLoginViaSso()
		{
			await _steps.SsoSignInAsync(_cfg.SsoTenant ?? string.Empty, _cfg.SsoUsername ?? string.Empty, _cfg.SsoPassword ?? string.Empty);
		}

		[Then("I arrive on Storybook intro")]
		public async Task ThenArriveOnIntro() => await _steps.SeeUrlContainsAsync("?path=/docs/1-intro--docs");

		[Then("I open all sidebar docs")]
		public async Task ThenOpenAllSidebarDocs() => await _steps.OpenAllSidebarDocsAsync();
	}
}


