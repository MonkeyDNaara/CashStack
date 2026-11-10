using System.Text.Json;
using System.Text.Json.Serialization;

namespace CashStack.Repositories;

// Shared serializer settings, so all JSON files have the same format
internal static class JsonStorage
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}
