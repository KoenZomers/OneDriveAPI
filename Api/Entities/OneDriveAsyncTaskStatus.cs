using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveAsyncTaskStatus : OneDriveItemBase
    {
        [JsonPropertyName("operation")]
        public Enums.OneDriveAsyncJobType Operation { get; set; }

        [JsonPropertyName("percentageComplete")]
        public double PercentComplete { get; set; }

        [JsonPropertyName("status")]
        public Enums.OneDriveAsyncJobStatus Status { get; set; }
        
    }
}
