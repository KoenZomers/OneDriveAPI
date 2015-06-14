using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Api.Entities
{
    public abstract class OneDriveItemBase
    {
        [JsonIgnore]
        public string OriginalJson { get; set; }
    }
}
