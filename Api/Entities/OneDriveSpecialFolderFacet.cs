using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveSpecialFolderFacet
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
