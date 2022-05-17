using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveError : OneDriveItemBase
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }
    }
}
