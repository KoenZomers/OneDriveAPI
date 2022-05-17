using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// The IdentitySet type is a keyed collection of Identity objects. It is used to represent a set of identities associated with various events for an item, such as created by or last modified by.
    /// </summary>
    public class OneDriveIdentitySet : OneDriveItemBase
    {
        /// <summary>
        /// An Identity resource that represents a user
        /// </summary>
        [JsonPropertyName("user")]
        public OneDriveIdentity User { get; set; }

        /// <summary>
        /// An Identity resource that represents the application
        /// </summary>
        [JsonPropertyName("device")]
        public OneDriveIdentity Device { get; set; }

        /// <summary>
        /// An Identity resource that represents the device
        /// </summary>
        [JsonPropertyName("application")]
        public OneDriveIdentity Application { get; set; }
    }
}
