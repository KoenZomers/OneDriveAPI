using KoenZomers.OneDrive.Api.Enums;
using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// The Drive resource represents a drive in OneDrive. It provides information about the owner of the drive, total and available storage space, and exposes a collection of all the Items contained within the drive.
    /// </summary>
    public class OneDriveDrive : OneDriveItemBase
    {
        /// <summary>
        /// The unique identifier of the drive
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Enumerated value that identifies the type of drive account. OneDrive drives will show as personal. 
        /// </summary>
        [JsonPropertyName("driveType")]
        public OneDriveDriveType DriveType { get; set; }

        /// <summary>
        /// The user account that owns the drive
        /// </summary>
        [JsonPropertyName("owner")]
        public OneDriveIdentitySet Owner { get; set; }

        /// <summary>
        /// Information about the drive's storage space quota
        /// </summary>
        [JsonPropertyName("quota")]
        public OneDriveQuotaFacet Quota { get; set; }
    }
}
