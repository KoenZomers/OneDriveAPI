using System.Runtime.Serialization;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Enums
{
    public enum OneDriveAsyncJobStatus
    {
        [EnumMember(Value = "NotStarted")]
        NotStarted,

        [EnumMember(Value = "InProgress")]
        InProgress,

        [EnumMember(Value = "Complete")]
        Complete
    }
}
