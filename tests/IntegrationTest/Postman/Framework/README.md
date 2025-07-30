# Postman Collection Runner for .NET

This framework allows you to execute Postman collections as integration tests using .NET and xUnit.

## Features

- Run Postman collections as xUnit tests
- Support for Postman environment files and variable resolution
- Dynamic variable substitution (uuid, timestamps, random data)
- Basic support for Postman test scripts
- Automatic conversion of Postman tests to FluentAssertions
- Environment variable passing between requests

## Usage

### Basic Usage

```csharp
// Create a test class
public class MyPostmanTests
{
    private readonly HttpClient _httpClient = new HttpClient();
    
    [Fact]
    public async Task Run_My_Collection()
    {
        // Paths to your Postman files
        var collectionPath = "path/to/your/collection.json";
        var environmentPath = "path/to/your/environment.json";
        
        // Create the runner
        var runner = new PostmanCollectionRunner(_httpClient, environmentPath);
        
        // Run the collection
        await runner.RunCollectionAsync(collectionPath);
    }
}
```

### With Logging

```csharp
// Create a test class with logging
public class MyPostmanTests
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    
    public MyPostmanTests(ITestOutputHelper output)
    {
        _output = output;
        _httpClient = new HttpClient();
        _logger = new TestOutputHelperLogger(output);
    }
    
    [Fact]
    public async Task Run_My_Collection()
    {
        var collectionPath = "path/to/your/collection.json";
        var environmentPath = "path/to/your/environment.json";
        
        var runner = new PostmanCollectionRunner(_httpClient, environmentPath, _logger);
        await runner.RunCollectionAsync(collectionPath);
    }
}
```

### With Dependency Injection

```csharp
// In your Startup.cs or Program.cs
services.AddPostmanCollectionRunner();

// In your test class
public class MyPostmanTests
{
    private readonly PostmanCollectionRunner _runner;
    
    public MyPostmanTests(PostmanCollectionRunner runner)
    {
        _runner = runner;
    }
    
    [Fact]
    public async Task Run_My_Collection()
    {
        await _runner.RunCollectionAsync("path/to/collection.json");
    }
}
```

## Supported Features

### Variable Resolution

- Environment variables from Postman environment files
- Dynamic variables:
  - `{{$uuid}}` → `Guid.NewGuid()`
  - `{{$timestamp}}` → `DateTimeOffset.UtcNow.ToUnixTimeSeconds()`
  - `{{$randomFirstName}}` → `new Faker().Name.FirstName()`
  - `{{$randomLastName}}` → `new Faker().Name.LastName()`
  - `{{$randomEmail}}` → `new Faker().Internet.Email()`
  - `{{$randomInt}}` → `new Faker().Random.Int(1, 1000)`
  - And many more...

### Postman Test Assertions

- Status code assertions: `pm.response.to.have.status(200)`
- JSON property existence: `pm.expect(response.data).to.exist`
- JSON property type: `pm.expect(response.data.accessToken).to.be.a('string')`
- Environment variable setting: `pm.environment.set("accessToken", response.data.accessToken)`

## Extending

You can extend this framework by:

1. Adding more variable resolvers in `PostmanEnvironmentResolver`
2. Adding support for more test assertions in `PostmanTestCaseBuilder`
3. Enhancing error reporting and logging

## Notes

- This framework handles basic Postman scripts for testing and variable setting
- For complex Postman scripts with JavaScript logic, you may need to add custom handlers
