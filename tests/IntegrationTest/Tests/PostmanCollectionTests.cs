using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTest.Postman.Framework.Tests
{
    public class PostmanCollectionTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PostmanCollectionTests> _logger;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public PostmanCollectionTests(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _output = output;
            
            // Store the original factory
            _factory = factory;
            
            // Create a configured WebApplicationFactory with the desired settings
            var configuredFactory = factory.WithWebHostBuilder(builder =>
            {
                // Configure server to listen on port 5000
                builder.UseUrls("http://localhost:5000");
                builder.UseEnvironment("Test");
                
                builder.ConfigureServices(services =>
                {
                    _output.WriteLine("Configuring test services for integration tests");
                });
            });

            _logger = new TestOutputHelperLogger<PostmanCollectionTests>(output);
            
            // Configure client with settings suitable for integration testing
            _httpClient = configuredFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost:5000"),
                AllowAutoRedirect = false,
                HandleCookies = true // Important for maintaining session state in tests
            });
            
            _output.WriteLine($"Test client configured with base address: {_httpClient.BaseAddress}");
        }

        [Fact]
        public async Task Run_Practices_Collection()
        {
            // Arrange
            _output.WriteLine("Starting Postman collection test");
            var collectionPath = Path.Combine(AppContext.BaseDirectory, "Postman", "Practices.postman_collection.json");
            var environmentPath = Path.Combine(AppContext.BaseDirectory, "Postman", "PracticesLocal.postman_environment.json");
            
            _output.WriteLine($"Collection path: {collectionPath}");
            _output.WriteLine($"Environment path: {environmentPath}");
            
            // Act & Assert
            var runner = new PostmanCollectionRunner(_httpClient, environmentPath, _logger);
            await runner.RunCollectionAsync(collectionPath);
        }

        // [Theory]
        // [InlineData("Practices.postman_collection.json", "PracticesLocal.postman_environment.json")]
        // public async Task Run_Parameterized_Collection(string collectionFileName, string environmentFileName)
        // {
        //     // Arrange
        //     _output.WriteLine($"Starting parameterized Postman collection test with: {collectionFileName}");
        //     var collectionPath = Path.Combine(AppContext.BaseDirectory, "Postman", collectionFileName);
        //     var environmentPath = Path.Combine(AppContext.BaseDirectory, "Postman", environmentFileName);
            
        //     _output.WriteLine($"Collection path: {collectionPath}");
        //     _output.WriteLine($"Environment path: {environmentPath}");
            
        //     // Act & Assert
        //     var runner = new PostmanCollectionRunner(_httpClient, environmentPath, _logger);
        //     await runner.RunCollectionAsync(collectionPath);
        // }
    }

    // Helper class to convert ITestOutputHelper to ILogger
    public class TestOutputHelperLogger<T> : ILogger<T>, ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestOutputHelperLogger(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _testOutputHelper.WriteLine($"[{logLevel}] {formatter(state, exception)}");
            if (exception != null)
            {
                _testOutputHelper.WriteLine(exception.ToString());
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NoopDisposable();

        private class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
