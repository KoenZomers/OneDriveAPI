using System.Collections.Generic;
using System.Text.Json.Serialization;
using System;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Information regarding a SharePoint site
    /// </summary>
    public class SharePointSite : OneDriveItemBase
    {
        /// <summary>
        /// Date and time at which the site was created
        /// </summary>
        [JsonPropertyName("createdDateTime")]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Description of the site
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Unique identifier of the site
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Date and time at which the site was last modified
        /// </summary>
        [JsonPropertyName("lastModifiedDateTime")]
        public DateTime? LastModifiedDateTime { get; set; }

        /// <summary>
        /// Name of the site
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Full URL to where the site resides
        /// </summary>
        [JsonPropertyName("webUrl")]
        public string WebUrl { get; set; }

        //[JsonPropertyName("siteCollection")]
        //public SiteCollection SiteCollection { get; set; }

        /// <summary>
        /// Title of the site
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }
    }
}
