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

## Repository Layout

- `src/`
  - `dp-playwright.sln`: solution
  - `Directory.Build.props`: strict build settings
  - `SpaPerfTests/`
    - `SpaPerfTests.csproj`: test project
    - `PlaywrightFixture.cs`: launches Chromium with precise memory + gc
    - `CdpHelper.cs`: CDP helpers (Performance.getMetrics, CPU throttle)
    - `StepCatalog.cs`: stable UI actions (OpenHome, SignIn, SeeHeading...)
    - `PerfSpec.cs`: SPA navigation perf test, trace/HAR export
    - `SpecFlow/`: BDD test scenarios
      - `Features/StorybookSso.feature`: Gherkin scenarios
      - `Steps/StorybookSsoSteps.cs`: step definitions
      - `Support/PlaywrightWorld.cs`: test context
    - `TestConfig.cs`: configuration management
    - `artifacts/`: generated test artifacts
      - `metrics/`: CSV performance data
      - `perf/`: HAR and trace files

## Performance Data Analysis

The test suite generates comprehensive performance metrics in CSV format:

### Metrics Collected
- **JS Heap Used**: Current JavaScript heap usage in bytes
- **JS Heap Total**: Total JavaScript heap size in bytes  
- **Task Duration**: Time spent on main thread tasks in milliseconds
- **Delta Heap Used**: Memory change from previous page load

### Sample Performance Data
```
timestamp,href,text,jsHeapUsedBytes,jsHeapTotalBytes,taskDurationMs,deltaHeapUsedBytes
2025-09-22T20:15:48.3595408Z,/adaptive-components/?path=/docs/1-intro--docs,1. Intro,81325964,248262656,0.008,0
2025-09-22T20:15:51.4585637Z,/adaptive-components/?path=/docs/about-change-log--docs,Change log,81678020,173551616,0.008,352056
```

### Performance Insights
- **Memory Usage**: 81-97MB (stable for complex Storybook app)
- **Rendering Speed**: 0.007-0.029ms (very fast)
- **Memory Management**: Negative deltas indicate proper cleanup
- **Heavy Components**: Icons page shows 7.8MB delta (expected for icon libraries)

## Artifacts

- **Metrics**: `artifacts/metrics/*.csv` - Performance data
- **Traces**: `artifacts/perf/*.har` - Network requests
- **Debug**: `artifacts/perf/*_trace.zip` - Playwright traces

## Quick Start

1. **Install**: `dotnet restore` in `src/` directory
2. **Configure**: Set SSO credentials in User Secrets
3. **Run**: `dotnet test` in `src/` directory
4. **Analyze**: Check `artifacts/metrics/` for performance data

## Documentation

- **[Getting Started](getting-started.md)**: Installation, configuration, and running tests
- **[Performance Analysis](performance-analysis.md)**: Understanding and analyzing performance metrics
- **[Quick Reference](quick-reference.md)**: Common commands and troubleshooting
- **[Creating Scenarios](getting-started.md#creating-new-test-scenarios)**: Adding new test scenarios and step definitions

## Quick Links

- **Installation**: See [Getting Started](getting-started.md#installation)
- **Running Tests**: See [Getting Started](getting-started.md#running-tests)
- **Performance Analysis**: See [Performance Analysis Guide](performance-analysis.md)
- **Quick Commands**: See [Quick Reference](quick-reference.md)
- **Adding New Tests**: See [Creating New Test Scenarios](getting-started.md#creating-new-test-scenarios)
