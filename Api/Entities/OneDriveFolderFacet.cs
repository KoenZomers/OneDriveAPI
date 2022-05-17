using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveFolderFacet
    {
        [JsonPropertyName("childCount")]
        public long ChildCount { get; set; }
    }
}
