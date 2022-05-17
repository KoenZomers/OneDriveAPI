using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public abstract class OneDriveItemBase
    {
        /// <summary>
        /// The original raw JSON message
        /// </summary>
        [JsonIgnore]
        public string OriginalJson { get; set; }
    }
}
