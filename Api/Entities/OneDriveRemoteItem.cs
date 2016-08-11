using System;
using KoenZomers.OneDrive.Api.Enums;
using Newtonsoft.Json;

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
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }

        /// <summary>
        /// Size of the item in bytes. Read-only.
        /// </summary>
        [JsonProperty("size", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Size { get; set; }

        /// <summary>
        /// URL that displays the resource in the browser. Read-only.
        /// </summary>
        [JsonProperty("webUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string WebUrl { get; set; }

        /// <summary>
        /// Parent information, if the item has a parent. Writeable
        /// </summary>
        [JsonProperty("parentReference", NullValueHandling = NullValueHandling.Ignore)]
        public OneDriveItemReference ParentReference { get; set; }

        /// <summary>
        /// Folder metadata, if the item is a folder. Read-only.
        /// </summary>
        [JsonProperty("folder", NullValueHandling = NullValueHandling.Ignore)]
        public OneDriveFolderFacet Folder { get; set; }
    }
}
