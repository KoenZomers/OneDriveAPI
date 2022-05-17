using KoenZomers.OneDrive.Api.Enums;
using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// The Quota facet groups storage space quota-related information on OneDrive into a single structure
    /// </summary>
    public class OneDriveQuotaFacet
    {
        /// <summary>
        /// Total allowed storage space, in bytes
        /// </summary>
        [JsonPropertyName("total")]
        public long Total { get; set; }

        /// <summary>
        /// Total space used, in bytes
        /// </summary>
        [JsonPropertyName("used")]
        public long Used { get; set; }

        /// <summary>
        /// Total space remaining before reaching the quota limit, in bytes
        /// </summary>
        [JsonPropertyName("remaining")]
        public long Remaining { get; set; }

        /// <summary>
        /// Total space consumed by files in the recycle bin, in bytes
        /// </summary>
        [JsonPropertyName("deleted")]
        public long Deleted { get; set; }

        /// <summary>
        /// Enumeration value that indicates the state of the storage space
        /// </summary>
        [JsonPropertyName("state")]
        public OneDriveQuotaState State { get; set; }
    }
}
