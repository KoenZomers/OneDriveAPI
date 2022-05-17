using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    internal class OneDriveUploadSessionItemContainer : OneDriveItemBase
    {
        [JsonPropertyName("item")]
        public OneDriveUploadSessionItem Item { get; set; }
    }
}
