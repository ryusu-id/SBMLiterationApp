using System.Text.Json;
using System.Text.Json.Serialization;

namespace PureTCOWebApp.Core.JsonConverter;

public class NumberJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(int) || typeToConvert == typeof(int?) ||
               typeToConvert == typeof(long) || typeToConvert == typeof(long?) ||
               typeToConvert == typeof(decimal) || typeToConvert == typeof(decimal?) ||
               typeToConvert == typeof(double) || typeToConvert == typeof(double?) ||
               typeToConvert == typeof(float) || typeToConvert == typeof(float?) ||
               typeToConvert == typeof(short) || typeToConvert == typeof(short?) ||
               typeToConvert == typeof(byte) || typeToConvert == typeof(byte?);
    }

    public override System.Text.Json.Serialization.JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(int)) return new NumberConverter<int>();
        if (typeToConvert == typeof(int?)) return new NullableNumberConverter<int>();
        if (typeToConvert == typeof(long)) return new NumberConverter<long>();
        if (typeToConvert == typeof(long?)) return new NullableNumberConverter<long>();
        if (typeToConvert == typeof(decimal)) return new NumberConverter<decimal>();
        if (typeToConvert == typeof(decimal?)) return new NullableNumberConverter<decimal>();
        if (typeToConvert == typeof(double)) return new NumberConverter<double>();
        if (typeToConvert == typeof(double?)) return new NullableNumberConverter<double>();
        if (typeToConvert == typeof(float)) return new NumberConverter<float>();
        if (typeToConvert == typeof(float?)) return new NullableNumberConverter<float>();
        if (typeToConvert == typeof(short)) return new NumberConverter<short>();
        if (typeToConvert == typeof(short?)) return new NullableNumberConverter<short>();
        if (typeToConvert == typeof(byte)) return new NumberConverter<byte>();
        if (typeToConvert == typeof(byte?)) return new NullableNumberConverter<byte>();

        throw new ArgumentException($"Type {typeToConvert} is not supported");
    }

    private class NumberConverter<T> : JsonConverter<T> where T : struct
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return ReadNumber(ref reader, typeToConvert);
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                
                // If empty or whitespace, return default (0)
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return default(T);
                }

                // Try to parse the string value
                try
                {
                    return ParseValue(stringValue, typeToConvert);
                }
                catch (FormatException)
                {
                    throw new JsonException($"The value '{stringValue}' contains non-digit characters and cannot be converted to {typeToConvert.Name}");
                }
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return default(T);
            }

            throw new JsonException($"Unable to convert token type {reader.TokenType} to {typeToConvert.Name}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            
            writer.WriteNumberValue(Convert.ToDecimal(value));
        }

        private T ReadNumber(ref Utf8JsonReader reader, Type typeToConvert)
        {
            if (typeToConvert == typeof(int)) return (T)(object)reader.GetInt32();
            if (typeToConvert == typeof(long)) return (T)(object)reader.GetInt64();
            if (typeToConvert == typeof(decimal)) return (T)(object)reader.GetDecimal();
            if (typeToConvert == typeof(double)) return (T)(object)reader.GetDouble();
            if (typeToConvert == typeof(float)) return (T)(object)(float)reader.GetDouble();
            if (typeToConvert == typeof(short)) return (T)(object)reader.GetInt16();
            if (typeToConvert == typeof(byte)) return (T)(object)reader.GetByte();
            
            throw new JsonException($"Unable to read number for type {typeToConvert.Name}");
        }

        private T ParseValue(string value, Type typeToConvert)
        {
            if (typeToConvert == typeof(int)) return (T)(object)int.Parse(value);
            
            if (typeToConvert == typeof(long)) return (T)(object)long.Parse(value);
            
            if (typeToConvert == typeof(decimal)) return (T)(object)decimal.Parse(value);
            if (typeToConvert == typeof(double)) return (T)(object)double.Parse(value);
            
            if (typeToConvert == typeof(float)) return (T)(object)float.Parse(value);
            
            if (typeToConvert == typeof(short)) return (T)(object)short.Parse(value);
            if (typeToConvert == typeof(byte)) return (T)(object)byte.Parse(value);
            
            throw new JsonException($"Unable to parse value for type {typeToConvert.Name}");
        }
    }

    private class NullableNumberConverter<T> : JsonConverter<T?> where T : struct
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return ReadNumber(ref reader, typeof(T));
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                
                // If empty or whitespace, return null for nullable types
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    return null;
                }

                // Try to parse the string value
                try
                {
                    return ParseValue(stringValue, typeof(T));
                }
                catch (FormatException)
                {
                    throw new JsonException($"The value '{stringValue}' contains non-digit characters and cannot be converted to {typeof(T).Name}");
                }
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            throw new JsonException($"Unable to convert token type {reader.TokenType} to {typeof(T).Name}?");
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(Convert.ToDecimal(value.Value));
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        private T ReadNumber(ref Utf8JsonReader reader, Type typeToConvert)
        {
            if (typeToConvert == typeof(int)) return (T)(object)reader.GetInt32();
            if (typeToConvert == typeof(long)) return (T)(object)reader.GetInt64();
            if (typeToConvert == typeof(decimal)) return (T)(object)reader.GetDecimal();
            if (typeToConvert == typeof(double)) return (T)(object)reader.GetDouble();
            if (typeToConvert == typeof(float)) return (T)(object)(float)reader.GetDouble();
            if (typeToConvert == typeof(short)) return (T)(object)reader.GetInt16();
            if (typeToConvert == typeof(byte)) return (T)(object)reader.GetByte();
            
            throw new JsonException($"Unable to read number for type {typeToConvert.Name}");
        }

        private T ParseValue(string value, Type typeToConvert)
        {
            if (typeToConvert == typeof(int)) return (T)(object)int.Parse(value);
            
            
            if (typeToConvert == typeof(long)) return (T)(object)long.Parse(value);
            
            if (typeToConvert == typeof(decimal)) return (T)(object)decimal.Parse(value);
            if (typeToConvert == typeof(double)) return (T)(object)double.Parse(value);
            
            if (typeToConvert == typeof(float)) return (T)(object)float.Parse(value);
            
            if (typeToConvert == typeof(short)) return (T)(object)short.Parse(value);
            if (typeToConvert == typeof(byte)) return (T)(object)byte.Parse(value);
            
            throw new JsonException($"Unable to parse value for type {typeToConvert.Name}");
        }
    }
}
