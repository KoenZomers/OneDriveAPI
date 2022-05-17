using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KoenZomers.OneDrive.Api.Entities
{
    /// <summary>
    /// Office365 Service Discovery result with set of services being returned
    /// </summary>
    public class ServiceDiscoverySet
    {
        [JsonPropertyName("value")]
        public List<ServiceDiscoveryItem> Services { get; set; }
    }
}
