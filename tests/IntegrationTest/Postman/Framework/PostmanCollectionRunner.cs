using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using IntegrationTest.Postman.Framework.Models;
using Microsoft.Extensions.Logging;

namespace IntegrationTest.Postman.Framework
{
    public class PostmanCollectionRunner
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger? _logger;
        private readonly PostmanEnvironmentResolver _environmentResolver;
        private readonly PostmanTestCaseBuilder _testCaseBuilder;

        public PostmanCollectionRunner(HttpClient httpClient, PostmanEnvironment environment, ILogger? logger = null)
        {
            _httpClient = httpClient;
            _logger = logger;
            _environmentResolver = new PostmanEnvironmentResolver(environment);
            _testCaseBuilder = new PostmanTestCaseBuilder(_environmentResolver);
        }

        public PostmanCollectionRunner(HttpClient httpClient, string environmentFilePath, ILogger? logger = null)
            : this(httpClient, LoadEnvironmentFromFile(environmentFilePath), logger)
        {
        }

        public async Task RunCollectionAsync(PostmanCollection collection)
        {
            if (collection.Items == null)
            {
                _logger?.LogWarning("No items found in the collection");
                return;
            }

            await RunItemsAsync(collection.Items);
        }

        public async Task RunCollectionAsync(string collectionFilePath)
        {
            var collection = LoadCollectionFromFile(collectionFilePath);
            await RunCollectionAsync(collection);
        }

        private async Task RunItemsAsync(List<PostmanItem> items)
        {
            foreach (var item in items)
            {
                if (item.IsFolder && item.Items != null)
                {
                    _logger?.LogInformation("Running folder: {FolderName}", item.Name);
                    await RunItemsAsync(item.Items);
                }
                else if (item.Request != null)
                {
                    _logger?.LogInformation("Running request: {RequestName}", item.Name);
                    await RunRequestAsync(item);
                }
            }
        }

        private async Task RunRequestAsync(PostmanItem item)
        {
            if (item.Request == null)
            {
                _logger?.LogWarning("Request is null for item {ItemName}", item.Name);
                return;
            }

            var request = item.Request;
            var url = _environmentResolver.ResolveVariables(request.Url?.Raw ?? string.Empty);
            
            if (string.IsNullOrEmpty(url))
            {
                _logger?.LogError("URL is empty for request {RequestName}", item.Name);
                return;
            }

            var httpMethod = new HttpMethod(request.Method ?? "GET");
            var httpRequestMessage = new HttpRequestMessage(httpMethod, url);

            // Add headers
            if (request.Headers != null)
            {
                foreach (var header in request.Headers.Where(h => h.Disabled != true))
                {
                    if (header.Key != null && header.Value != null)
                    {
                        var resolvedValue = _environmentResolver.ResolveVariables(header.Value);
                        httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, resolvedValue);
                    }
                }
            }

            // Add body if it exists
            if (request.Body != null && !string.IsNullOrEmpty(request.Body.Mode))
            {
                switch (request.Body.Mode?.ToLower())
                {
                    case "raw":
                        if (!string.IsNullOrEmpty(request.Body.Raw))
                        {
                            var resolvedBody = _environmentResolver.ResolveVariables(request.Body.Raw);
                            httpRequestMessage.Content = new StringContent(resolvedBody, Encoding.UTF8, "application/json");
                        }
                        break;
                    case "formdata":
                        if (request.Body.FormData != null)
                        {
                            var formContent = new MultipartFormDataContent();
                            foreach (var formItem in request.Body.FormData)
                            {
                                if (formItem.Key != null && formItem.Value != null)
                                {
                                    var resolvedValue = _environmentResolver.ResolveVariables(formItem.Value);
                                    formContent.Add(new StringContent(resolvedValue), formItem.Key);
                                }
                            }
                            httpRequestMessage.Content = formContent;
                        }
                        break;
                    case "urlencoded":
                        if (request.Body.UrlEncoded != null)
                        {
                            var keyValuePairs = new List<KeyValuePair<string, string>>();
                            foreach (var formItem in request.Body.UrlEncoded)
                            {
                                if (formItem.Key != null && formItem.Value != null)
                                {
                                    var resolvedValue = _environmentResolver.ResolveVariables(formItem.Value);
                                    keyValuePairs.Add(new KeyValuePair<string, string>(formItem.Key, resolvedValue));
                                }
                            }
                            httpRequestMessage.Content = new FormUrlEncodedContent(keyValuePairs);
                        }
                        break;
                }
            }

            // Execute the request
            try
            {
                _logger?.LogInformation("Sending {Method} request to {Url}", httpMethod, url);
                var response = await _httpClient.SendAsync(httpRequestMessage);
                
                _logger?.LogInformation("Response status code: {StatusCode}", response.StatusCode);

                // Execute tests
                var testScripts = item.Events?
                    .Where(e => e.Listen == "test" && e.Script?.Exec != null)
                    .SelectMany(e => e.Script!.Exec!)
                    .ToList();

                if (testScripts != null && testScripts.Any())
                {
                    await _testCaseBuilder.ExecuteTestsAsync(testScripts, response);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing request {RequestName}: {Message}", item.Name, ex.Message);
                throw;
            }
        }

        private static PostmanCollection LoadCollectionFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<PostmanCollection>(json) 
                ?? throw new InvalidOperationException("Failed to deserialize Postman collection");
        }

        private static PostmanEnvironment LoadEnvironmentFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<PostmanEnvironment>(json) 
                ?? throw new InvalidOperationException("Failed to deserialize Postman environment");
        }
    }
}
