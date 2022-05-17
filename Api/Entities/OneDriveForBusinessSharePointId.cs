using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Entity representing a reference to a SharePoint list in OneDrive for Business
    /// </summary>
    public class OneDriveForBusinessSharePointId
    {
        /// <summary>
        /// Unique numeric identifier of the item in the SharePoint list
        /// </summary>
        [JsonPropertyName("listItemId")]
        public string ListItemId { get; set; }

        /// <summary>
        /// Unique GUID identifier of the item in the SharePoint list
        /// </summary>
        [JsonPropertyName("listItemUniqueId")]
        public string ListItemUniqueId { get; set; }

        /// <summary>
        /// Unique GUID identifier of the site collection the SharePoint list resides in
        /// </summary>
        [JsonPropertyName("siteId")]
        public string SiteId { get; set; }

        /// <summary>
        /// Unique GUID identifier of the site the SharePoint list resides in
        /// </summary>
        [JsonPropertyName("webId")]
        public string WebId { get; set; }

        /// <summary>
        /// Unique GUID identifier of the SharePoint list
        /// </summary>
        [JsonPropertyName("listId")]
        public string ListId { get; set; }
    }
}
