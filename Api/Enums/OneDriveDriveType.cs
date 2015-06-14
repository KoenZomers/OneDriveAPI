using System.Runtime.Serialization;

namespace KoenZomers.OneDrive.Api.Enums
{
    /// <summary>
    /// Types of OneDrive
    /// </summary>
    public enum OneDriveDriveType
    {
        /// <summary>
        /// Public OneDrive
        /// </summary>
        [EnumMember(Value = "personal")] 
        Personal,
        
        /// <summary>
        /// Enterprise OneDrive
        /// </summary>
        [EnumMember(Value = "business")]
        Business
    }
}