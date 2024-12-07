using NPServer.Database.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NPServer.Database.Json;

public class DBEntityCollectionJsonConverter : JsonConverter<DBEntityCollection>
{
    public override DBEntityCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        DBEntity[]? entities = JsonSerializer.Deserialize<DBEntity[]>(ref reader, options);
        entities ??= [];

        return new DBEntityCollection(entities);
    }

    public override void Write(Utf8JsonWriter writer, DBEntityCollection value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Entries.ToArray(), options);
    }
}