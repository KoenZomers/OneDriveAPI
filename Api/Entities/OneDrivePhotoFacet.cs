using System;
using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDrivePhotoFacet
    {
        [JsonPropertyName("takenDateTime")]
        public DateTimeOffset TakenDateTime { get; set; }

        [JsonPropertyName("cameraMake")]
        public string CameraMake { get; set; }
        
        [JsonPropertyName("cameraModel")]
        public string CameraModel { get; set; }

        [JsonPropertyName("fNumber")]
        public double FStop { get; set; }

        [JsonPropertyName("exposureDenominator")]
        public double ExposureDenominator { get; set; }

        [JsonPropertyName("exposureNumerator")]
        public double ExposureNumerator { get; set; }

        [JsonPropertyName("focalLength")]
        public double FocalLength { get; set; }

        [JsonPropertyName("iso")]
        public int ISO { get; set; }
    }
}
