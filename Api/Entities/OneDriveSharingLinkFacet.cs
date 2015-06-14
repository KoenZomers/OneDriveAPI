using KoenZomers.OneDrive.Api.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveSharingLinkFacet
    {
        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("application")]
        public OneDriveIdentity Application { get; set; }

        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public OneDriveLinkType Type { get; set; }
    }
}
