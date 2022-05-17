using KoenZomers.OneDrive.Api.Enums;
using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveViewChangesResult : OneDriveItemCollection
    {
        [JsonPropertyName("@changes.hasMoreChanges")]
        public bool HasMoreChanges { get; set; }

        [JsonPropertyName("@changes.token")]
        public string NextToken { get; set; }

        [JsonPropertyName("@changes.resync")]
        public OneDriveResyncLogicTypes ResyncBehavior { get; set; }
    }
}
