using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Represents an user
    /// </summary>
    public class OneDriveUserProfile : OneDriveItemBase
    {
        /// <summary>
        /// Unique identifier of the user
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Friendly name of the user
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// E-mail address of the user
        /// </summary>
        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Username of the user
        /// </summary>
        [JsonPropertyName("username")]
        public string Username { get; set; }

        /// <summary>
        /// Organization the user belongs to
        /// </summary>
        [JsonPropertyName("organization")]
        public string Organization { get; set; }
    }
}
