using System;
using KoenZomers.OneDrive.Api.Enums;
using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// The Item resource type represents metadata for an item in OneDrive. All top-level filesystem objects in OneDrive are Item resources. If an item is a Folder or File facet, the Item resource will contain a value for either the folder or file property, respectively.
    /// </summary>
    public class OneDriveItem : OneDriveItemBase
    {
        /// <summary>
        /// The unique identifier of the item within the Drive. Read-only.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of the item (filename and extension). Writable.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// eTag for the entire item (metadata + content). Read-only.
        /// </summary>
        [JsonPropertyName("etag")]
        public string ETag { get; set; }

        /// <summary>
        /// An eTag for the content of the item. This eTag is not changed if only the metadata is changed. Read-only.
        /// </summary>
        [JsonPropertyName("ctag")]
        public string CTag { get; set; }

        /// <summary>
        /// Identity of the user, device, and application which created the item. Read-only.
        /// </summary>
        [JsonPropertyName("createdBy")]
        public OneDriveIdentitySet CreatedBy { get; set; }

        /// <summary>
        /// Date and time of item creation. Read-only.
        /// </summary>
        [JsonPropertyName("createdDateTime")]
        public DateTimeOffset CreatedDateTime { get; set; }

        /// <summary>
        /// Identity of the user, device, and application which last modified the item. Read-only.
        /// </summary>
        [JsonPropertyName("lastModifiedBy")]
        public OneDriveIdentitySet LastModifiedBy { get; set; }

        /// <summary>
        /// Date and time the item was last modified. Read-only.
        /// </summary>
        [JsonPropertyName("lastModifiedDateTime")]
        public DateTimeOffset LastModifiedDateTime { get; set; }

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
        /// File metadata, if the item is a file. Read-only.
        /// </summary>
        [JsonPropertyName("file")]
        public OneDriveFileFacet File { get; set; }

        /// <summary>
        /// Image metadata, if the item is an image. Read-only.
        /// </summary>
        [JsonPropertyName("image")]
        public OneDriveImageFacet Image { get; set; }

        /// <summary>
        ///  Photo metadata, if the item is a photo. Read-only. 
        /// </summary>
        [JsonPropertyName("photo")]
        public OneDrivePhotoFacet Photo { get; set; }

        /// <summary>
        /// Audio metadata, if the item is an audio file. Read-only.
        /// </summary>
        [JsonPropertyName("audio")]
        public OneDriveAudioFacet Audio { get; set; }

        /// <summary>
        /// Video metadata, if the item is a video. Read-only.
        /// </summary>
        [JsonPropertyName("video")]
        public OneDriveVideoFacet Video { get; set; }

        /// <summary>
        /// Location metadata, if the item has location data. Read-only.
        /// </summary>
        [JsonPropertyName("location")]
        public OneDriveLocationFacet Location { get; set; }

        /// <summary>
        /// Information about the deleted state of the item. Read-only.
        /// </summary>
        [JsonPropertyName("deleted")]
        public OneDriveTombstoneFacet Deleted { get; set; }

        [JsonPropertyName("specialFolder")]
        public OneDriveSpecialFolderFacet SpecialFolder { get; set; }

        /// <summary>
        /// The conflict resolution behavior for actions that create a new item. An item will never be returned with this annotation. Write-only.
        /// </summary>
        [JsonPropertyName("@microsoft.graph.conflictBehavior")]
        public NameConflictBehavior? NameConflictBehahiorAnnotation { get; set; }

        /// <summary>
        /// A Url that can be used to download this file's content. Authentication is not required with this URL. Read-only.
        /// </summary>
        [JsonPropertyName("@content.downloadUrl")]
        public string DownloadUrlAnnotation { get; set; }

        /// <summary>
        /// When issuing a PUT request, this instance annotation can be used to instruct the service to download the contents of the URL, and store it as the file. Write-only.
        /// </summary>
        [JsonPropertyName("@content.sourceUrl")]
        public string SourceUrlAnnotation { get; set; }

        [JsonPropertyName("children@odata.nextLink")]
        public string ChildrenNextLinkAnnotation { get; set; }

        /// <summary>
        /// Collection containing ThumbnailSet objects associated with the item. For more info, see getting thumbnails.
        /// </summary>
        [JsonPropertyName("thumbnails")]
        public OneDriveThumbnailSet[] Thumbnails { get; set; }

        /// <summary>
        /// Collection containing Item objects for the immediate children of Item. Only items representing folders have children.
        /// </summary>
        [JsonPropertyName("children")]
        public OneDriveItem[] Children { get; set; }

        /// <summary>
        /// If containing information, it regards a OneDriveItem stored on another OneDrive but linked by the current OneDrive
        /// </summary>
        [JsonPropertyName("remoteItem")]
        public OneDriveRemoteItem RemoteItem{ get; set; }

        /// <summary>
        /// Information about the owner of a shared item
        /// </summary>
        [JsonPropertyName("shared")]
        public OneDriveSharedItem Shared { get; set; }
    }
}
