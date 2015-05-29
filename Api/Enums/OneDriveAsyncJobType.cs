using System.Runtime.Serialization;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Enums
{
    public enum OneDriveAsyncJobType
    {
        [EnumMember(Value = "DownloadUrl")]
        DownloadUrl,
        [EnumMember(Value = "CopyItem")]
        CopyItem
    }
}
