using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Message to request sharing of an item
    /// </summary>
    internal class OneDriveRequestShare : OneDriveItemBase
    {
        /// <summary>
        /// Type of sharing to request
        /// </summary>
        [JsonProperty("type")]
        public Enums.OneDriveLinkType SharingType { get; set; }
    }
}
