using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AchievementUploader;

public record struct NullAsFalse<T>(T? Item)
{
    public static implicit operator NullAsFalse<T>(T? value) => new(value);
    public static implicit operator T?(NullAsFalse<T> ths) => ths.Item;

    public static JsonConverter GetConverter() => new Converter();
    class Converter : JsonConverter<NullAsFalse<T>>
    {
        public override NullAsFalse<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return new NullAsFalse<T>(default);
            if (reader.TokenType == JsonTokenType.False) return new NullAsFalse<T>(default);
            var item = JsonSerializer.Deserialize<T>(ref reader, options);
            return new NullAsFalse<T>(item);
        }

        public override void Write(Utf8JsonWriter writer, NullAsFalse<T> value, JsonSerializerOptions options)
        {
            if (value.Item == null)
            {
                writer.WriteBooleanValue(false);
                return;
            }
            JsonSerializer.Serialize(writer, value.Item, options);
        }
    }
}

public static class NullAsFalseModifier
{
    public static void AddNullFalseSupport(this JsonSerializerOptions opt)
    {
        opt.TypeInfoResolver ??= new DefaultJsonTypeInfoResolver();
        if(opt.TypeInfoResolver is not DefaultJsonTypeInfoResolver r) throw new InvalidOperationException("TypeInfoResolver must be DefaultJsonTypeInfoResolver");
        r.Modifiers.Add(Modifier);
    }
    static void Modifier(JsonTypeInfo typeInfo)
    {
        foreach (var prop in typeInfo.Properties)
        {
            if(prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(NullAsFalse<>))
            {
                var converter = (JsonConverter)prop.PropertyType.GetMethod(nameof(NullAsFalse<>.GetConverter))!.Invoke(null, null)!;
                prop.CustomConverter = converter;
            }
        }
    }
}
