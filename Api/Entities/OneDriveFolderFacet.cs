using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Entities
{
    public class OneDriveFolderFacet
    {
        [JsonProperty("childCount")]
        public long ChildCount { get; set; }
    }
}
