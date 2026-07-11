using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using KoenZomers.OneDrive.Api.Entities;
using KoenZomers.OneDrive.Api.Helpers;
using System.Text.Json.Serialization;
using KoenZomers.OneDrive.Api.Enums;
using System.Net.Http;
using System.Text.Json;

namespace KoenZomers.OneDrive.Api
{
    /// <summary>
    /// API for OneDrive for Business on Office 365
    /// Create your own Client ID / Client Secret at https://entra.microsoft.com
    /// </summary>
    public class OneDriveForBusinessO365Api : OneDriveApi
    {
        #region Constants

        /// <summary>
        /// The url to provide as the redirect URL after successful authentication
        /// </summary>
        public override string AuthenticationRedirectUrl { get; set; } = "https://login.microsoftonline.com/common/oauth2/nativeclient";

        /// <summary>
        /// The Microsoft Entra ID (Azure AD v2.0) authority to authenticate against
        /// </summary>
        protected override string Authority => "https://login.microsoftonline.com/common/";

        /// <summary>
        /// String formatted Uri that can be called to sign out from the OneDrive API
        /// </summary>
        public override string SignoutUri => "https://login.microsoftonline.com/common/oauth2/v2.0/logout";

        /// <summary>
        /// Defines the maximum allowed file size that can be used for basic uploads
        /// </summary>
        public new static long MaximumBasicFileUploadSizeInBytes = 5 * 1024;

        #endregion

        #region Properties

        /// <summary>
        /// The root SharePoint/OneDrive for Business resource Uri for the tenant, e.g. https://contoso-my.sharepoint.com
        /// Used both as the base Uri for API calls and to construct the MSAL v2 '.default' scope to request access to.
        /// </summary>
        public string OneDriveForBusinessResourceUri { get; }

        /// <summary>
        /// The default scopes to request access to for the OneDrive for Business resource
        /// </summary>
        protected override string[] DefaultScopes => new[] { $"{OneDriveForBusinessResourceUri}/.default" };

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new instance of the OneDrive for Business API
        /// </summary>
        /// <param name="clientId">OneDrive Client ID to use to connect</param>
        /// <param name="clientSecret">OneDrive Client Secret to use to connect</param>
        /// <param name="oneDriveForBusinessResourceUri">The root SharePoint/OneDrive for Business resource Uri for the tenant, e.g. https://contoso-my.sharepoint.com. Used to construct the MSAL v2 scope and the API base Uri, replacing the legacy Office 365 Discovery Service.</param>
        public OneDriveForBusinessO365Api(string clientId, string clientSecret, string oneDriveForBusinessResourceUri) : base(clientId, clientSecret)
        {
            if (string.IsNullOrEmpty(oneDriveForBusinessResourceUri))
            {
                throw new ArgumentException("A OneDrive for Business resource Uri (e.g. https://contoso-my.sharepoint.com) must be provided. This replaces the legacy Office 365 Discovery Service that is no longer used now that authentication is handled through MSAL.", nameof(oneDriveForBusinessResourceUri));
            }

            OneDriveForBusinessResourceUri = oneDriveForBusinessResourceUri.TrimEnd('/');
            OneDriveApiBaseUrl = string.Concat(OneDriveForBusinessResourceUri, "/_api/v2.0/");

            InitializeMsalClientApplication();
        }

        #endregion

        #region Public Methods - Validate

        /// <summary>
        /// Validates if the provided filename is valid to be used on OneDrive
        /// </summary>
        /// <param name="filename">Filename to validate</param>
        /// <returns>True if filename is valid to be used, false if it isn't</returns>
        public override bool ValidFilename(string filename)
        {
            char[] restrictedCharacters = { '\\', '/', ':', '*', '?', '"', '<', '>', '|', '#', '%' };
            return filename.IndexOfAny(restrictedCharacters) == -1;
        }

        #endregion

        #region Public Methods - OneDrive for Business Only

