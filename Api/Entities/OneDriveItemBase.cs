using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Entities
{
    public abstract class OneDriveItemBase
    {
        [JsonIgnore]
        public string OriginalJson { get; set; }
    }
}
