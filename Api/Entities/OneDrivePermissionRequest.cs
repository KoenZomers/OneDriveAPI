using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Requests a new permission on a OneDrive item
    /// </summary>
    public class OneDrivePermissionRequest : OneDriveItemBase
    {
        /// <summary>
        /// The personal message to add to the e-mail notification sent to invitees
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// The type of permission, can be read or write
        /// </summary>
        [JsonPropertyName("roles")]
        public string[] Roles { get; set; }

        /// <summary>
        /// The people that should be invited to gain access to the OneDrive item
        /// </summary>
        [JsonPropertyName("recipients")]
        public OneDriveDriveRecipient[] Recipients { get; set; }

        /// <summary>
        /// Boolean indicating if the invitee should sign in before being able to access the item
        /// </summary>
        [JsonPropertyName("requireSignin")]
        public bool RequireSignin { get; set; }

        /// <summary>
        /// Boolean indicating if the invitee should receive an e-mail notification to indicate an item has been shared with them
        /// </summary>
        [JsonPropertyName("sendInvitation")]
        public bool SendInvitation { get; set; }
    }
}
