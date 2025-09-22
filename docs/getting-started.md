# Getting Started

## Prerequisites
- .NET SDK 8.0+
- Bash/PowerShell
- Internet for first-time browser download (Playwright)
- Access to Storybook application (for external testing)

## Installation

### 1. Clone and Restore Dependencies
```bash
git clone <repository-url>
cd dp-playwright/src
dotnet restore
```

### 2. Install Playwright Browsers
```bash
# Install Playwright CLI if not already installed
dotnet tool install --global Microsoft.Playwright.CLI

# Install Chromium browser
~/.dotnet/tools/playwright install chromium
```

### 3. Configure User Secrets (Required for SSO)
```bash
cd src/SpaPerfTests
dotnet user-secrets init
dotnet user-secrets set APP_BASE_URL "https://docs.asee.io/adaptive-components"
dotnet user-secrets set ALLOW_EXTERNAL_TESTS "true"
dotnet user-secrets set SSO_TENANT "Asseco_SRB"
dotnet user-secrets set SSO_USERNAME "your.username@example.com"
dotnet user-secrets set SSO_PASSWORD "your-password"
```

## Running Tests

### 1. Run All Tests
```bash
cd src
dotnet test
```

### 2. Run Specific Test Categories
```bash
# Run only performance tests
dotnet test --filter "SPA navigation meets memory and CPU budgets"

# Run only SSO authentication tests
dotnet test --filter "Login to Storybook docs through SSO"

# Run with verbose output
dotnet test --logger "console;verbosity=normal"
```

### 3. Run with Custom Configuration
```bash
# Override base URL for different environments
export APP_BASE_URL="https://your-storybook-url.com"
export ALLOW_EXTERNAL_TESTS="true"
dotnet test

# Enable visual debugging (browser window visible)
export HEADED="true"
export SLOWMO_MS="1000"
dotnet test
```

## Performance Testing

### Metrics Collected
- **JS Heap Used**: Current JavaScript memory usage
- **JS Heap Total**: Total JavaScript heap size
- **Task Duration**: Main thread task execution time
- **Memory Delta**: Memory change between page loads

### Performance Test Configuration
```bash
# Enable CPU throttling for realistic performance testing
export CPU_THROTTLE_RATE="4"  # 4x slower than normal

# Run performance tests with custom settings
export APP_BASE_URL="https://docs.asee.io/adaptive-components"
export ALLOW_EXTERNAL_TESTS="true"
export CPU_THROTTLE_RATE="2"
dotnet test --filter "SPA navigation meets memory and CPU budgets"
```

### Generated Artifacts
- **CSV Metrics**: `artifacts/metrics/storybook_metrics_*.csv`
- **HAR Files**: `artifacts/perf/*.har` (network requests)
- **Traces**: `artifacts/perf/*_trace.zip` (Playwright traces)

## Creating New Test Scenarios

### 1. BDD Scenarios (SpecFlow)
Create new `.feature` files in `src/SpaPerfTests/SpecFlow/Features/`:

```gherkin
Feature: New Component Testing
  Scenario: Test new component functionality
    Given I open Storybook intro docs
    When I navigate to "new-component" page
    Then I should see "New Component" heading
    And I should verify component renders correctly
```

### 2. Step Definitions
Add step definitions in `src/SpaPerfTests/SpecFlow/Steps/`:

```csharp
[Given(@"I navigate to ""(.*)"" page")]
public async Task GivenINavigateToPage(string pageName)
{
    await _stepCatalog.NavigateToPageAsync(pageName);
}

[Then(@"I should verify component renders correctly")]
public async Task ThenIShouldVerifyComponentRendersCorrectly()
{
    await _stepCatalog.VerifyComponentRendersAsync();
}
```

### 3. Step Catalog Methods
Add new methods to `StepCatalog.cs`:

```csharp
public async Task NavigateToPageAsync(string pageName)
{
    var link = _page.GetByRole(AriaRole.Link, new() { Name = pageName });
    await link.ClickAsync();
    await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
}

public async Task VerifyComponentRendersAsync()
{
    var component = _page.Locator("[data-testid='component']");
    await Expect(component).ToBeVisibleAsync();
}
```

## Configuration Management

### Environment Variables
```bash
# Application settings
export APP_BASE_URL="https://docs.asee.io/adaptive-components"
export ALLOW_EXTERNAL_TESTS="true"

# Performance testing
export CPU_THROTTLE_RATE="2"
export HEADED="false"
export SLOWMO_MS="0"

# SSO credentials (use User Secrets instead)
export SSO_TENANT="Asseco_SRB"
export SSO_USERNAME="user@example.com"
export SSO_PASSWORD="password"
```

### User Secrets (Recommended)
```bash
cd src/SpaPerfTests
dotnet user-secrets set APP_BASE_URL "https://docs.asee.io/adaptive-components"
dotnet user-secrets set ALLOW_EXTERNAL_TESTS "true"
dotnet user-secrets set SSO_TENANT "Asseco_SRB"
dotnet user-secrets set SSO_USERNAME "your.username@example.com"
dotnet user-secrets set SSO_PASSWORD "your-password"
```

### Configuration Precedence
1. `appsettings*.json` files
2. User Secrets (local development)
3. Environment variables
4. Default values

## Troubleshooting

### Common Issues

**Test Timeout**
- Check if SSO credentials are properly configured
- Verify network connectivity to target application
- Increase timeout values if needed

**Browser Not Found**
```bash
~/.dotnet/tools/playwright install chromium
```

**SSO Authentication Fails**
- Verify tenant selection is correct
- Check username/password format
- Ensure User Secrets are properly set

**Performance Data Shows Zeros**
- This is normal for some pages
- Check if Performance domain is enabled in CDP
- Verify browser supports performance.memory API

### Debug Mode
```bash
# Enable visual debugging
export HEADED="true"
export SLOWMO_MS="1000"
dotnet test
```

## Safety and Best Practices

- **Forbidden APIs**: Banned by analyzers (`analyzers/BannedSymbols.txt`)
- **File I/O**: Only allowed in `artifacts/` directory
- **Locators**: Prefer `GetByRole`/`GetByLabel` over CSS selectors
- **Waits**: Use Playwright waits instead of `Thread.Sleep`
- **Deterministic**: Use `LoadState.DOMContentLoaded` for reliable waits
