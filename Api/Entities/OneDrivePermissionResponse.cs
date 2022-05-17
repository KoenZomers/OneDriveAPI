using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Response to a new permission request on a OneDrive item
    /// </summary>
    public class OneDrivePermissionResponse : OneDriveItemBase
    {
        /// <summary>
        /// The grant created for an individual to access the OneDrive item
        /// </summary>
        [JsonPropertyName("grantedTo")]
        public OneDrivePermissionResponseGrant GrantedTo { get; set; }

        /// <summary>
        /// Unique identifier of the grant
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Invitation for a specific user to access the OneDrive item
        /// </summary>
        [JsonPropertyName("invitation")]
        public OneDrivePermissionResponseInvitation Invitation { get; set; }

        /// <summary>
        /// The roles assigned to the item
        /// </summary>
        [JsonPropertyName("roles")]
        public string[] Roles { get; set; }
    }
}
