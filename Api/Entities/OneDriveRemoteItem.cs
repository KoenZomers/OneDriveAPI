using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// The Item resource type represents metadata for an item in OneDrive that links to a OneDriveItem stored on another OneDrive
    /// </summary>
    public class OneDriveRemoteItem : OneDriveItemBase
    {
        /// <summary>
        /// The unique identifier of the item within the linked OneDrive
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Size of the item in bytes. Read-only.
        /// </summary>
        [JsonPropertyName("size")]
        public long Size { get; set; }

        /// <summary>
        /// URL that displays the resource in the browser. Read-only.
        /// </summary>
        [JsonPropertyName("webUrl")]
        public string WebUrl { get; set; }

        /// <summary>
        /// Parent information, if the item has a parent. Writeable
        /// </summary>
        [JsonPropertyName("parentReference")]
        public OneDriveItemReference ParentReference { get; set; }

        /// <summary>
        /// Folder metadata, if the item is a folder. Read-only.
        /// </summary>
        [JsonPropertyName("folder")]
        public OneDriveFolderFacet Folder { get; set; }

        /// <summary>
        /// File metadata, if the item is a file
        /// </summary>
        [JsonPropertyName("file")]
        public OneDriveFileFacet File { get; set; }

        /// <summary>
        /// Date and time at which the item has been created
        /// </summary>
        [JsonPropertyName("createdDateTime")]
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Date and time at which the item was last modified
        /// </summary>
        [JsonPropertyName("lastModifiedDateTime")]
        public string LastModifiedDateTime { get; set; }

        /// <summary>
        /// User that last modified the contents of this item
        /// </summary>
        [JsonPropertyName("lastModifiedBy")]
        public OneDriveUserProfile LastModifiedBy { get; set; }

        /// <summary>
        /// User that created the contents of this item
        /// </summary>
        [JsonPropertyName("createdBy")]
        public OneDriveUserProfile CreatedBy { get; set; }

        /// <summary>
        /// SharePoint specific identifiers for this item
        /// </summary>
        [JsonPropertyName("sharepointIds")]
        public OneDriveForBusinessSharePointId SharePointIds { get; set; }

        /// <summary>
        /// Name of the item
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Url to use with the WebDav protocol to access this item
        /// </summary>
        [JsonPropertyName("webDavUrl")]
        public string WebDavUrl { get; set; }

        /// <summary>
        /// Information about the owner of the shared item
        /// </summary>
        [JsonPropertyName("shared")]
        public OneDriveSharedItem Shared { get; set; }
    }
}
