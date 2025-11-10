# Continuous Integration (CI) Pipeline

This repository uses GitHub Actions for automated building, testing, and code quality checks.

## Pipeline Overview

The CI pipeline runs on:
- Every push to `main` and feature branches (`feat/**`, `fix/**`, `refactor/**`)
- Every pull request targeting `main`
- Manual trigger via GitHub UI

## Jobs

### 1. Build and Test
**Purpose**: Verify code compiles and all tests pass

**Steps**:
- ✅ Checkout code
- ✅ Setup .NET 9.0
- ✅ Restore NuGet packages (with caching)
- ✅ Build solution in Release mode
- ✅ Run unit tests with code coverage
- ✅ Run infrastructure tests with code coverage
- ✅ Run integration tests with code coverage
- ✅ Generate HTML coverage report
- ✅ Verify coverage meets 70% threshold

**Artifacts**:
- `coverage-report/` - HTML coverage report (7 days retention)
- `test-results/` - TRX test result files (7 days retention)

### 2. Mutation Testing (Stryker.NET)
**Purpose**: Measure test quality by introducing code mutations

**Runs on**: Pull requests and main branch only

**Steps**:
- ✅ Build solution
- ✅ Run Stryker.NET mutation testing on `Finance.Domain`
- ✅ Generate mutation report (HTML + JSON)
- ✅ Upload report as artifact

**Thresholds**:
- **High**: 80% mutation score (excellent)
- **Low**: 60% mutation score (acceptable)
- **Break**: 50% mutation score (minimum)

**Note**: Mutation testing is informational and won't fail the build, but scores below thresholds should be investigated.

**Artifacts**:
- `stryker-report/` - HTML mutation testing report (7 days retention)

### 3. Code Quality Checks
**Purpose**: Enforce code style and build warnings

**Steps**:
- ✅ Verify code formatting (`.editorconfig` rules)
- ✅ Build with warnings as errors

### 4. CI Summary
**Purpose**: Provide overview of all checks

Displays summary in GitHub Actions UI showing status of:
- Build and Tests
- Code Quality
- Mutation Testing

## Coverage Requirements

**Minimum coverage threshold**: 70%

The build will fail if coverage drops below this threshold.

Current coverage includes:
- Unit tests (Finance.Domain.Tests)
- Infrastructure tests (Finance.Infrastructure.Tests)
- Integration tests (Finance.API.IntegrationTests)

## Running Locally

### Run all tests with coverage
```pwsh
# Run tests
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generate report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:CoverageReport" "-reporttypes:Html"

# Open report
start CoverageReport/index.html  # Windows
open CoverageReport/index.html   # macOS
xdg-open CoverageReport/index.html  # Linux
```

### Run mutation testing
```pwsh
cd tests/Finance.Domain.Tests
dotnet tool install -g dotnet-stryker
dotnet stryker
```

The mutation report will open automatically in your browser.

## Troubleshooting

### Build fails on formatting check
Run locally:
```pwsh
dotnet format
```

### Coverage below threshold
1. Check which files are not covered: `CoverageReport/index.html`
2. Add unit tests for uncovered code
3. Verify tests are running in CI

### Mutation testing fails
1. Download the `stryker-report` artifact from GitHub Actions
2. Open `mutation-report.html` to see which mutations survived
3. Add tests to kill surviving mutations

## Performance Optimization

**Caching**: NuGet packages are cached based on `Directory.Packages.props` hash to speed up builds.

**Parallelization**: 
- Unit, infrastructure, and integration tests run sequentially
- Mutation testing runs in parallel with code quality checks
- Stryker.NET uses 4 concurrent workers

## Branch Protection

Recommended settings for `main` branch:
- ✅ Require status checks to pass before merging
  - `Build and Test`
  - `Code Quality Checks`
- ✅ Require branches to be up to date before merging
- ✅ Require conversation resolution before merging

## GitHub Secrets

No secrets required for current CI configuration. All tools use publicly available packages.

## Future Enhancements

Planned improvements:
- [ ] E2E tests with Playwright
- [ ] SonarCloud integration for code quality metrics
- [ ] Codecov or Coveralls integration for coverage tracking
- [ ] Automatic dependency updates with Dependabot
- [ ] Performance benchmarking
- [ ] Docker image building and publishing

## Related Documentation

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Stryker.NET Documentation](https://stryker-mutator.io/docs/stryker-net/introduction/)
- [ReportGenerator Documentation](https://github.com/danielpalme/ReportGenerator)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
