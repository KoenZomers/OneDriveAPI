using KoenZomers.OneDrive.Api.Enums;
using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    internal class OneDriveUploadSessionItem
    {
        [JsonPropertyName("@microsoft.graph.conflictBehavior")]
        public NameConflictBehavior FilenameConflictBehavior { get; set; }

        [JsonPropertyName("name")]
        public string Filename { get; set; }
    }
}
