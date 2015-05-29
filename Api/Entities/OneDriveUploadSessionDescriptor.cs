using KoenZomers.OneDrive.Sync.BusinessLogic.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Entities
{
    internal class OneDriveUploadSessionDescriptor
    {
        [JsonProperty("@name.conflictBehavior", DefaultValueHandling = DefaultValueHandling.Ignore), JsonConverter(typeof(StringEnumConverter))]
        public NameConflictBehavior FilenameConflictBehavior { get; set; }

        [JsonProperty("name", DefaultValueHandling=DefaultValueHandling.Ignore)]
        public string Filename { get; set; }
    }
}
