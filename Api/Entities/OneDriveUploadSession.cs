using System;
using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveUploadSession : OneDriveItemBase
    {
        [JsonPropertyName("uploadUrl")]
        public string UploadUrl { get; set; }

        [JsonPropertyName("expirationDateTime")]
        public DateTimeOffset Expiration { get; set; }

        [JsonPropertyName("nextExpectedRanges")]
        public string[] ExpectedRanges { get; set; }
    }
}
