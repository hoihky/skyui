using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkyUI.Controls.Timeline.Serialization;

public sealed class JsonTimelineProjectSerializer : ITimelineProjectSerializer
{
    private readonly JsonSerializerOptions options;

    public JsonTimelineProjectSerializer()
    {
        options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        options.Converters.Add(new JsonStringEnumConverter());
    }

    public string Serialize(TimelineProjectDocument document) =>
        JsonSerializer.Serialize(document, options);

    public TimelineProjectDocument Deserialize(string json) =>
        JsonSerializer.Deserialize<TimelineProjectDocument>(json, options)
        ?? new TimelineProjectDocument();
}
