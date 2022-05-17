using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Details about an error that occurred
    /// </summary>
    public class OneDriveErrorDetail : OneDriveItemBase
    {
        /// <summary>
        /// Error code
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Target platform
        /// </summary>
        [JsonPropertyName("target")]
        public string Target { get; set; }

        /// <summary>
        /// Error occurring that lead to this error
        /// </summary>
        [JsonPropertyName("innererror")]
        public OneDriveErrorDetail InnerError { get; set; }
    }
}
