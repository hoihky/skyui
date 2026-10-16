using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkyUI.Controls.Professional;

/// <summary>Persists <see cref="SkyDockLayoutState"/> as JSON for layout restore.</summary>
public static class SkyDockLayoutSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static string ToJson(SkyDockLayoutState state) =>
        JsonSerializer.Serialize(state, Options);

    public static SkyDockLayoutState? TryFromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<SkyDockLayoutState>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
