using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using IntegrationTest.Postman.Framework.Models;

namespace IntegrationTest.Postman.Framework
{
    public class PostmanTestCaseBuilder
    {
        private readonly PostmanEnvironmentResolver _environmentResolver;
        private static readonly Regex StatusCodeRegex = new(@"pm\.response\.to\.have\.status\((\d+)\)", RegexOptions.Compiled);
        private static readonly Regex JsonPropertyExistsRegex = new(@"pm\.expect\(response\.([^)]+)\)\.to\.exist", RegexOptions.Compiled);
        private static readonly Regex JsonPropertyTypeRegex = new(@"pm\.expect\(response\.([^)]+)\)\.to\.be\.a\('([^']+)'\)", RegexOptions.Compiled);
        private static readonly Regex EnvironmentSetRegex = new(@"pm\.environment\.set\(""([^""]+)"",\s*response\.([^)]+)\)", RegexOptions.Compiled);

        public PostmanTestCaseBuilder(PostmanEnvironmentResolver environmentResolver)
        {
            _environmentResolver = environmentResolver;
        }

        public async Task ExecuteTestsAsync(List<string> testScripts, HttpResponseMessage response)
        {
            if (testScripts == null || !testScripts.Any())
            {
                return;
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = string.IsNullOrEmpty(responseBody) ? null : JsonDocument.Parse(responseBody);

            // Join all test scripts into a single string for easier regex parsing
            var scriptContent = string.Join("\n", testScripts);

            // Parse status code assertions
            var statusMatches = StatusCodeRegex.Matches(scriptContent);
            foreach (Match match in statusMatches)
            {
                if (match.Success && int.TryParse(match.Groups[1].Value, out int expectedStatus))
                {
                    response.StatusCode.Should().Be((HttpStatusCode)expectedStatus);
                }
            }

            // Parse JSON property existence assertions
            var jsonExistsMatches = JsonPropertyExistsRegex.Matches(scriptContent);
            foreach (Match match in jsonExistsMatches)
            {
                if (match.Success && responseJson != null)
                {
                    string propertyPath = match.Groups[1].Value;
                    var jsonElement = _environmentResolver.ExtractJsonValue(responseBody, propertyPath);
                    jsonElement.Should().NotBeNull($"Property '{propertyPath}' should exist");
                }
            }

            // Parse JSON property type assertions
            var jsonTypeMatches = JsonPropertyTypeRegex.Matches(scriptContent);
            foreach (Match match in jsonTypeMatches)
            {
                if (match.Success && responseJson != null)
                {
                    string propertyPath = match.Groups[1].Value;
                    string expectedType = match.Groups[2].Value;

                    var jsonElement = _environmentResolver.ExtractJsonValue(responseBody, propertyPath);
                    jsonElement.Should().NotBeNull($"Property '{propertyPath}' should exist");

                    if (jsonElement.HasValue)
                    {
                        switch (expectedType.ToLower())
                        {
                            case "string":
                                jsonElement.Value.ValueKind.Should().Be(JsonValueKind.String);
                                break;
                            case "number":
                                jsonElement.Value.ValueKind.Should().BeOneOf(JsonValueKind.Number);
                                break;
                            case "object":
                                jsonElement.Value.ValueKind.Should().Be(JsonValueKind.Object);
                                break;
                            case "array":
                                jsonElement.Value.ValueKind.Should().Be(JsonValueKind.Array);
                                break;
                            case "boolean":
                                jsonElement.Value.ValueKind.Should().Be(JsonValueKind.True, becauseArgs: JsonValueKind.False);
                                break;
                        }
                    }
                }
            }

            // Handle environment variable setting
            var environmentSetMatches = EnvironmentSetRegex.Matches(scriptContent);
            foreach (Match match in environmentSetMatches)
            {
                if (match.Success && responseJson != null)
                {
                    string variableName = match.Groups[1].Value;
                    string propertyPath = match.Groups[2].Value;

                    var jsonElement = _environmentResolver.ExtractJsonValue(responseBody, propertyPath);
                    if (jsonElement.HasValue && jsonElement.Value.ValueKind == JsonValueKind.String)
                    {
                        _environmentResolver.SetVariable(variableName, jsonElement.Value.GetString() ?? string.Empty);
                    }
                    else if (jsonElement.HasValue)
                    {
                        _environmentResolver.SetVariable(variableName, jsonElement.Value.ToString() ?? string.Empty);
                    }
                }
            }
        }
    }
}
