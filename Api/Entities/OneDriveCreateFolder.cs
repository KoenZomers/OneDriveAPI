using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveCreateFolder : OneDriveItemBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("folder")]
        public object Folder { get; set; }

        [JsonPropertyName("@microsoft.graph.conflictBehavior")]
        public string ConflictBehavior { get; set; } = "rename";
    }
}
