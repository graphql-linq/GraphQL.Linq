// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Text.Json;
using System.Text.Json.Serialization;

public class TrimmedDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDecimal();

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        => writer.WriteRawValue(value.ToString("G29", System.Globalization.CultureInfo.InvariantCulture));
}

// Optional for nullable decimals:
public class TrimmedNullableDecimalConverter : JsonConverter<decimal?>
{
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : reader.GetDecimal();

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteRawValue(value.Value.ToString("G29", System.Globalization.CultureInfo.InvariantCulture));
        else
            writer.WriteNullValue();
    }
}
