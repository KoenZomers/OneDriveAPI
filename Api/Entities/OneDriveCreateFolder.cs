using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    internal class OneDriveCreateFolder : OneDriveItemBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("folder")]
        public object Folder { get; set; }
    }
}
