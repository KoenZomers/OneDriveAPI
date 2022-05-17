using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    internal class GraphApiUploadSessionItemContainer : OneDriveItemBase
    {
        [JsonPropertyName("item")]
        public GraphApiUploadSessionItem Item { get; set; }
    }
}
