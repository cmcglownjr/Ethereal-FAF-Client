﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace beta.Infrastructure.Converters.JSON
{
    public class RawStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            return jsonDoc.RootElement.GetRawText();
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
