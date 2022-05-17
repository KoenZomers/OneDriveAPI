using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveThumbnailSet : OneDriveItemBase
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("small")]
        public OneDriveThumbnail Small { get; set; }

        [JsonPropertyName("medium")]
        public OneDriveThumbnail Medium { get; set; }

        [JsonPropertyName("large")]
        public OneDriveThumbnail Large { get; set; }
    }
}
