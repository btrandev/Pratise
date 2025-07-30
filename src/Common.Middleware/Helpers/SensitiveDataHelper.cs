using Common.Middleware.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Middleware.Helpers
{
    /// <summary>
    /// Helper for managing sensitive data in payloads
    /// </summary>
    public static class SensitiveDataHelper
    {
        private const string RedactedValue = "***REDACTED***";
        
        /// <summary>
        /// Creates a sanitized version of the payload where sensitive properties are redacted
        /// </summary>
        /// <param name="payload">The payload object to sanitize</param>
        /// <returns>A sanitized representation of the payload as a string</returns>
        public static string SanitizePayload(object payload)
        {
            if (payload == null)
                return "null";

            try
            {
                // Handle primitive types directly
                if (IsPrimitiveType(payload.GetType()))
                {
                    return JsonSerializer.Serialize(payload);
                }
                
                // Check if this is a command/query with a Payload property
                var payloadProperty = payload.GetType().GetProperty("Payload");
                if (payloadProperty != null)
                {
                    // Extract the actual payload object
                    var actualPayload = payloadProperty.GetValue(payload);
                    if (actualPayload != null)
                    {
                        // If the payload is a primitive type, return it directly
                        if (IsPrimitiveType(actualPayload.GetType()))
                        {
                            return JsonSerializer.Serialize(actualPayload);
                        }
                        
                        // Sanitize the actual payload object
                        return SanitizeObject(actualPayload);
                    }
                }
                
                // If no Payload property or it's null, sanitize the object itself
                return SanitizeObject(payload);
            }
            catch (Exception ex)
            {
                return $"{{\"error\":\"Failed to sanitize payload: {ex.Message}\"}}";
            }
        }
        
        private static string SanitizeObject(object obj)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new SensitiveDataJsonConverter() }
            };
            
            return JsonSerializer.Serialize(obj, options);
        }
        
        /// <summary>
        /// Checks if an object has any properties marked with [Sensitive] attribute
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>True if the object has sensitive properties, false otherwise</returns>
        public static bool HasSensitiveProperties(object obj)
        {
            if (obj == null)
                return false;
                
            try
            {
                var type = obj.GetType();
                
                // Primitive types cannot have sensitive properties
                if (IsPrimitiveType(type))
                {
                    return false;
                }
                
                // Check if the object itself has sensitive properties
                if (type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Any(p => p.GetCustomAttribute<SensitiveAttribute>() != null))
                {
                    return true;
                }
                
                // Check if this is a command/query with a Payload property
                var payloadProperty = type.GetProperty("Payload");
                if (payloadProperty != null)
                {
                    var payload = payloadProperty.GetValue(obj);
                    if (payload != null)
                    {
                        // If payload is a primitive type, it can't have sensitive properties
                        if (IsPrimitiveType(payload.GetType()))
                        {
                            return false;
                        }
                        
                        // Check if the payload has sensitive properties
                        return payload.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Any(p => p.GetCustomAttribute<SensitiveAttribute>() != null);
                    }
                }
                
                return false;
            }
            catch
            {
                // If any error occurs during reflection, assume it might have sensitive properties
                return true;
            }
        }
        
        /// <summary>
        /// Checks if a type is a primitive or simple value type that can't have sensitive properties
        /// </summary>
        private static bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive || 
                   type.IsEnum || 
                   type == typeof(string) || 
                   type == typeof(decimal) || 
                   type == typeof(DateTime) || 
                   type == typeof(DateTimeOffset) || 
                   type == typeof(TimeSpan) || 
                   type == typeof(Guid) || 
                   Nullable.GetUnderlyingType(type) != null;
        }

        
        /// <summary>
        /// Custom JSON converter that redacts properties marked as sensitive
        /// </summary>
        private class SensitiveDataJsonConverter : JsonConverter<object>
        {
            public override bool CanConvert(Type typeToConvert) => true;
            
            public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // Use the default serialization for reading
                // This is needed for the object to be properly deserialized during our redaction process
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }
                
                // For primitive types, deserialize directly
                if (IsPrimitiveType(typeToConvert))
                {
                    return JsonSerializer.Deserialize(ref reader, typeToConvert);
                }
                
                // For complex types, we need to skip this converter to avoid infinite recursion
                var newOptions = CreateOptionsWithoutThisConverter(options);
                
                return JsonSerializer.Deserialize(ref reader, typeToConvert, newOptions);
            }
            
            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                if (value == null)
                {
                    writer.WriteNullValue();
                    return;
                }
                
                var type = value.GetType();
                
                // Handle primitive types and strings
                if (IsPrimitiveType(type))
                {
                    // Create options without this converter to avoid infinite recursion
                    var newOptions = CreateOptionsWithoutThisConverter(options);
                    
                    JsonSerializer.Serialize(writer, value, newOptions);
                    return;
                }
                
                // Handle dictionaries
                if (IsDictionary(type))
                {
                    WriteDictionary(writer, value, options);
                    return;
                }
                
                // Handle collections
                if (IsCollection(type) && !IsString(type))
                {
                    WriteCollection(writer, value, options);
                    return;
                }
                
                // Handle complex objects
                writer.WriteStartObject();
                
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in properties)
                {
                    var propName = GetJsonPropertyName(prop);
                    writer.WritePropertyName(propName);
                    
                    // Check if property is sensitive
                    if (IsSensitiveProperty(prop))
                    {
                        writer.WriteStringValue(RedactedValue);
                    }
                    else
                    {
                        var propValue = prop.GetValue(value);
                        
                            // Create new options without this converter to avoid infinite recursion
                        var newOptions = CreateOptionsWithoutThisConverter(options);
                        
                        JsonSerializer.Serialize(writer, propValue, newOptions);
                    }
                }
                
                writer.WriteEndObject();
            }
            
            private bool IsPrimitiveOrString(Type type)
            {
                // Use the shared helper method
                return IsPrimitiveType(type);
            }
            
            private bool IsString(Type type) => type == typeof(string);
            
            private bool IsDictionary(Type type)
            {
                return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            }
            
            private bool IsCollection(Type type)
            {
                return typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
            }
            
            private JsonSerializerOptions CreateOptionsWithoutThisConverter(JsonSerializerOptions options)
            {
                var newOptions = new JsonSerializerOptions(options);
                newOptions.Converters.Clear();
                foreach (var converter in options.Converters)
                {
                    if (!(converter is SensitiveDataJsonConverter))
                    {
                        newOptions.Converters.Add(converter);
                    }
                }
                return newOptions;
            }
            
            private void WriteDictionary(Utf8JsonWriter writer, object dictionary, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                
                var enumerableDict = (System.Collections.IEnumerable)dictionary;
                foreach (var item in enumerableDict)
                {
                    var keyProperty = item.GetType().GetProperty("Key");
                    var valueProperty = item.GetType().GetProperty("Value");
                    
                    if (keyProperty != null && valueProperty != null)
                    {
                        var key = keyProperty.GetValue(item)?.ToString() ?? "null";
                        var value = valueProperty.GetValue(item);
                        
                        writer.WritePropertyName(key);
                        
                        // Create options without this converter to avoid infinite recursion
                        var newOptions = CreateOptionsWithoutThisConverter(options);
                        
                        JsonSerializer.Serialize(writer, value, newOptions);
                    }
                }
                
                writer.WriteEndObject();
            }
            
            private void WriteCollection(Utf8JsonWriter writer, object collection, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                
                var enumerable = (System.Collections.IEnumerable)collection;
                
                // Create options without this converter to avoid infinite recursion
                var newOptions = CreateOptionsWithoutThisConverter(options);
                
                foreach (var item in enumerable)
                {
                    JsonSerializer.Serialize(writer, item, newOptions);
                }
                
                writer.WriteEndArray();
            }
            
            private bool IsSensitiveProperty(PropertyInfo property)
            {
                return property.GetCustomAttribute<SensitiveAttribute>() != null;
            }
            
            private string GetJsonPropertyName(PropertyInfo property)
            {
                var jsonPropertyNameAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jsonPropertyNameAttribute != null)
                {
                    return jsonPropertyNameAttribute.Name;
                }
                
                // Default to camelCase
                var name = property.Name;
                return char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
        }
    }
}
