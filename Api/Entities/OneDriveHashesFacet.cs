using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Hashes of the file
    /// </summary>
    public class OneDriveHashesFacet
    {
        /// <summary>
        /// SHA1 hash of the file
        /// </summary>
        [JsonPropertyName("sha1Hash")]
        public string Sha1 { get; set; }

        /// <summary>
        /// CRC32 hash of the file
        /// </summary>
        [JsonPropertyName("crc32Hash")]
        public string Crc32 { get; set; }
    }
}
