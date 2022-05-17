using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveAudioFacet
    {
        [JsonPropertyName("album")]
        public string Album { get; set; }

        [JsonPropertyName("albumArtist")]
        public string AlbumArtist { get; set; }

        [JsonPropertyName("artist")]
        public string Artist { get; set; }

        [JsonPropertyName("bitrate")]
        public int BitRate { get; set; }

        [JsonPropertyName("copyright")]
        public string Copyright { get; set; }

        [JsonPropertyName("disc")]
        public int Disc { get; set; }

        [JsonPropertyName("discCount")]
        public int DiscCount { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("genre")]
        public string Genre { get; set; }

        [JsonPropertyName("hasDrm")]
        public bool HasDrm { get; set; }

        [JsonPropertyName("isVariableBitrate")]
        public bool IsVariableBitRate { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("track")]
        public int Track { get; set; }

        [JsonPropertyName("trackCount")]
        public int TrackCount { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }
    }
}
