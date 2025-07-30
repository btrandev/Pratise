@echo off
echo Running tests with coverage...

REM Clean previous results
if exist "coverage-results" rmdir /s /q coverage-results
if exist "coverage-report" rmdir /s /q coverage-report

REM Run unit tests with coverage
dotnet test ../AdminService.Tests/AdminService.Tests.csproj --collect:"XPlat Code Coverage" --results-directory ./coverage-results
dotnet test ../Common.Middleware.Tests/Common.Middleware.Tests.csproj --collect:"XPlat Code Coverage" --results-directory ./coverage-results

REM Install Newman reporters if needed
call npm list -g newman-reporter-htmlextra > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Installing newman-reporter-htmlextra...
    call npm install -g newman-reporter-htmlextra
)

REM Run integration tests with Newman
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage-results

REM Install ReportGenerator if not already installed
dotnet tool list -g | findstr reportgenerator > nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Installing ReportGenerator...
    dotnet tool install -g dotnet-reportgenerator-globaltool
)

REM Generate coverage report
reportgenerator -reports:./coverage-results/**/coverage.cobertura.xml -targetdir:./coverage-report -reporttypes:Html;Cobertura

REM Open the report
start "" ./coverage-report/index.html
