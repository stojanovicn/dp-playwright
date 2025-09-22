using System.Threading.Tasks;
using BoDi;
using Microsoft.Playwright;
using TechTalk.SpecFlow;

namespace SpaPerfTests.SpecFlow.Support
{
	[Binding]
	public sealed class PlaywrightWorld
	{
		private readonly IObjectContainer _container;
		private readonly PlaywrightFixture _fixture;
		private IBrowserContext _context = default!;
		private IPage _page = default!;

		public PlaywrightWorld(IObjectContainer container)
		{
			_container = container;
			_fixture = new PlaywrightFixture();
		}

		[BeforeScenario]
		public async Task BeforeScenario()
		{
			await _fixture.InitializeAsync();
			_context = await _fixture.CreateContextAsync();
			_page = await PlaywrightFixture.NewPageAsync(_context);
			_container.RegisterInstanceAs(_page);
		}

		[AfterScenario]
		public async Task AfterScenario()
		{
			if (_context != null)
			{
				await _context.CloseAsync();
			}
			await _fixture.DisposeAsync();
		}
	}
}


