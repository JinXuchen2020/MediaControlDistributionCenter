namespace MediaControlDistributionCenter.Converters
{
    using MediaControlDistributionCenter.Models;
    using MediaControlDistributionCenter.ViewModels;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class MediaTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BaseComponent);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            object? result = default;
            var target = serializer.Deserialize<JObject>(reader);
            var ruleType = target?.Property("Type")?.Value.ToObject<MediaType>();
            if (ruleType == null)
            {
                return result;
            }
            switch (ruleType)
            {
                case MediaType.Video:
                    result = target?.ToObject<VideoComponent>();
                    break;
                case MediaType.Image:
                    result = target?.ToObject<ImageComponent>();
                    break;
                case MediaType.Text:
                    result = target?.ToObject<TextComponent>();
                    break;
                case MediaType.Hdmi:
                    result = target?.ToObject<HdmiComponent>();
                    break;
                case MediaType.Stream:
                    result = target?.ToObject<StreamComponent>();
                    break;
                case MediaType.Rss:
                    result = target?.ToObject<RssComponent>();
                    break;
                case MediaType.Web:
                    result = target?.ToObject<WebComponent>();
                    break;
                case MediaType.Word:
                    result = target?.ToObject<WordComponent>();
                    break;
                case MediaType.ColorText:
                    result = target?.ToObject<ColorTextComponent>();
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
