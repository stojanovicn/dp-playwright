# Quick Reference

## Installation & Setup
```bash
# Clone and setup
git clone <repository-url>
cd dp-playwright/src
dotnet restore

# Install Playwright
dotnet tool install --global Microsoft.Playwright.CLI
~/.dotnet/tools/playwright install chromium

# Configure SSO (required)
cd SpaPerfTests
dotnet user-secrets init
dotnet user-secrets set APP_BASE_URL "https://docs.asee.io/adaptive-components"
dotnet user-secrets set ALLOW_EXTERNAL_TESTS "true"
dotnet user-secrets set SSO_TENANT "Asseco_SRB"
dotnet user-secrets set SSO_USERNAME "your.username@example.com"
dotnet user-secrets set SSO_PASSWORD "your-password"
```

## Running Tests
```bash
# Run all tests
cd src
dotnet test

# Run specific tests
dotnet test --filter "SPA navigation meets memory and CPU budgets"
dotnet test --filter "Login to Storybook docs through SSO"

# Run with visual debugging
export HEADED="true"
export SLOWMO_MS="1000"
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=normal"
```

## Performance Testing
```bash
# Enable CPU throttling
export CPU_THROTTLE_RATE="4"
dotnet test

# Custom base URL
export APP_BASE_URL="https://your-storybook-url.com"
export ALLOW_EXTERNAL_TESTS="true"
dotnet test
```

## Configuration
```bash
# Environment variables
export APP_BASE_URL="https://docs.asee.io/adaptive-components"
export ALLOW_EXTERNAL_TESTS="true"
export CPU_THROTTLE_RATE="2"
export HEADED="false"
export SLOWMO_MS="0"

# User Secrets (recommended)
cd src/SpaPerfTests
dotnet user-secrets set KEY "value"
```

## Generated Artifacts
- **CSV Metrics**: `artifacts/metrics/storybook_metrics_*.csv`
- **HAR Files**: `artifacts/perf/*.har`
- **Traces**: `artifacts/perf/*_trace.zip`

## Performance Analysis
```bash
# Count pages tested
wc -l artifacts/metrics/storybook_metrics_*.csv

# Find slowest pages
sort -t',' -k6 -nr artifacts/metrics/storybook_metrics_*.csv | head -10

# Calculate average memory usage
awk -F',' 'NR>1 {sum+=$4; count++} END {print "Average:", sum/count/1024/1024 "MB"}' artifacts/metrics/storybook_metrics_*.csv

# Find memory leaks (positive deltas)
awk -F',' '$7 > 0' artifacts/metrics/storybook_metrics_*.csv | wc -l
```

## Troubleshooting
```bash
# Browser not found
~/.dotnet/tools/playwright install chromium

# Check User Secrets
dotnet user-secrets list

# Clear artifacts
rm -rf artifacts/

# Debug mode
export HEADED="true"
export SLOWMO_MS="1000"
dotnet test
```

## File Structure
```
src/
├── SpaPerfTests/
│   ├── PlaywrightFixture.cs      # Browser setup
│   ├── CdpHelper.cs              # Performance metrics
│   ├── StepCatalog.cs            # UI actions
│   ├── PerfSpec.cs               # Performance tests
│   ├── TestConfig.cs             # Configuration
│   ├── SpecFlow/                 # BDD scenarios
│   │   ├── Features/             # .feature files
│   │   ├── Steps/                # Step definitions
│   │   └── Support/              # Test context
│   └── artifacts/                # Generated files
│       ├── metrics/              # CSV data
│       └── perf/                 # HAR/traces
└── docs/                         # Documentation
    ├── README.md
    ├── getting-started.md
    ├── performance-analysis.md
    └── quick-reference.md
```

## Common Issues
- **SSO Authentication Fails**: Check User Secrets configuration
- **Test Timeout**: Verify network connectivity and credentials
- **Performance Data Shows Zeros**: Normal for some pages, check CDP setup
- **Browser Not Found**: Run `~/.dotnet/tools/playwright install chromium`
