using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Contains information regarding an access token to OneDrive
    /// </summary>
    public class OneDriveAccessToken
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int AccessTokenExpirationDuration { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scopes { get; set; }

        [JsonPropertyName("authentication_token")]
        public string AuthenticationToken { get; set; }
    }
}
