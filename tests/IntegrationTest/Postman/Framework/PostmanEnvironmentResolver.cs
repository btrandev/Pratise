using System.Buffers.Text;
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

        public string ResolveAuth(PostmanAuth auth)
        {
            if (auth == null)
            {
                return null;
            }

            // Handle Bearer token auth
            if (auth.Type == "bearer" && auth.Bearer != null)
            {
                var resolvedBearer = new List<PostmanAuthParam>();
                foreach (var param in auth.Bearer)
                {
                    resolvedBearer.Add(new PostmanAuthParam
                    {
                        Key = param.Key,
                        Value = ResolveVariables(param.Value),
                        Type = param.Type
                    });
                }
                return $"Bearer {string.Join(" ", resolvedBearer.Select(p => p.Value))}";
            }

            // Handle Basic auth
            if (auth.Type == "basic" && auth.Basic != null)
            {
                var resolvedBasic = new List<PostmanAuthParam>();
                foreach (var param in auth.Basic)
                {
                    resolvedBasic.Add(new PostmanAuthParam
                    {
                        Key = param.Key,
                        Value = ResolveVariables(param.Value),
                        Type = param.Type
                    });
                    // Convert to Base64 using Convert.ToBase64String
                    string authString = string.Join(":", resolvedBasic.Select(p => p.Value));
                    string base64Auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authString));
                    return $"Basic {base64Auth}";
                }
            }
            // Add other auth types as needed (apikey, oauth2, etc.)
            return string.Empty;
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
