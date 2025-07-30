# Test Coverage Pipeline with Newman Integration

This project includes a full test coverage pipeline that:
1. Runs unit tests with coverage
2. Runs integration tests with Newman for Postman collections
3. Generates a consolidated coverage report

## Running Locally

### Prerequisites
- .NET 9.0 SDK
- Node.js and npm
- Newman: `npm install -g newman newman-reporter-htmlextra`
- ReportGenerator: `dotnet tool install -g dotnet-reportgenerator-globaltool`

### Running Tests with Coverage
From the root directory:
```bash
cd tests/IntegrationTest
run-coverage.bat
```

The coverage report will be generated in `tests/IntegrationTest/coverage-report` and automatically opened in your browser.

### Newman Reports
Newman HTML reports are generated at `tests/IntegrationTest/newman-report.html` when the tests are run.

## CI/CD Integration

A GitHub Actions workflow is configured to run on push to main and pull requests. The workflow:
1. Builds the application
2. Runs all unit tests with code coverage
3. Runs integration tests with Newman
4. Generates a consolidated coverage report
5. Uploads coverage reports as artifacts

To view the results:
1. Go to the GitHub Actions tab in your repository
2. Select the latest "Test Coverage" workflow run
3. Download the artifacts to view the coverage report

## Configuration

- Coverage settings are configured in `tests/IntegrationTest/coverage.runsettings`
- GitHub Actions workflow is defined in `.github/workflows/test-coverage.yml`
- Postman collections are in `tests/IntegrationTest/Postman`
