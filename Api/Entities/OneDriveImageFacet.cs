using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Entities
{
    public class OneDriveImageFacet
    {
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
