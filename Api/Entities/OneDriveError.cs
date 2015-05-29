using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Entities
{
    public class OneDriveError : OneDriveErrorDetail
    {
        [JsonProperty("error")]
        public OneDriveErrorDetail Error { get; set; }
    }
}
