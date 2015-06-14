using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveError : OneDriveErrorDetail
    {
        [JsonProperty("error")]
        public OneDriveErrorDetail Error { get; set; }
    }
}