        /// <summary>
        /// Returns all the items that have been shared by others through OneDrive for Business with the current user
        /// </summary>
        /// <returns>Collection with items that have been shared by others with the current user</returns>
        public override async Task<OneDriveItemCollection> GetSharedWithMe()
        {
            var oneDriveItems = await GetData<OneDriveItemCollection>("drive/view.sharedWithMe");
            return oneDriveItems;
        }

        /// <summary>
        /// Shares a OneDrive item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        public override async Task<OneDrivePermission> ShareItem(string itemPath, OneDriveLinkType linkType)
        {
            return await ShareItemInternal(string.Concat("drive/root:/", itemPath, ":/createLink"), linkType);
        }

        /// <summary>
        /// Shares a OneDrive item
        /// </summary>
        /// <param name="item">The OneDrive item to share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        public override async Task<OneDrivePermission> ShareItem(OneDriveItem item, OneDriveLinkType linkType)
        {
            return await ShareItemInternal(string.Concat("drive/items/", item.Id, "/createLink"), linkType);
        }

        /// <summary>
        /// Shares a OneDrive item by creating an anonymous link to the item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <param name="scope">Scope defining who has access to the shared item</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        public async Task<OneDrivePermission> ShareItem(string itemPath, OneDriveLinkType linkType, OneDriveSharingScope scope)
        {
            return await ShareItemInternal(string.Concat("drive/root:/", itemPath, ":/createLink"), linkType, scope);
        }

        /// <summary>
        /// Shares a OneDrive item by creating an anonymous link to the item
        /// </summary>
        /// <param name="item">The OneDrive item to share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <param name="scope">Scope defining who has access to the shared item</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        public async Task<OneDrivePermission> ShareItem(OneDriveItem item, OneDriveLinkType linkType, OneDriveSharingScope scope)
        {
            return await ShareItemInternal(string.Concat("drive/items/", item.Id, "/createLink"), linkType, scope);
        }

        /// <summary>
        /// Initiates a resumable upload session to OneDrive. It doesn't perform the actual upload yet.
        /// </summary>
        /// <param name="fileName">Filename to store the uploaded content under</param>
        /// <param name="oneDriveItem">OneDriveItem container in which the file should be uploaded</param>
        /// <returns>OneDriveUploadSession instance containing the details where to upload the content to</returns>
        protected override async Task<OneDriveUploadSession> CreateResumableUploadSession(string fileName, OneDriveItem oneDriveItem)
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/items/", oneDriveItem.Id, ":/", fileName, ":/createUploadSession");

            // Construct the OneDriveUploadSessionItemContainer entity with the upload details
            // Add the conflictbehavior header to always overwrite the file if it already exists on OneDrive
            var uploadItemContainer = new OneDriveUploadSessionItemContainer
            {
                Item = new OneDriveUploadSessionItem
                {
                    FilenameConflictBehavior = NameConflictBehavior.Replace
                }
            };

            // Call the OneDrive webservice
            var result = await SendMessageReturnOneDriveItem<OneDriveUploadSession>(uploadItemContainer, HttpMethod.Post, completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Constructs the complete Url to be called based on the part of the url provided that contains the command
        /// </summary>
        /// <param name="commandUrl">Part of the URL to call that contains the command to execute for the API that is being called</param>
        /// <returns>Full URL to call the API</returns>
        protected override string ConstructCompleteUrl(string commandUrl)
        {
            if (commandUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                return commandUrl;
            }
            return string.Concat(commandUrl.StartsWith("drives/", StringComparison.InvariantCultureIgnoreCase) ? OneDriveApiBaseUrl.EndsWith("me/") ? OneDriveApiBaseUrl.Remove(OneDriveApiBaseUrl.LastIndexOf("me/", StringComparison.OrdinalIgnoreCase)) : OneDriveApiBaseUrl : OneDriveApiBaseUrl, commandUrl);
        }

        #endregion
    }
}
