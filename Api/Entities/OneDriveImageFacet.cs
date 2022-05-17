using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveImageFacet
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}
