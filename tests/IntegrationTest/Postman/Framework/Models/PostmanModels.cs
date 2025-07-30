using System.Text.Json.Serialization;

namespace IntegrationTest.Postman.Framework.Models
{
    public class PostmanCollection
    {
        [JsonPropertyName("info")]
        public PostmanInfo? Info { get; set; }
        
        [JsonPropertyName("item")]
        public List<PostmanItem>? Items { get; set; }
    }

    public class PostmanInfo
    {
        [JsonPropertyName("_postman_id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("schema")]
        public string? Schema { get; set; }
    }

    public class PostmanItem
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("item")]
        public List<PostmanItem>? Items { get; set; }
        
        [JsonPropertyName("request")]
        public PostmanRequest? Request { get; set; }
        
        [JsonPropertyName("event")]
        public List<PostmanEvent>? Events { get; set; }

        [JsonIgnore]
        public bool IsFolder => Items != null;
    }

    public class PostmanRequest
    {
        [JsonPropertyName("method")]
        public string? Method { get; set; }
        
        [JsonPropertyName("header")]
        public List<PostmanHeader>? Headers { get; set; }
        
        [JsonPropertyName("body")]
        public PostmanBody? Body { get; set; }
        
        [JsonPropertyName("url")]
        public PostmanUrl? Url { get; set; }
    }

    public class PostmanHeader
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
        
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        [JsonPropertyName("disabled")]
        public bool? Disabled { get; set; }
    }

    public class PostmanBody
    {
        [JsonPropertyName("mode")]
        public string? Mode { get; set; }
        
        [JsonPropertyName("raw")]
        public string? Raw { get; set; }
        
        [JsonPropertyName("formdata")]
        public List<PostmanFormData>? FormData { get; set; }
        
        [JsonPropertyName("urlencoded")]
        public List<PostmanUrlEncoded>? UrlEncoded { get; set; }
    }

    public class PostmanFormData
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
        
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class PostmanUrlEncoded
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
        
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class PostmanUrl
    {
        [JsonPropertyName("raw")]
        public string? Raw { get; set; }
        
        [JsonPropertyName("host")]
        public List<string>? Host { get; set; }
        
        [JsonPropertyName("path")]
        public List<string>? Path { get; set; }
        
        [JsonPropertyName("query")]
        public List<PostmanQuery>? Query { get; set; }
    }

    public class PostmanQuery
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
        
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class PostmanEvent
    {
        [JsonPropertyName("listen")]
        public string? Listen { get; set; }
        
        [JsonPropertyName("script")]
        public PostmanScript? Script { get; set; }
    }

    public class PostmanScript
    {
        [JsonPropertyName("exec")]
        public List<string>? Exec { get; set; }
        
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class PostmanEnvironment
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("values")]
        public List<PostmanEnvironmentVariable>? Values { get; set; }
    }

    public class PostmanEnvironmentVariable
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
        
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }
}
