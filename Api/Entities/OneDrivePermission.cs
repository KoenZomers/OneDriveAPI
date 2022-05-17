using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Represents a permission on a OneDrive item
    /// </summary>
    public class OneDrivePermission : OneDriveItemBase
    {
        /// <summary>
        /// The unique identifier of the permission among all permissions on the item
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The type of permission, can be read or write
        /// </summary>
        [JsonPropertyName("roles")]
        public string[] Roles { get; set; }

        /// <summary>
        /// Provides the link details of the current permission, if it is a link type permissions
        /// </summary>
        [JsonPropertyName("link")]
        public OneDriveSharingLinkFacet Link { get; set; }

        /// <summary>
        /// Provides a reference to the ancestor of the current permission, if it is inherited from an ancestor
        /// </summary>
        [JsonPropertyName("inheritedFrom")]
        public OneDriveItemReference InheritedFrom { get; set; }
    }
}
