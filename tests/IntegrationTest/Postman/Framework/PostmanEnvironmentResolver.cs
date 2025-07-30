using System.Text.Json;
using System.Text.RegularExpressions;
using Bogus;
using IntegrationTest.Postman.Framework.Models;

namespace IntegrationTest.Postman.Framework
{
    public class PostmanEnvironmentResolver
    {
        private readonly Dictionary<string, string> _variables = new(StringComparer.OrdinalIgnoreCase);
        private readonly Faker _faker = new();
        private readonly Regex _variablePattern = new(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);

        public PostmanEnvironmentResolver()
        {
        }

        public PostmanEnvironmentResolver(PostmanEnvironment environment)
        {
            if (environment.Values != null)
            {
                foreach (var variable in environment.Values.Where(v => v.Enabled))
                {
                    if (variable.Key != null && variable.Value != null)
                    {
                        _variables[variable.Key] = variable.Value;
                    }
                }
            }
        }

        public string ResolveVariables(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return _variablePattern.Replace(input, match =>
            {
                var variableName = match.Groups[1].Value;
                return ResolveVariable(variableName);
            });
        }

        public Dictionary<string, string> ResolveHeaders(Dictionary<string, string> headers)
        {
            if (headers == null)
            {
                return new Dictionary<string, string>();
            }

            var resolvedHeaders = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                resolvedHeaders[ResolveVariables(header.Key)] = ResolveVariables(header.Value);
            }

            return resolvedHeaders;
        }

        public void SetVariable(string name, string value)
        {
            _variables[name] = value;
        }

        public string GetVariable(string name)
        {
            return _variables.TryGetValue(name, out var value) ? value : string.Empty;
        }

        private string ResolveVariable(string variableName)
        {
            // Check for predefined dynamic variables
            return variableName switch
            {
                "$uuid" => Guid.NewGuid().ToString(),
                "$timestamp" => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                "$randomFirstName" => _faker.Name.FirstName(),
                "$randomLastName" => _faker.Name.LastName(),
                "$randomEmail" => _faker.Internet.Email(),
                "$randomInt" => _faker.Random.Int(1, 1000).ToString(),
                "$randomPhoneNumber" => _faker.Phone.PhoneNumber(),
                "$randomUserName" => _faker.Internet.UserName(),
                "$randomUrl" => _faker.Internet.Url(),
                "$randomBankAccount" => _faker.Finance.Account(),
                "$randomBic" => _faker.Finance.Bic(),
                "$randomIban" => _faker.Finance.Iban(),
                "$randomCreditCardNumber" => _faker.Finance.CreditCardNumber(),
                _ => _variables.TryGetValue(variableName, out var value) ? value : $"{{{{{variableName}}}}}"
            };
        }

        public JsonElement? ExtractJsonValue(string json, string path)
        {
            try
            {
                if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(path))
                {
                    return null;
                }

                var jsonDocument = JsonDocument.Parse(json);
                var pathParts = path.Split('.');
                JsonElement current = jsonDocument.RootElement;

                foreach (var part in pathParts)
                {
                    if (current.ValueKind != JsonValueKind.Object)
                    {
                        return null;
                    }

                    if (!current.TryGetProperty(part, out current))
                    {
                        return null;
                    }
                }

                return current;
            }
            catch
            {
                return null;
            }
        }
    }
}
