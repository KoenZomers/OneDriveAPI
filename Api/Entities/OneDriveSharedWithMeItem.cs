using System;
using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Entity for a shared with me item
    /// </summary>
    public class OneDriveSharedWithMeItem
    {
        /// <summary>
        /// Date and time at which the item was created
        /// </summary>
        [JsonPropertyName("createdDateTime")]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Unique OneDrive item ID
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Date and time at which this item was last modified
        /// </summary>
        [JsonPropertyName("lastModifiedDateTime")]
        public DateTime? LastModifiedDateTime { get; set; }

        /// <summary>
        /// Details of this item
        /// </summary>
        [JsonPropertyName("remoteItem")]
        public OneDriveRemoteItem RemoteItem { get; set; }

        /// <summary>
        /// Size of this item
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }
    }
}
