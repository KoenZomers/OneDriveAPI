using KoenZomers.OneDrive.Sync.BusinessLogic.Enums;
using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Entities
{
    public class OneDriveViewChangesResult : OneDriveItemCollection
    {
        [JsonProperty("@changes.hasMoreChanges")]
        public bool HasMoreChanges { get; set; }

        [JsonProperty("@changes.token")]
        public string NextToken { get; set; }

        [JsonProperty("@changes.resync", DefaultValueHandling=DefaultValueHandling.IgnoreAndPopulate)]
        public OneDriveResyncLogicTypes ResyncBehavior { get; set; }
    }
}
