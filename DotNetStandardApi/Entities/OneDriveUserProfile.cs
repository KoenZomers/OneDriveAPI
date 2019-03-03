namespace KoenZomers.OneDrive.Api.Entities
{
    public class OneDriveUserProfile : OneDriveItemBase
    {
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string EmailAddress { get; set; }

        public string Username { get; set; }

        public string Organization { get; set; }
    }
}
