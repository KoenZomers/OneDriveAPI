using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Office365 Service Discovery result with one service result
    /// </summary>
    public class ServiceDiscoveryItem
    {
        [JsonPropertyName("serviceEndpointUri")]
        public string ServiceEndPointUri { get; set; }

        [JsonPropertyName("serviceResourceId")]
        public string ServiceResourceId { get; set; }

        [JsonPropertyName("capability")]
        public string Capability { get; set; }

        [JsonPropertyName("serviceApiVersion")]
        public string ServiceApiVersion { get; set; }
    }
}
