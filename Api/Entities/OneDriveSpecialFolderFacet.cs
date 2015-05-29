using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Entities
{
    public class OneDriveSpecialFolderFacet
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
