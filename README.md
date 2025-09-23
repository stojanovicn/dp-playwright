# dp-playwright

Advanced Playwright test suite for .NET targeting Storybook applications with comprehensive performance monitoring, SSO authentication, and automated component testing. Built with .NET 8, xUnit, and SpecFlow for BDD scenarios.

## Features

- **Performance Monitoring**: Real-time JS heap usage, task duration, and memory delta tracking
- **SSO Authentication**: Automated single sign-on flow with tenant selection
- **Component Testing**: Automated testing of all Storybook components and documentation
- **CSV Export**: Detailed performance metrics exported to CSV for analysis
- **BDD Support**: SpecFlow integration for behavior-driven development
- **Error Resilience**: Robust error handling with graceful degradation

## Technology Stack

- **Language/Framework**: .NET 8 (C#), xUnit
- **Browser**: Chromium (Playwright) with precise memory tracking
- **BDD**: SpecFlow for Gherkin scenarios
- **Performance**: CDP metrics, CPU throttling, trace/HAR artifacts
- **Authentication**: SSO integration with tenant selection

## Quick Start

### Prerequisites
- .NET SDK 8.0+
- Bash/PowerShell
- Internet for first-time browser download (Playwright)
- Access to Storybook application (for external testing)

### Installation
```bash
# Clone and restore dependencies
git clone <repository-url>
cd dp-playwright/src
dotnet restore

# Install Playwright browsers
dotnet tool install --global Microsoft.Playwright.CLI
~/.dotnet/tools/playwright install chromium

# Configure SSO credentials (required)
cd SpaPerfTests
dotnet user-secrets init
dotnet user-secrets set APP_BASE_URL "https://docs.asee.io/adaptive-components"
dotnet user-secrets set ALLOW_EXTERNAL_TESTS "true"
dotnet user-secrets set SSO_TENANT "Asseco_SRB"
dotnet user-secrets set SSO_USERNAME "your.username@example.com"
dotnet user-secrets set SSO_PASSWORD "your-password"
```

### Running Tests
```bash
# Run all tests
cd src
dotnet test

# Run specific test categories
dotnet test --filter "SPA navigation meets memory and CPU budgets"
dotnet test --filter "Login to Storybook docs through SSO"

# Run with visual debugging
export HEADED="true"
export SLOWMO_MS="1000"
dotnet test
```

## Performance Monitoring

The test suite generates comprehensive performance metrics in CSV format:

### Metrics Collected
- **JS Heap Used**: Current JavaScript heap usage in bytes
- **JS Heap Total**: Total JavaScript heap size in bytes  
- **Task Duration**: Time spent on main thread tasks in milliseconds
- **Delta Heap Used**: Memory change from previous page load

### Sample Performance Data
```csv
timestamp,href,text,jsHeapUsedBytes,jsHeapTotalBytes,taskDurationMs,deltaHeapUsedBytes
2025-09-22T20:15:48.3595408Z,/adaptive-components/?path=/docs/1-intro--docs,1. Intro,81325964,248262656,0.008,0
2025-09-22T20:15:51.4585637Z,/adaptive-components/?path=/docs/about-change-log--docs,Change log,81678020,173551616,0.008,352056
```

### Performance Analysis
- **Memory Usage**: 81-97MB (stable for complex Storybook app)
- **Rendering Speed**: 0.007-0.029ms (very fast)
- **Memory Management**: Negative deltas indicate proper cleanup
- **Heavy Components**: Icons page shows 7.8MB delta (expected for icon libraries)

## Repository Structure

```
src/
├── SpaPerfTests/
│   ├── PlaywrightFixture.cs      # Browser setup with memory tracking
│   ├── CdpHelper.cs              # CDP helpers (Performance.getMetrics, CPU throttle)
│   ├── StepCatalog.cs            # Stable UI actions (OpenHome, SignIn, SeeHeading...)
│   ├── PerfSpec.cs               # SPA navigation perf test, trace/HAR export
│   ├── TestConfig.cs             # Configuration management
│   ├── SpecFlow/                 # BDD test scenarios
│   │   ├── Features/             # .feature files
│   │   ├── Steps/                # Step definitions
│   │   └── Support/              # Test context
│   └── artifacts/                # Generated test artifacts
│       ├── metrics/              # CSV performance data
│       └── perf/                 # HAR and trace files
└── docs/                         # Documentation
    ├── README.md
    ├── getting-started.md
    ├── performance-analysis.md
    └── quick-reference.md
```

## Generated Artifacts

- **Metrics**: `artifacts/metrics/*.csv` - Performance data
- **Traces**: `artifacts/perf/*.har` - Network requests
- **Debug**: `artifacts/perf/*_trace.zip` - Playwright traces


## Documentation

- **[Getting Started](docs/getting-started.md)**: Installation, configuration, and running tests
- **[Performance Analysis](docs/performance-analysis.md)**: Understanding and analyzing performance metrics
- **[Quick Reference](docs/quick-reference.md)**: Common commands and troubleshooting

## Quick Links

- **Installation**: See [Getting Started](docs/getting-started.md#installation)
- **Running Tests**: See [Getting Started](docs/getting-started.md#running-tests)
- **Performance Analysis**: See [Performance Analysis Guide](docs/performance-analysis.md)
- **Quick Commands**: See [Quick Reference](docs/quick-reference.md)
- **Adding New Tests**: See [Creating New Test Scenarios](docs/getting-started.md#creating-new-test-scenarios)
