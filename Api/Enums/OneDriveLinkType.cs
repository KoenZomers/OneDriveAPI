using System.Runtime.Serialization;

namespace KoenZomers.OneDrive.Sync.BusinessLogic.Enums
{
    public enum OneDriveLinkType
    {
        /// <summary>
        /// Read-only
        /// </summary>
        [EnumMember(Value = "view")]
        View,

        /// <summary>
        /// Read-write
        /// </summary>
        [EnumMember(Value = "edit")]
        Edit
    }
}
