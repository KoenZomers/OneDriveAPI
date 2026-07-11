using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using KoenZomers.OneDrive.Api.Entities;
using KoenZomers.OneDrive.Api.Enums;
using KoenZomers.OneDrive.Api.Helpers;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Identity.Client;

namespace KoenZomers.OneDrive.Api
{
    /// <summary>
    /// API for both OneDrive Personal and OneDrive for Business on Office 365 through the Microsoft Graph API.
    /// Create your own Client ID / Client Secret at https://entra.microsoft.com
    /// </summary>
    public class OneDriveGraphApi
    {
        #region Properties

        /// <summary>
        /// The oAuth 2.0 Application Client ID
        /// </summary>
        public string ClientId { get; protected set; }

        /// <summary>
        /// The oAuth 2.0 Application Client Secret
        /// </summary>
        public string ClientSecret { get; protected set; }

        /// <summary>
        /// If provided, this proxy will be used for communication with the OneDrive API. If not provided, no proxy will be used.
        /// </summary>
        public IWebProxy ProxyConfiguration { get; set; }

        /// <summary>
        /// If provided along with a proxy configuration, these credentials will be used to authenticate to the proxy. If omitted, the default system credentials will be used.
        /// </summary>
        public NetworkCredential ProxyCredential { get; set; }

        /// <summary>
        /// Authorization token used for requesting tokens
        /// </summary>
        public string AuthorizationToken { get; private set; }

        /// <summary>
        /// Access Token for communicating with OneDrive
        /// </summary>
        public OneDriveAccessToken AccessToken { get; protected set; }

        /// <summary>
        /// Date and time until which the access token should be valid based on the information provided by MSAL
        /// </summary>
        public DateTime? AccessTokenValidUntil { get; protected set; }

        /// <summary>
        /// The MSAL public client application used for authentication flows that do not require a client secret
        /// (e.g. AcquireTokenInteractive, AcquireTokenByDeviceCode, AcquireTokenByAuthorizationCode, AcquireTokenSilent).
        /// Will be NULL when a client secret was provided, in which case <see cref="ConfidentialClientApplication"/> is used instead.
        /// Exposed directly so consumers can use any authentication flow supported by MSAL, not just the ones wrapped by this library.
        /// </summary>
        public IPublicClientApplication PublicClientApplication { get; private set; }

        /// <summary>
        /// The MSAL confidential client application used for authentication flows that require a client secret
        /// (e.g. AcquireTokenByAuthorizationCode, AcquireTokenForClient for app-only scenarios, AcquireTokenSilent).
        /// Will be NULL when no client secret was provided, in which case <see cref="PublicClientApplication"/> is used instead.
        /// Exposed directly so consumers can use any authentication flow supported by MSAL, not just the ones wrapped by this library.
        /// </summary>
        public IConfidentialClientApplication ConfidentialClientApplication { get; private set; }

        /// <summary>
        /// The MSAL account that was used for the last successful authentication. Used to acquire tokens silently from the MSAL token cache.
        /// </summary>
        protected IAccount Account { get; set; }

        /// <summary>
        /// Base URL of the OneDrive API
        /// </summary>
        protected string OneDriveApiBaseUrl { get; set; }

        /// <summary>
        /// Defines the maximum allowed file size that can be used for basic uploads. Should be set 4 MB as described in the API documentation at https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/item_uploadcontent
        /// </summary>
        public static long MaximumBasicFileUploadSizeInBytes = 4 * 1024;

        /// <summary>
        /// Size of the chunks to upload when using the resumable upload method. Must be a multiple of 327680 bytes. See https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/driveitem_createuploadsession#best-practices
        /// </summary>
        public long ResumableUploadChunkSizeInBytes = 10485760;

        /// <summary>
        /// The url to provide as the redirect URL after successful authentication.
        /// Uses the loopback address recommended by Microsoft for MSAL desktop/public client applications;
        /// MSAL will automatically pick a free port on this address for interactive/authorization-code flows.
        /// Register this exact value (http://localhost) as a "Mobile and desktop applications" redirect URI on the app registration.
        /// </summary>
        public string AuthenticationRedirectUrl { get; set; } = "http://localhost";

        /// <summary>
        /// The Microsoft Entra ID (Azure AD v2.0) authority to authenticate against
        /// </summary>
        protected string Authority => "https://login.microsoftonline.com/common/";

        /// <summary>
        /// The default scopes to request access to at the Graph API
        /// </summary>
        protected string[] DefaultScopes => new[] { "offline_access", "files.readwrite.all" };

        /// <summary>
        /// Returns the default MSAL scopes used by this API instance. Useful when calling MSAL authentication flows
        /// directly on <see cref="PublicClientApplication"/> or <see cref="ConfidentialClientApplication"/>.
        /// </summary>
        public string[] GetDefaultScopes() => DefaultScopes;

        /// <summary>
        /// String formatted Uri that can be called to sign out from the Graph API
        /// </summary>
        public string SignoutUri => "https://login.microsoftonline.com/common/oauth2/v2.0/logout";

        /// <summary>
        /// Base URL of the Graph API
        /// </summary>
        protected string GraphApiBaseUrl => "https://graph.microsoft.com/v1.0/";

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when uploading a file using UploadFileViaResumableUpload to indicate the progress of the upload process
        /// </summary>
        public event EventHandler<OneDriveUploadProgressChangedEventArgs> UploadProgressChanged;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new instance of the Graph API
        /// </summary>
        /// <param name="clientId">OneDrive Client ID to use to connect. Must be provided by the consumer of this library - register your own app registration in the Microsoft Entra admin center (https://entra.microsoft.com).</param>
        /// <param name="clientSecret">OneDrive Client Secret to use to connect</param>
        /// <exception cref="ArgumentException">Thrown when no clientId is provided</exception>
        public OneDriveGraphApi(string clientId, string clientSecret = null)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("A client ID must be provided. Register your own app registration in the Microsoft Entra admin center (https://entra.microsoft.com) and provide its Application (client) ID.", nameof(clientId));
            }

            ClientId = clientId;
            ClientSecret = clientSecret;
            OneDriveApiBaseUrl = GraphApiBaseUrl + "me/";

            InitializeMsalClientApplication();
        }

        #endregion

        #region Public Methods - Authentication

        /// <summary>
        /// Initializes the MSAL client application(s) used for authentication.
        /// Builds a <see cref="ConfidentialClientApplication"/> when a client secret was provided, otherwise a <see cref="PublicClientApplication"/>.
        /// </summary>
        private void InitializeMsalClientApplication()
        {
            if (!string.IsNullOrEmpty(ClientSecret))
            {
                ConfidentialClientApplication = ConfidentialClientApplicationBuilder.Create(ClientId)
                    .WithClientSecret(ClientSecret)
                    .WithAuthority(new Uri(Authority))
                    .WithRedirectUri(AuthenticationRedirectUrl)
                    .Build();
            }
            else
            {
                PublicClientApplication = PublicClientApplicationBuilder.Create(ClientId)
                    .WithAuthority(new Uri(Authority))
                    .WithRedirectUri(AuthenticationRedirectUrl)
                    .Build();
            }
        }

        /// <summary>
        /// Returns the Uri that needs to be called to authenticate to the OneDrive API using the "bring your own browser" authorization code pattern.
        /// Only available when using a confidential client (a client secret was configured). For public clients, use
        /// <see cref="PublicClientApplication"/> directly with e.g. AcquireTokenInteractive or AcquireTokenByDeviceCode, which
        /// manage the browser/device-code interaction themselves rather than exposing a raw authorization URL.
        /// </summary>
        /// <param name="scopes">Scopes to request access for. Omit to use the API's default scopes.</param>
        /// <returns>Uri that needs to be called in a browser to authenticate to the OneDrive API</returns>
        public virtual async Task<Uri> GetAuthenticationUri(string[] scopes = null)
        {
            if (ConfidentialClientApplication == null)
            {
                throw new InvalidOperationException("GetAuthenticationUri requires a confidential client application (a client secret must be configured). For public clients, use PublicClientApplication directly with AcquireTokenInteractive, AcquireTokenByDeviceCode or another MSAL flow.");
            }

            return await ConfidentialClientApplication.GetAuthorizationRequestUrl(scopes ?? DefaultScopes)
                .WithRedirectUri(AuthenticationRedirectUrl)
                .ExecuteAsync();
        }

        /// <summary>
        /// Returns the authorization token from the provided URL to which the OneDrive API authentication request was sent after succesful authentication
        /// </summary>
        /// <param name="url">Url received from the OneDrive API after succesful authentication</param>
        /// <returns>Authorization token or NULL if unable to identify from provided URL</returns>
        public string GetAuthorizationTokenFromUrl(string url)
        {
            // Url must be provided
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            // Url must start with the return url followed by a question mark to provide querystring parameters
            if (!url.StartsWith(string.Concat(AuthenticationRedirectUrl, "?")) && !url.StartsWith(string.Concat(AuthenticationRedirectUrl, "/?")))
            {
                return null;
            }

            // Get the querystring parameters from the URL
            var queryString = url.Remove(0, AuthenticationRedirectUrl.Length + 1);
            var queryStringParams = HttpUtility.ParseQueryString(queryString);

            AuthorizationToken = queryStringParams["code"];
            return AuthorizationToken;
        }

        /// <summary>
        /// Tries to retrieve an access token based on the tokens already available in this OneDrive instance, using MSAL's
        /// silent token acquisition (cached token or refreshed automatically). If no cached account is available yet and an
        /// authorization code was captured through <see cref="GetAuthorizationTokenFromUrl"/>, it will be exchanged for a token.
        /// </summary>
        /// <returns>OneDrive access token or NULL if unable to get an access token without user interaction</returns>
        public async Task<OneDriveAccessToken> GetAccessToken()
        {
            // Check if we have an access token that is still valid
            if (AccessToken != null && AccessTokenValidUntil.HasValue && AccessTokenValidUntil.Value > DateTime.Now)
            {
                return AccessToken;
            }

            // Try to silently acquire (or renew) a token from MSAL's token cache
            if (Account != null)
            {
                try
                {
                    var silentResult = PublicClientApplication != null
                        ? await PublicClientApplication.AcquireTokenSilent(DefaultScopes, Account).ExecuteAsync()
                        : await ConfidentialClientApplication.AcquireTokenSilent(DefaultScopes, Account).ExecuteAsync();

                    return SetAuthenticationResult(silentResult);
                }
                catch (MsalUiRequiredException)
                {
                    // Silent renewal is not possible, interactive authentication is required
                    return null;
                }
                catch (MsalException ex)
                {
                    throw new Exceptions.TokenRetrievalFailedException(message: ex.Message, innerException: ex);
                }
            }

            // No cached account is available, check if we have an authorization code to exchange for a token
            if (string.IsNullOrEmpty(AuthorizationToken))
            {
                // No access token, no authorization code, interactive authentication is required first
                return null;
            }

            await AuthenticateUsingAuthorizationCode(AuthorizationToken);
            return AccessToken;
        }

        /// <summary>
        /// Returns the Uri that needs to be called to sign the current user out of the OneDrive API
        /// </summary>
        /// <returns>Uri that needs to be called to sign the current user out of the OneDrive API</returns>
        public Uri GetSignOutUri()
        {
            return new Uri(string.Format(SignoutUri, ClientId));
        }

        /// <summary>
        /// Stores the result of a MSAL authentication flow (e.g. AcquireTokenInteractive, AcquireTokenByDeviceCode,
        /// AcquireTokenByAuthorizationCode) so it can be used for subsequent calls to the OneDrive API and for silent renewal.
        /// Use this after calling any authentication flow directly on <see cref="PublicClientApplication"/> or
        /// <see cref="ConfidentialClientApplication"/> to give full flexibility in which MSAL flow to use.
        /// </summary>
        /// <param name="authenticationResult">The result returned by a MSAL AcquireToken* flow</param>
        /// <returns>OneDrive access token derived from the provided authentication result</returns>
        public OneDriveAccessToken SetAuthenticationResult(AuthenticationResult authenticationResult)
        {
            if (authenticationResult == null)
            {
                throw new ArgumentNullException(nameof(authenticationResult));
            }

            Account = authenticationResult.Account;
            AccessToken = new OneDriveAccessToken
            {
                TokenType = authenticationResult.TokenType,
                AccessToken = authenticationResult.AccessToken,
                Scopes = authenticationResult.Scopes != null ? string.Join(" ", authenticationResult.Scopes) : null,
                AccessTokenExpirationDuration = (int)Math.Max(0, (authenticationResult.ExpiresOn - DateTimeOffset.UtcNow).TotalSeconds)
            };
            AccessTokenValidUntil = authenticationResult.ExpiresOn.LocalDateTime;

            return AccessToken;
        }

        /// <summary>
        /// Authenticates to OneDrive using the provided authorization code (e.g. captured through <see cref="GetAuthorizationTokenFromUrl"/>)
        /// using MSAL's AcquireTokenByAuthorizationCode flow. Requires a confidential client (a client secret must be configured);
        /// for public clients use <see cref="PublicClientApplication"/> directly with AcquireTokenInteractive or another MSAL flow.
        /// </summary>
        /// <param name="authorizationCode">Authorization code to exchange for an access token</param>
        /// <param name="scopes">Scopes to request access for. Omit to use the API's default scopes.</param>
        /// <exception cref="Exceptions.TokenRetrievalFailedException">Thrown when unable to retrieve a valid access token</exception>
        public async Task AuthenticateUsingAuthorizationCode(string authorizationCode, string[] scopes = null)
        {
            if (ConfidentialClientApplication == null)
            {
                throw new InvalidOperationException("AuthenticateUsingAuthorizationCode requires a confidential client application (a client secret must be configured). For public clients, use PublicClientApplication directly with AcquireTokenInteractive, AcquireTokenByDeviceCode or another MSAL flow, then call SetAuthenticationResult with the result.");
            }

            try
            {
                var result = await ConfidentialClientApplication.AcquireTokenByAuthorizationCode(scopes ?? DefaultScopes, authorizationCode).ExecuteAsync();
                SetAuthenticationResult(result);
            }
            catch (MsalException ex)
            {
                throw new Exceptions.TokenRetrievalFailedException(message: ex.Message, innerException: ex);
            }
        }

        /// <summary>
        /// Authenticates to OneDrive using the provided Refresh Token through MSAL's refresh token migration API.
        /// Intended primarily for migrating refresh tokens obtained outside of MSAL (e.g. from a previous version of this library).
        /// For new authentication flows, prefer using <see cref="PublicClientApplication"/>/<see cref="ConfidentialClientApplication"/> directly.
        /// </summary>
        /// <param name="refreshToken">Refreshtoken to use to authenticate to OneDrive</param>
        /// <param name="scopes">Scopes to request access for. Omit to use the API's default scopes.</param>
        /// <exception cref="Exceptions.TokenRetrievalFailedException">Thrown when unable to retrieve a valid access token</exception>
        public async Task AuthenticateUsingRefreshToken(string refreshToken, string[] scopes = null)
        {
            try
            {
                var result = PublicClientApplication != null
                    ? await ((IByRefreshToken)PublicClientApplication).AcquireTokenByRefreshToken(scopes ?? DefaultScopes, refreshToken).ExecuteAsync()
                    : await ((IByRefreshToken)ConfidentialClientApplication).AcquireTokenByRefreshToken(scopes ?? DefaultScopes, refreshToken).ExecuteAsync();

                SetAuthenticationResult(result);
            }
            catch (MsalException ex)
            {
                throw new Exceptions.TokenRetrievalFailedException(message: ex.Message, innerException: ex);
            }
        }

        #endregion

        #region Public Methods - Validate

        /// <summary>
        /// Validates if the provided filename is valid to be used on OneDrive
        /// </summary>
        /// <param name="filename">Filename to validate</param>
        /// <returns>True if filename is valid to be used, false if it isn't</returns>
        public virtual bool ValidFilename(string filename)
        {
            char[] restrictedCharacters = { '\\', '/', ':', '*', '?', '"', '<', '>', '|', '#', '%' };
            return filename.IndexOfAny(restrictedCharacters) == -1;
        }

        #endregion

        #region Public Methods - Getting content

        /// <summary>
        /// Retrieves the current OneDrive drive information
        /// </summary>
        public virtual async Task<OneDriveDrive> GetDrive()
        {
            return await GetData<OneDriveDrive>("drive");
        }

        /// <summary>
        /// Retrieves the OneDrive drive information from the provided drive
        /// </summary>
        /// <param name="driveId">Id of the drive to retrieve</param>
        public virtual async Task<OneDriveDrive> GetDrive(string driveId)
        {
            return await GetData<OneDriveDrive>($"drives/{driveId}");
        }

        /// <summary>
        /// Retrieves the current OneDrive drive information from the provided drive
        /// </summary>
        /// <param name="drive">Drive to retrieve</param>
        public virtual async Task<OneDriveDrive> GetDrive(OneDriveDrive drive)
        {
            return await GetDrive(drive.Id);
        }

        /// <summary>
        /// Retrieves the OneDrive root folder
        /// </summary>
        public virtual async Task<OneDriveItem> GetDriveRoot()
        {
            return await GetData<OneDriveItem>("drive/root");
        }

        /// <summary>
        /// Retrieves the first batch of children under the OneDrive root folder
        /// </summary>
        /// <returns>OneDriveItemCollection containing the first batch of items in the root folder</returns>
        public virtual async Task<OneDriveItemCollection> GetDriveRootChildren()
        {
            return await GetData<OneDriveItemCollection>("drive/root/children");
        }

        /// <summary>
        /// Retrieves all the children under the OneDrive root folder
        /// </summary>
        /// <returns>OneDriveItem array containing all items in the requested folder</returns>
        public virtual async Task<OneDriveItem[]> GetAllDriveRootChildren()
        {
            return await GetAllChildrenInternal("drive/root/children");
        }

        /// <summary>
        /// Retrieves the first batch of children under the provided OneDrive path. Use GetNextChildrenByPath and provide the NextLink from the results to fetch the next batch.
        /// </summary>
        /// <param name="path">Path within OneDrive to retrieve the child items of</param>
        /// <returns>OneDriveItemCollection containing the first batch of items in the requested folder</returns>
        public virtual async Task<OneDriveItemCollection> GetChildrenByPath(string path)
        {
            return await GetData<OneDriveItemCollection>(string.Concat("drive/root:/", path, ":/children"));
        }

        /// <summary>
        /// Retrieves a next batch of children using the provided full SkipToken path
        /// </summary>
        /// <param name="skipTokenUrl">Full URL from a NextLink in the response of a GetChildrenByPath request</param>
        /// <returns>OneDriveItemCollection containing the next batch of items in the requested folder</returns>
        public virtual async Task<OneDriveItemCollection> GetNextChildrenByPath(string skipTokenUrl)
        {
            return await GetData<OneDriveItemCollection>(skipTokenUrl);
        }

        /// <summary>
        /// Retrieves all children under the provided OneDrive path
        /// </summary>
        /// <param name="path">Path within OneDrive to retrieve the child items of</param>
        /// <returns>OneDriveItem array containing all items in the requested folder</returns>
        public virtual async Task<OneDriveItem[]> GetAllChildrenByPath(string path)
        {
            return await GetAllChildrenInternal(string.Concat("drive/root:/", path, ":/children"));
        }

        /// <summary>
        /// Retrieves the first batch of children under the OneDrive folder with the provided id
        /// </summary>
        /// <param name="id">Unique identifier of the folder under which to retrieve the child items</param>
        /// <returns>OneDriveItemCollection containing the first batch of items in the folder</returns>
        public virtual async Task<OneDriveItemCollection> GetChildrenByFolderId(string id)
        {
            return await GetData<OneDriveItemCollection>(string.Concat("drive/items/", id, "/children"));
        }

        /// <summary>
        /// Retrieves all the children under the OneDrive folder with the provided id
        /// </summary>
        /// <param name="id">Unique identifier of the folder under which to retrieve the child items</param>
        /// <returns>OneDriveItem array containing all items in the requested folder</returns>
        public virtual async Task<OneDriveItem[]> GetAllChildrenByFolderId(string id)
        {
            return await GetAllChildrenInternal(string.Concat("drive/items/", id, "/children"));
        }

        /// <summary>
        /// Retrieves the firsth batch of children under the provided OneDrive Item
        /// </summary>
        /// <param name="item">OneDrive item to retrieve the child items of</param>
        /// <returns>OneDriveItemCollection containing the first batch of items in the folder</returns>
        public virtual async Task<OneDriveItemCollection> GetChildrenByParentItem(OneDriveItem item)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (item.RemoteItem != null)
            {
                // Item to get the children from is shared from another drive
                completeUrl = string.Concat("drives/", item.RemoteItem.ParentReference.DriveId, "/items/", item.RemoteItem.Id, "/children");
            }
            else if (!string.IsNullOrEmpty(item.ParentReference.DriveId))
            {
                // Item to get the children from is shared from another drive
                completeUrl = string.Concat("drives/", item.ParentReference.DriveId, "/items/", item.Id, "/children");
            }
            else
            {
                // Item to get the children from resides on the current user its drive
                completeUrl = string.Concat("drive/items/", item.Id, "/children");
            }

            return await GetData<OneDriveItemCollection>(completeUrl);
        }

        /// <summary>
        /// Retrieves all the children under the OneDrive folder with the provided id
        /// </summary>
        /// <param name="item">OneDrive item to retrieve the child items of</param>
        /// <returns>OneDriveItem array containing all items in the requested folder</returns>
        public virtual async Task<OneDriveItem[]> GetAllChildrenByParentItem(OneDriveItem item)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (item.RemoteItem != null)
            {
                // Item to get the children from is shared from another drive
                completeUrl = string.Concat("drives/", item.RemoteItem.ParentReference.DriveId, "/items/", item.RemoteItem.Id, "/children");
            }
            else if (!string.IsNullOrEmpty(item.ParentReference.DriveId))
            {
                // Item to get the children from is shared from another drive
                completeUrl = string.Concat("drives/", item.ParentReference.DriveId, "/items/", item.Id, "/children");
            }
            else
            {
                // Item to get the children from resides on the current user its drive
                completeUrl = string.Concat("drive/items/", item.Id, "/children");
            }

            return await GetAllChildrenInternal(completeUrl);
        }

        /// <summary>
        /// Retrieves all the children under the OneDrive folder with the provided id
        /// </summary>
        /// <param name="fetchUrl">Url to use to fetch the first set of child items</param>
        /// <returns>OneDriveItem array containing all items in the requested folder</returns>
        protected async Task<OneDriveItem[]> GetAllChildrenInternal(string fetchUrl)
        {
            var results = new List<OneDriveItem>();
            do
            {
                // Retrieve a batch with child items
                var resultSet = await GetData<OneDriveItemCollection>(fetchUrl);

                // Add the batch results to the complete list with results
                results.AddRange(resultSet.Collection);

                // Check if there's a NextLink in the response. If so, continue with fetching the next bach of items. If not, we're done.
                if (string.IsNullOrEmpty(resultSet.NextLink)) break;

                // Set the url where to get the next batch of items based on the NextLink provided in the previous batch results
                fetchUrl = resultSet.NextLink;
            } while (true);

            return results.ToArray();
        }

        /// <summary>
        /// Retrieves the OneDrive Item by its complete path to the file
        /// </summary>
        /// <param name="path">Path of the OneDrive item to retrieve</param>
        /// <returns>OneDriveItem representing the file or NULL if the file was not found</returns>
        public virtual async Task<OneDriveItem> GetItem(string path)
        {
            return await GetData<OneDriveItem>(string.Concat("drive/root:/", path));
        }

        /// <summary>
        /// Retrieves the OneDrive Item by it's filename in a specific folder
        /// </summary>
        /// <param name="folder">OneDriveItem representing the folder in which the file should reside</param>
        /// <param name="fileName">File name of the file to retrieve</param>
        /// <returns>OneDriveItem representing the file or NULL if the file was not found</returns>
        public virtual async Task<OneDriveItem> GetItemInFolder(OneDriveItem folder, string fileName)
        {
            var itemsInFolder = await GetAllChildrenByParentItem(folder);
            var item = itemsInFolder.FirstOrDefault(i => string.Equals(i.Name, fileName, StringComparison.InvariantCultureIgnoreCase));
            return item;
        }

        /// <summary>
        /// Retrieves the OneDrive Item by it's filename in a specific folder
        /// </summary>
        /// <param name="folderId">Unique identifier of the folder in which the file should reside</param>
        /// <param name="fileName">File name of the file to retrieve</param>
        /// <returns>OneDriveItem representing the file or NULL if the file was not found</returns>
        public virtual async Task<OneDriveItem> GetItemInFolder(string folderId, string fileName)
        {
            var folder = await GetItemById(folderId);
            if (folder == null) return null;
            return await GetItemInFolder(folder, fileName);
        }

        /// <summary>
        /// Retrieves the OneDrive Item from the current drive by it's unique identifier
        /// </summary>
        /// <param name="id">Unique identifier of the OneDrive item to retrieve</param>
        /// <returns>OneDriveItem representing the file or NULL if the file was not found</returns>
        public virtual async Task<OneDriveItem> GetItemById(string id)
        {
            return await GetData<OneDriveItem>(string.Concat("drive/items/", id));
        }

        /// <summary>
        /// Retrieves the OneDrive Item from the provided drive by it's unique identifier
        /// </summary>
        /// <param name="id">Unique identifier of the OneDrive item to retrieve</param>
        /// <param name="driveId">Id of the drive on which the item resides</param>
        /// <returns>OneDriveItem representing the file or NULL if the file was not found</returns>
        public virtual async Task<OneDriveItem> GetItemFromDriveById(string id, string driveId)
        {
            return await GetData<OneDriveItem>(string.Concat("drives/", driveId, "/items/", id));
        }

        /// <summary>
        /// Retrieves the OneDrive Item from the provided drive by it's unique identifier
        /// </summary>
        /// <param name="id">Unique identifier of the OneDrive item to retrieve</param>
        /// <param name="drive">Drive on which the item resides</param>
        /// <returns>OneDriveItem representing the file or NULL if the file was not found</returns>
        public virtual async Task<OneDriveItem> GetItemFromDriveById(string id, OneDriveDrive drive)
        {
            return await GetData<OneDriveItem>(string.Concat("drives/", drive.Id, "/items/", id));
        }

        /// <summary>
        /// Retrieves the OneDrive folder item or creates it if it doesn't exist yet
        /// </summary>
        /// <param name="path">Path of the OneDrive folder to retrieve or create. It will ensure that the whole provided path exists and create (sub)folders if they don't exist yet</param>
        /// <example>Files\Work\Contracts</example>
        /// <returns>OneDriveItem representing the folder designated in the path</returns>
        public virtual async Task<OneDriveItem> GetFolderOrCreate(string path)
        {
            // Replace possible forward slashes with backslashes
            path = path.Replace("/", "\\");

            // Check if the path contains multiple folders
            if(path.Contains("\\"))
            {
                // Path contains multiple folders, use recursion to ensure the entire path exists
                await GetFolderOrCreate(path.Remove(path.LastIndexOf("\\")));
            }

            // Try to get the folder
            var folder = await GetData<OneDriveItem>(string.Concat("drive/root:/", path));

            if (folder != null)
            {
                // Folder found, return it
                return folder;
            }

            // Folder not found, create it
            var folderName = path.Contains("\\") ? path.Remove(0, path.LastIndexOf("\\", StringComparison.Ordinal) + 1) : path;
            var parentPath = path.Contains("\\") ? path.Remove(path.Length - folderName.Length - 1) : "";
            folder = await CreateFolder(parentPath, folderName);

            return folder;
        }

        /// <summary>
        /// Retrieves the items in the CameraRoll folder
        /// </summary>
        public virtual async Task<OneDriveItemCollection> GetDriveCameraRollFolder()
        {
            return await GetData<OneDriveItemCollection>("drive/special/cameraroll");
        }

        /// <summary>
        /// Retrieves the items in the Documents folder
        /// </summary>
        public virtual async Task<OneDriveItemCollection> GetDriveDocumentsFolder()
        {
            return await GetData<OneDriveItemCollection>("drive/special/documents");
        }

        /// <summary>
        /// Retrieves the items in the Photos folder
        /// </summary>
        public virtual async Task<OneDriveItemCollection> GetDrivePhotosFolder()
        {
            return await GetData<OneDriveItemCollection>("drive/special/photos");
        }

        /// <summary>
        /// Retrieves the items in the Public folder
        /// </summary>
        public virtual async Task<OneDriveItemCollection> GetDrivePublicFolder()
        {
            return await GetData<OneDriveItemCollection>("drive/special/public");
        }

        /// <summary>
        /// Searches for items on OneDrive with the provided query
        /// </summary>
        /// <param name="query">Search query to use</param>
        /// <returns>All OneDrive items resulting from the search</returns>
        public virtual async Task<IList<OneDriveItem>> Search(string query)
        {
            return await SearchInternal($"drive/root/search(q='{query}')");
        }

        /// <summary>
        /// Searches for items on OneDrive in the provided path with the provided query
        /// </summary>
        /// <param name="query">Search query to use</param>
        /// <param name="path">OneDrive path where to search in</param>
        /// <returns>All OneDrive items resulting from the search</returns>
        public virtual async Task<IList<OneDriveItem>> Search(string query, string path)
        {
            return await SearchInternal($"drive/root:/{path}:/search(q='{query}')");
        }

        /// <summary>
        /// Searches for items on OneDrive in the provided path with the provided query
        /// </summary>
        /// <param name="query">Search query to use</param>
        /// <param name="oneDriveItem">OneDrive item representing a folder to search in</param>
        /// <returns>All OneDrive items resulting from the search</returns>
        public virtual async Task<IList<OneDriveItem>> Search(string query, OneDriveItem oneDriveItem)
        {
            return await SearchInternal($"drive/items/{oneDriveItem.Id}/search(q='{query}')");
        }

        /// <summary>
        /// Deletes the provided OneDriveItem from OneDrive
        /// </summary>
        /// <param name="oneDriveItem">The OneDriveItem reference to delete from OneDrive</param>
        public virtual async Task<bool> Delete(OneDriveItem oneDriveItem)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveItem.RemoteItem != null)
            {
                // Item to delete is shared from another drive
                completeUrl = string.Concat("drives/", oneDriveItem.RemoteItem.ParentReference.DriveId, "/items/", oneDriveItem.RemoteItem.Id);
            }
            else if (oneDriveItem.ParentReference != null && !string.IsNullOrEmpty(oneDriveItem.ParentReference.DriveId))
            {
                // Item to delete is shared from another drive
                completeUrl = string.Concat("drives/", oneDriveItem.ParentReference.DriveId, "/items/", oneDriveItem.Id);
            }
            else
            {
                // Item to delete resides on the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveItem.Id);
            }

            return await DeleteItemInternal(completeUrl);
        }

        /// <summary>
        /// Deletes the provided OneDriveItem from OneDrive
        /// </summary>
        /// <param name="oneDriveItemPath">The path to the OneDrive item to delete from OneDrive</param>
        public virtual async Task<bool> Delete(string oneDriveItemPath)
        {
            return await DeleteItemInternal(string.Concat("drive/root:/", oneDriveItemPath));
        }

        /// <summary>
        /// Copies the provided OneDriveItem to the provided destination on OneDrive
        /// </summary>
        /// <param name="oneDriveSourceItemPath">The path to the OneDrive Item to be copied</param>
        /// <param name="oneDriveDestinationItemPath">The path to the OneDrive parent item to copy the item into</param>
        /// <param name="destinationName">The name of the item at the destination where it will be copied to. Omit to use the source name.</param>
        /// <returns>True if successful, false if failed</returns>
        public virtual async Task<bool> Copy(string oneDriveSourceItemPath, string oneDriveDestinationItemPath, string destinationName = null)
        {
            var oneDriveSourceItem = await GetItem(oneDriveSourceItemPath);
            var oneDriveDestinationItem = await GetItem(oneDriveDestinationItemPath);
            return await Copy(oneDriveSourceItem, oneDriveDestinationItem, destinationName);
        }

        /// <summary>
        /// Copies the provided OneDriveItem to the provided destination on OneDrive
        /// </summary>
        /// <param name="oneDriveSourceItem">The path to the OneDrive Item to be copied</param>
        /// <param name="oneDriveDestinationItem">The path tothe OneDrive parent item to copy the item into</param>
        /// <param name="destinationName">The name of the item at the destination where it will be copied to. Omit to use the source name.</param>
        /// <returns>True if successful, false if failed</returns>
        public virtual async Task<bool> Copy(OneDriveItem oneDriveSourceItem, OneDriveItem oneDriveDestinationItem, string destinationName = null)
        {
            return await CopyItemInternal(oneDriveSourceItem, oneDriveDestinationItem, destinationName);
        }

        /// <summary>
        /// Moves the provided OneDriveItem to the provided destination on OneDrive
        /// </summary>
        /// <param name="oneDriveSourceItemPath">The path to the OneDrive Item to be moved</param>
        /// <param name="oneDriveDestinationItemPath">The path to the OneDrive parent item to move the item into</param>
        /// <returns>True if successful, false if failed</returns>
        public virtual async Task<bool> Move(string oneDriveSourceItemPath, string oneDriveDestinationItemPath)
        {
            var oneDriveSourceItem = await GetItem(oneDriveSourceItemPath);
            var oneDriveDestinationItem = await GetItem(oneDriveDestinationItemPath);
            return await Move(oneDriveSourceItem, oneDriveDestinationItem);
        }

        /// <summary>
        /// Moves the provided OneDriveItem to the provided destination on OneDrive
        /// </summary>
        /// <param name="oneDriveSourceItem">The OneDrive Item to be moved</param>
        /// <param name="oneDriveDestinationItem">The OneDrive parent item to move the item into</param>
        /// <returns>True if successful, false if failed</returns>
        public virtual async Task<bool> Move(OneDriveItem oneDriveSourceItem, OneDriveItem oneDriveDestinationItem)
        {
            return await MoveItemInternal(oneDriveSourceItem, oneDriveDestinationItem);
        }

        /// <summary>
        /// Renames the provided OneDriveItem to the provided name
        /// </summary>
        /// <param name="oneDriveItemPath">The path to the OneDrive Item to be renamed</param>
        /// <param name="name">The new name to assign to the OneDrive item</param>
        /// <returns>True if successful, false if failed</returns>
        public virtual async Task<bool> Rename(string oneDriveItemPath, string name)
        {
            var oneDriveItem = await GetItem(oneDriveItemPath);
            return await Rename(oneDriveItem, name);
        }

        /// <summary>
        /// Renames the provided OneDriveItem to the provided name
        /// </summary>
        /// <param name="oneDriveItemPath">The OneDrive Item to be renamed</param>
        /// <param name="name">The new name to assign to the OneDrive item</param>
        /// <returns>True if successful, false if failed</returns>
        public virtual async Task<bool> Rename(OneDriveItem oneDriveItemPath, string name)
        {
            return await RenameItemInternal(oneDriveItemPath, name);
        }

        /// <summary>
        /// Downloads the contents of the item on OneDrive at the provided path to the folder provided keeping the original filename
        /// </summary>
        /// <param name="path">Path to an item on OneDrive to download its contents of</param>
        /// <param name="saveTo">Path where to save the file to. The same filename as used on OneDrive will be used to save the file under.</param>
        /// <returns>True if download was successful, false if it failed</returns>
        public virtual async Task<bool> DownloadItem(string path, string saveTo)
        {
            var oneDriveItem = await GetItem(path);
            return await DownloadItem(oneDriveItem, saveTo);
        }

        /// <summary>
        /// Downloads the contents of the provided OneDriveItem to the folder provided keeping the original filename
        /// </summary>
        /// <param name="oneDriveItem">OneDriveItem to download its contents of</param>
        /// <param name="saveTo">Path where to save the file to. The same filename as used on OneDrive will be used to save the file under.</param>
        /// <returns>True if download was successful, false if it failed</returns>
        public virtual async Task<bool> DownloadItem(OneDriveItem oneDriveItem, string saveTo)
        {
            return await DownloadItemAndSaveAs(oneDriveItem, Path.Combine(saveTo, oneDriveItem.Name));
        }

        /// <summary>
        /// Downloads the contents of the item on OneDrive at the provided path to the full path provided
        /// </summary>
        /// <param name="path">Path to an item on OneDrive to download its contents of</param>
        /// <param name="saveAs">Full path including filename where to store the downloaded file</param>
        /// <returns>True if download was successful, false if it failed</returns>
        public virtual async Task<bool> DownloadItemAndSaveAs(string path, string saveAs)
        {
            var oneDriveItem = await GetItem(path);
            return await DownloadItemAndSaveAs(oneDriveItem, saveAs);
        }

        /// <summary>
        /// Downloads the contents of the provided OneDriveItem to the full path provided
        /// </summary>
        /// <param name="oneDriveItem">OneDriveItem to download its contents of</param>
        /// <param name="saveAs">Full path including filename where to store the downloaded file</param>
        /// <returns>True if download was successful, false if it failed</returns>
        public virtual async Task<bool> DownloadItemAndSaveAs(OneDriveItem oneDriveItem, string saveAs)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveItem.RemoteItem != null)
            {
                // Item to download is shared from another drive
                completeUrl = string.Concat("drives/", oneDriveItem.RemoteItem.ParentReference.DriveId, "/items/", oneDriveItem.RemoteItem.Id, "/content");
            }
            else if (oneDriveItem.ParentReference != null && !string.IsNullOrEmpty(oneDriveItem.ParentReference.DriveId))
            {
                // Item to download is shared from another drive
                completeUrl = string.Concat("drives/", oneDriveItem.ParentReference.DriveId, "/items/", oneDriveItem.Id, "/content");
            }
            else
            {
                // Item to download resides on the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveItem.Id, "/content");
            }

            completeUrl = ConstructCompleteUrl(completeUrl);

            using (var stream = await DownloadItemInternal(oneDriveItem, completeUrl))
            {
                using (var outputStream = new FileStream(saveAs, FileMode.Create))
                {
                    await stream.CopyToAsync(outputStream);
                }
            }
            return true;
        }

        /// <summary>
        /// Downloads the contents of the item on OneDrive at the provided path and returns the contents as a stream
        /// </summary>
        /// <param name="path">Path to an item on OneDrive to download its contents of</param>
        /// <returns>Stream with the contents of the item on OneDrive</returns>
        public virtual async Task<Stream> DownloadItem(string path)
        {
            var oneDriveItem = await GetItem(path);
            return await DownloadItem(oneDriveItem);
        }

        /// <summary>
        /// Downloads the contents of the provided OneDriveItem and returns the contents as a stream
        /// </summary>
        /// <param name="oneDriveItem">OneDriveItem to download its contents of</param>
        /// <returns>Stream with the contents of the item on OneDrive</returns>
        public virtual async Task<Stream> DownloadItem(OneDriveItem oneDriveItem)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveItem.RemoteItem != null)
            {
                // Item to download is shared from another drive
                completeUrl = string.Concat("drives/", oneDriveItem.RemoteItem.ParentReference.DriveId, "/items/", oneDriveItem.RemoteItem.Id, "/content");
            }
            else if (oneDriveItem.ParentReference != null && !string.IsNullOrEmpty(oneDriveItem.ParentReference.DriveId))
            {
                // Item to download is shared from another drive
                completeUrl = string.Concat("drives/", oneDriveItem.ParentReference.DriveId, "/items/", oneDriveItem.Id, "/content");
            }
            else
            {
                // Item to download resides on the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveItem.Id, "/content");
            }

            completeUrl = ConstructCompleteUrl(completeUrl);

            return await DownloadItemInternal(oneDriveItem, completeUrl);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive updating the original file
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="oneDriveItem">OneDriveItem the item of which its contents should be updated</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public virtual async Task<OneDriveItem> UpdateFile(string filePath, OneDriveItem oneDriveItem)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Provided file could not be found", nameof(filePath));
            }

            // Get a reference to the file to upload
            var fileToUpload = new FileInfo(filePath);

            // Verify if the filename does not contain any for OneDrive illegal characters
            if (!ValidFilename(fileToUpload.Name))
            {
                throw new ArgumentException("Provided file contains illegal characters in its filename", nameof(filePath));
            }

            // Verify which upload method should be used
            if (fileToUpload.Length <= MaximumBasicFileUploadSizeInBytes)
            {
                // Use the basic upload method                
                return await UpdateFileViaSimpleUpload(fileToUpload, oneDriveItem);
            }

            // Use the resumable upload method
            return await UpdateFileViaResumableUpload(fileToUpload, oneDriveItem, null);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive keeping the original filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="parentFolder">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public virtual async Task<OneDriveItem> UploadFile(string filePath, OneDriveItem parentFolder)
        {
            return await UploadFileAs(filePath, null, parentFolder);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="oneDriveFolder">Path to a OneDrive folder where to upload the file to</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public virtual async Task<OneDriveItem> UploadFile(string filePath, string oneDriveFolder)
        {
            var oneDriveItem = await GetItem(oneDriveFolder);
            return await UploadFile(filePath, oneDriveItem);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="oneDriveFolder">Path to a OneDrive folder where to upload the file to</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public virtual async Task<OneDriveItem> UploadFileAs(string filePath, string fileName, string oneDriveFolder)
        {
            var oneDriveItem = await GetItem(oneDriveFolder);
            return await UploadFileAs(filePath, fileName, oneDriveItem);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="oneDriveFolder">Path to a OneDrive folder where to upload the file to</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public virtual async Task<OneDriveItem> UploadFileAs(Stream fileStream, string fileName, string oneDriveFolder)
        {
            var oneDriveItem = await GetItem(oneDriveFolder);
            return await UploadFileAs(fileStream, fileName, oneDriveItem);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="parentFolder">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public virtual async Task<OneDriveItem> UploadFileAs(string filePath, string fileName, OneDriveItem parentFolder)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Provided file could not be found", nameof(filePath));
            }

            // Get a reference to the file to upload
            var fileToUpload = new FileInfo(filePath);

            // If no filename has been provided, use the same filename as the original file has
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = fileToUpload.Name;
            }

            // Verify if the filename does not contain any for OneDrive illegal characters
            if (!ValidFilename(fileName))
            {
                throw new ArgumentException("Provided file contains illegal characters in its filename", nameof(filePath));
            }

            // Verify which upload method should be used
            if (fileToUpload.Length <= MaximumBasicFileUploadSizeInBytes)
            {
                // Use the basic upload method                
                return await UploadFileViaSimpleUpload(fileToUpload, fileName, parentFolder);
            }

            // Use the resumable upload method
            return await UploadFileViaResumableUpload(fileToUpload, fileName, parentFolder, null);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="parentFolder">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public virtual async Task<OneDriveItem> UploadFileAs(Stream fileStream, string fileName, OneDriveItem parentFolder)
        {
            if (fileStream == null || fileStream == Stream.Null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            if (parentFolder == null)
            {
                throw new ArgumentNullException(nameof(parentFolder));
            }

            // Verify if the filename does not contain any for OneDrive illegal characters
            if (!ValidFilename(fileName))
            {
                throw new ArgumentException("Provided file contains illegal characters in its filename", nameof(fileName));
            }

            // Verify which upload method should be used
            if (fileStream.Length <= MaximumBasicFileUploadSizeInBytes)
            {
                // Use the basic upload method                
                return await UploadFileViaSimpleUpload(fileStream, fileName, parentFolder);
            }

            // Use the resumable upload method
            return await UploadFileViaResumableUpload(fileStream, fileName, parentFolder, null);
        }

        /// <summary>
        /// Creates a new folder under the provided parent OneDrive item with the provided name
        /// </summary>
        /// <param name="parentPath">The path to the OneDrive folder under which the folder should be created</param>
        /// <param name="folderName">Name to assign to the new folder</param>
        /// <returns>OneDriveItem entity representing the newly created folder or NULL if the operation fails</returns>
        public virtual async Task<OneDriveItem> CreateFolder(string parentPath, string folderName)
        {
            return await CreateFolderInternal(!string.IsNullOrEmpty(parentPath) ? string.Concat("drive/root:/", parentPath, ":/children") : "drive/root/children", folderName);
        }

        /// <summary>
        /// Creates a new folder under the provided parent OneDrive item with the provided name
        /// </summary>
        /// <param name="parentItem">The OneDrive item under which the folder should be created</param>
        /// <param name="folderName">Name to assign to the new folder</param>
        /// <returns>OneDriveItem entity representing the newly created folder or NULL if the operation fails</returns>
        public virtual async Task<OneDriveItem> CreateFolder(OneDriveItem parentItem, string folderName)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (parentItem.RemoteItem != null)
            {
                // Item where to create a new folder is shared from another drive
                completeUrl = string.Concat("drives/", parentItem.RemoteItem.ParentReference.DriveId, "/items/", parentItem.RemoteItem.Id, "/children");
            }

            else
            {
                // Item where to create a new folder resides on the current user its drive
                completeUrl = string.Concat("drive/items/", parentItem.Id, "/children");
            }

            return await CreateFolderInternal(completeUrl, folderName);
        }

        /// <summary>
        /// Returns all the items that have been shared by others with the current user
        /// </summary>
        /// <returns>Collection with items that have been shared by others with the current user</returns>
        public virtual async Task<OneDriveItemCollection> GetSharedWithMe()
        {
            var oneDriveItems = await GetData<OneDriveItemCollection>("drive/sharedWithMe");
            return oneDriveItems;
        }

        /// <summary>
        /// Retrieves the first batch of children under the OneDrive folder with the provided id from the OneDrive with the provided id
        /// </summary>
        /// <param name="folderId">Id of the folder to retrieve the items of</param>
        /// <param name="driveId">Id of the drive on which the folder resides</param>
        /// <returns>OneDriveItemCollection containing the first batch of items in the folder</returns>
        public async Task<OneDriveItemCollection> GetChildrenFromDriveByFolderId(string driveId, string folderId)
        {
            var oneDriveItems = await GetData<OneDriveItemCollection>($"drives/{driveId}/items/{folderId}/children");
            return oneDriveItems;
        }

        /// <summary>
        /// Retrieves the first batch of children under the OneDrive folder with the provided id from the OneDrive with the provided id
        /// </summary>
        /// <param name="folder">Folder to retrieve the items of</param>
        /// <param name="drive">Drive on which the folder resides</param>
        /// <returns>OneDriveItemCollection containing the first batch of items in the folder</returns>
        public async Task<OneDriveItemCollection> GetChildrenFromDriveByFolderId(OneDriveDrive drive, OneDriveItem folder)
        {
            var oneDriveItems = await GetChildrenFromDriveByFolderId(drive.Id, folder.Id);
            return oneDriveItems;
        }

        /// <summary>
        /// Retrieves all the of children under the OneDrive folder with the provided id from the OneDrive with the provided id
        /// </summary>
        /// <param name="folderId">Id of the folder to retrieve the items of</param>
        /// <param name="driveId">Id of the drive on which the folder resides</param>
        /// <returns>OneDriveItem array containing all items in the requested folder</returns>
        public virtual async Task<OneDriveItem[]> GetAllChildrenFromDriveByFolderId(string driveId, string folderId)
        {
            return await GetAllChildrenInternal($"drives/{driveId}/items/{folderId}/children");
        }

        /// <summary>
        /// Retrieves all the of children under the OneDrive folder with the provided id from the OneDrive with the provided id
        /// </summary>
        /// <param name="folder">Folder to retrieve the items of</param>
        /// <param name="drive">Drive on which the folder resides</param>
        /// <returns>OneDriveItem array containing all items in the requested folder</returns>
        public virtual async Task<OneDriveItem[]> GetAllChildrenFromDriveByFolderId(OneDriveDrive drive, OneDriveItem folder)
        {
            return await GetAllChildrenFromDriveByFolderId(drive.Id, folder.Id);
        }

        #endregion

        #region Public Methods - Graph API Only

        #region Sharing

        /// <summary>
        /// Shares a OneDrive item by creating an anonymous link to the item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        public virtual async Task<OneDrivePermission> ShareItem(string itemPath, OneDriveLinkType linkType)
        {
            return await ShareItemInternal(string.Concat("drive/root:/", itemPath, ":/createLink"), linkType);
        }

        /// <summary>
        /// Shares a OneDrive item by creating an anonymous link to the item
        /// </summary>
        /// <param name="item">The OneDrive item to share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        public virtual async Task<OneDrivePermission> ShareItem(OneDriveItem item, OneDriveLinkType linkType)
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

        #endregion

        #region Adding permissions

        /// <summary>
        /// Adds permissions to a OneDrive item
        /// </summary>        
        /// <param name="item">The OneDrive item to add the permission to</param>
        /// <param name="permissionRequest">Details of the request for permission</param>
        /// <returns>Collection with OneDrivePermissionResponse objects representing the granted permissions</returns>
        public async Task<OneDriveCollectionResponse<OneDrivePermissionResponse>> AddPermission(OneDriveItem item, OneDrivePermissionRequest permissionRequest)
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/items/", item.Id, "/invite");

            var result = await SendMessageReturnOneDriveItem<OneDriveCollectionResponse<OneDrivePermissionResponse>>(permissionRequest, HttpMethod.Post, completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Adds permissions to a OneDrive item
        /// </summary>        
        /// <param name="itemPath">The path to the OneDrive item to add the permission to</param>
        /// <param name="permissionRequest">Details of the request for permission</param>
        /// <returns>Collection with OneDrivePermissionResponse objects representing the granted permissions</returns>
        public async Task<OneDriveCollectionResponse<OneDrivePermissionResponse>> AddPermission(string itemPath, OneDrivePermissionRequest permissionRequest)
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/root:/", itemPath, ":/invite");

            var result = await SendMessageReturnOneDriveItem<OneDriveCollectionResponse<OneDrivePermissionResponse>>(permissionRequest, HttpMethod.Post, completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Adds permissions to a OneDrive item
        /// </summary>
        /// <param name="item">The OneDrive item to add the permission to</param>
        /// <param name="requireSignin">Boolean to indicate if the user has to sign in before being able to access the OneDrive item</param>
        /// <param name="linkType">Indicates what type of access should be assigned to the invitees</param>
        /// <param name="emailAddresses">Array with e-mail addresses to receive access to the OneDrive item</param>
        /// <param name="sendInvitation">Send an e-mail to the invitees to inform them about having received permissions to the OneDrive item</param>
        /// <param name="sharingMessage">Custom message to add to the e-mail sent out to the invitees</param>
        /// <returns>Collection with OneDrivePermissionResponse objects representing the granted permissions</returns>
        public async Task<OneDriveCollectionResponse<OneDrivePermissionResponse>> AddPermission(OneDriveItem item, bool requireSignin, bool sendInvitation, OneDriveLinkType linkType, string sharingMessage, string[] emailAddresses)
        {
            var permissionRequest = new OneDrivePermissionRequest
            {
                Message = sharingMessage,
                RequireSignin = requireSignin,
                SendInvitation = sendInvitation,
                Roles = linkType == OneDriveLinkType.View ? new[] { "read" } : new[] { "write" }
            };

            var recipients = new List<OneDriveDriveRecipient>();
            foreach (var emailAddress in emailAddresses)
            {
                recipients.Add(new OneDriveDriveRecipient
                {
                    Email = emailAddress
                });
            }
            permissionRequest.Recipients = recipients.ToArray();

            return await AddPermission(item, permissionRequest);
        }

        /// <summary>
        /// Adds permissions to a OneDrive item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to add the permission to</param>
        /// <param name="requireSignin">Boolean to indicate if the user has to sign in before being able to access the OneDrive item</param>
        /// <param name="linkType">Indicates what type of access should be assigned to the invitees</param>
        /// <param name="emailAddresses">Array with e-mail addresses to receive access to the OneDrive item</param>
        /// <param name="sendInvitation">Send an e-mail to the invitees to inform them about having received permissions to the OneDrive item</param>
        /// <param name="sharingMessage">Custom message to add to the e-mail sent out to the invitees</param>
        /// <returns>Collection with OneDrivePermissionResponse objects representing the granted permissions</returns>
        public async Task<OneDriveCollectionResponse<OneDrivePermissionResponse>> AddPermission(string itemPath, bool requireSignin, bool sendInvitation, OneDriveLinkType linkType, string sharingMessage, string[] emailAddresses)
        {
            var permissionRequest = new OneDrivePermissionRequest
            {
                Message = sharingMessage,
                RequireSignin = requireSignin,
                SendInvitation = sendInvitation,
                Roles = linkType == OneDriveLinkType.View ? new[] { "read" } : new[] { "write" }
            };

            var recipients = new List<OneDriveDriveRecipient>();
            foreach (var emailAddress in emailAddresses)
            {
                recipients.Add(new OneDriveDriveRecipient
                {
                    Email = emailAddress
                });
            }
            permissionRequest.Recipients = recipients.ToArray();

            return await AddPermission(itemPath, permissionRequest);
        }

        #endregion

        #region Updating permissions

        /// <summary>
        /// Changes permissions on a OneDrive item
        /// </summary>
        /// <param name="item">The OneDrive item to change the permission of</param>
        /// <param name="permissionType">Permission to set on the OneDrive item</param>
        /// <param name="permissionId">ID of the permission object applied to the OneDrive item which needs its permissions changed</param>
        /// <returns>OneDrivePermissionResponse object representing the granted permission</returns>
        public async Task<OneDrivePermissionResponse> ChangePermission(OneDriveItem item, string permissionId, OneDriveLinkType permissionType)
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/items/", item.Id, "/permissions/", permissionId);

            var result = await SendMessageReturnOneDriveItem<OneDrivePermissionResponse>("{ \"roles\": [ \"" + (permissionType == OneDriveLinkType.Edit ? "write" : "read") + "\" ] }", new HttpMethod("PATCH"), completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Changes permissions on a OneDrive item
        /// </summary>
        /// <param name="item">The OneDrive item to change the permission of</param>
        /// <param name="permissionType">Permission to set on the OneDrive item</param>
        /// <param name="permission">Permission object applied to the OneDrive item which needs its permissions changed</param>
        /// <returns>OneDrivePermissionResponse object representing the granted permission</returns>
        public async Task<OneDrivePermissionResponse> ChangePermission(OneDriveItem item, OneDrivePermission permission, OneDriveLinkType permissionType)
        {
            return await ChangePermission(item, permission.Id, permissionType);
        }

        /// <summary>
        /// Changes permissions on a OneDrive item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to change the permission of</param>
        /// <param name="permissionType">Permission to set on the OneDrive item</param>
        /// <param name="permissionId">ID of the permission object applied to the OneDrive item which needs its permissions changed</param>
        /// <returns>OneDrivePermissionResponse object representing the granted permission</returns>
        public async Task<OneDrivePermissionResponse> ChangePermission(string itemPath, string permissionId, OneDriveLinkType permissionType)
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/root:/", itemPath, ":/permissions/", permissionId);

            var result = await SendMessageReturnOneDriveItem<OneDrivePermissionResponse>("{ \"roles\": [ \"" + (permissionType == OneDriveLinkType.Edit ? "write" : "read") + "\" ] }", new HttpMethod("PATCH"), completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Changes permissions on a OneDrive item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to change the permission of</param>
        /// <param name="permissionType">Permission to set on the OneDrive item</param>
        /// <param name="permission">Permission object applied to the OneDrive item which needs its permissions changed</param>
        /// <returns>OneDrivePermissionResponse object representing the granted permission</returns>
        public async Task<OneDrivePermissionResponse> ChangePermission(string itemPath, OneDrivePermission permission, OneDriveLinkType permissionType)
        {
            return await ChangePermission(itemPath, permission.Id, permissionType);
        }

        #endregion

        #region Removing permissions

        /// <summary>
        /// Removes the permission from a OneDrive item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to remove the permission from</param>
        /// <param name="permissionId">Unique permission identifier as received when addign the permission to the item</param>
        /// <returns>Boolean indicating if the operation was successful (true) or failed (false)</returns>
        public async Task<bool> RemovePermission(string itemPath, string permissionId)
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/root:/", itemPath, ":/permissions/", permissionId);

            var result = await SendMessageReturnBool(null, HttpMethod.Delete, completeUrl, HttpStatusCode.NoContent);
            return result;
        }

        /// <summary>
        /// Removes the permission from a OneDrive item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to remove the permission from</param>
        /// <param name="permission">Permission object as received when creating a permission on the item</param>
        /// <returns>Boolean indicating if the operation was successful (true) or failed (false)</returns>
        public async Task<bool> RemovePermission(string itemPath, OneDrivePermission permission)
        {
            return await RemovePermission(itemPath, permission.Id);
        }

        /// <summary>
        /// Removes the permission from a OneDrive item
        /// </summary>
        /// <param name="item">The OneDrive item to add a permission to</param>
        /// <param name="permissionId">Unique sharing permission identifier as received when adding the permission to the item</param>
        /// <returns>Boolean indicating if the operation was successful (true) or failed (false)</returns>
        public async Task<bool> RemovePermission(OneDriveItem item, string permissionId)
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/items/", item.Id, "/permissions/", permissionId);

            var result = await SendMessageReturnBool(null, HttpMethod.Delete, completeUrl, HttpStatusCode.NoContent);
            return result;            
        }

        /// <summary>
        /// Removes the permission from a OneDrive item
        /// </summary>
        /// <param name="item">The OneDrive item to add a permission to</param>
        /// <param name="permission">Permission object as received when creating a permission on the item</param>
        /// <returns>Boolean indicating if the operation was successful (true) or failed (false)</returns>
        public async Task<bool> RemovePermission(OneDriveItem item, OneDrivePermission permission)
        {
            return await RemovePermission(item, permission.Id);
        }

        #endregion

        #region Listing permissions

        /// <summary>
        /// Lists all permissions on a OneDrive item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to retrieve the permissions of</param>
        /// <returns>Collection with OneDrivePermission objects which indicate the permissions on the item</returns>
        public async Task<OneDriveCollectionResponse<OneDrivePermission>> ListPermissions(string itemPath)
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/root:/", itemPath, ":/permissions");

            var result = await SendMessageReturnOneDriveItem<OneDriveCollectionResponse<OneDrivePermission>>(string.Empty, HttpMethod.Get, completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Lists all permissions on a OneDrive item
        /// </summary>
        /// <param name="item">The OneDrive item to retrieve the permissions of</param>
        /// <returns>Collection with OneDrivePermission objects which indicate the permissions on the item</returns>
        public async Task<OneDriveCollectionResponse<OneDrivePermission>> ListPermissions(OneDriveItem item)
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/items/", item.Id, "/permissions");

            var result = await SendMessageReturnOneDriveItem<OneDriveCollectionResponse<OneDrivePermission>>(item, HttpMethod.Get, completeUrl, HttpStatusCode.OK);
            return result;
        }

        #endregion

        #region AppFolder commands

        #region Retrieving data from the AppFolder

        /// <summary>
        /// Gets the AppFolder root its metadata
        /// </summary>
        /// <returns>OneDriveItem object with the information about the current App Registration its AppFolder</returns>
        public async Task<OneDriveItem> GetAppFolderMetadata()
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/special/approot");

            var result = await SendMessageReturnOneDriveItem<OneDriveItem>(string.Empty, HttpMethod.Get, completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Gets the first batch of the files in the AppFolder
        /// </summary>
        /// <returns>Collection with OneDriveItem objects one for each file in the the current App Registration its AppFolder</returns>
        public async Task<OneDriveItemCollection> GetAppFolderChildren()
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/special/approot/children");

            var result = await SendMessageReturnOneDriveItem<OneDriveItemCollection>(string.Empty, HttpMethod.Get, completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Gets all files in the AppFolder
        /// </summary>
        /// <returns>Collection with OneDriveItem objects one for each file in the the current App Registration its AppFolder</returns>
        public async Task<OneDriveItem[]> GetAllAppFolderChildren()
        {
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/special/approot/children");

            var result = await GetAllChildrenInternal(completeUrl);
            return result;
        }

        /// <summary>
        /// Creates a folder in the AppFolder
        /// </summary>
        /// <param name="folderName">Name of the folder to create within the AppFolder</param>
        /// <returns>OneDriveItem object representing the newly created folder inside the AppFolder</returns>
        public async Task<OneDriveItem> CreateAppFolderFolder(string folderName)
        {
            var result = await CreateFolderInternal("drive/special/approot/children", folderName);
            return result;
        }

        /// <summary>
        /// Gets the folder with the provided name from the AppFolder or creates it if it doesn't exist yet
        /// </summary>
        /// <param name="folderName">Name of the folder to retrieve or create within the AppFolder</param>
        /// <returns>OneDriveItem object representing the requested folder inside the AppFolder</returns>
        public async Task<OneDriveItem> GetAppFolderFolderOrCreate(string folderName)
        {
            // Try to get the folder
            var folder = await GetData<OneDriveItem>(string.Concat("drive/special/approot/children/", folderName));

            if (folder != null)
            {
                // Folder found, return it
                return folder;
            }

            // Folder not found, create it
            return await CreateAppFolderFolder(folderName);
        }

        #endregion

        #region Uploading files to AppFolder

        /// <summary>
        /// Uploads the provided file to the AppFolder on OneDrive keeping the original filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileToAppFolder(string filePath)
        {
            return await UploadFileToAppFolder(filePath, NameConflictBehavior.Replace);
        }

        /// <summary>
        /// Uploads the provided file to the AppFolder on OneDrive keeping the original filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileToAppFolder(string filePath, NameConflictBehavior nameConflictBehavior)
        {
            return await UploadFileToAppFolderAs(filePath, null, nameConflictBehavior);
        }

        /// <summary>
        /// Uploads the provided file to the AppFolder on OneDrive using the provided filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileToAppFolderAs(string filePath, string fileName)
        {
            return await UploadFileToAppFolderAs(filePath, fileName, NameConflictBehavior.Replace);
        }

        /// <summary>
        /// Uploads the provided file to the AppFolder on OneDrive using the provided filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileToAppFolderAs(string filePath, string fileName, NameConflictBehavior nameConflictBehavior)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Provided file could not be found", nameof(filePath));
            }

            // Get a reference to the file to upload
            var fileToUpload = new FileInfo(filePath);

            // If no filename has been provided, use the same filename as the original file has
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = fileToUpload.Name;
            }

            // Verify if the filename does not contain any for OneDrive illegal characters
            if (!ValidFilename(fileName))
            {
                throw new ArgumentException("Provided file contains illegal characters in its filename", nameof(filePath));
            }

            // Use the resumable upload method
            return await UploadFileToAppFolderViaResumableUpload(fileToUpload, fileName, null);
        }

        /// <summary>
        /// Uploads the provided file to the AppFolder on OneDrive using the provided filename
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public virtual async Task<OneDriveItem> UploadFileToAppFolderAs(Stream fileStream, string fileName)
        {
            if (fileStream == null || fileStream == Stream.Null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            // Verify if the filename does not contain any for OneDrive illegal characters
            if (!ValidFilename(fileName))
            {
                throw new ArgumentException("Provided file contains illegal characters in its filename", nameof(fileName));
            }

            // Verify which upload method should be used
            if (fileStream.Length <= MaximumBasicFileUploadSizeInBytes)
            {
                // Use the basic upload method                
                return await UploadFileToAppFolderViaSimpleUpload(fileStream, fileName);
            }

            // Use the resumable upload method
            return await UploadFileToAppFolderViaResumableUpload(fileStream, fileName, null);
        }

        /// <summary>
        /// Performs a file upload to the AppFolder on OneDrive using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="file">File reference to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UploadFileToAppFolderViaSimpleUpload(FileInfo file, string fileName)
        {
            // Read the file to upload
            using (var fileStream = file.OpenRead())
            {
                return await UploadFileToAppFolderViaSimpleUpload(fileStream, fileName);
            }
        }

        /// <summary>
        /// Performs a file upload to the AppFolder on OneDrive using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UploadFileToAppFolderViaSimpleUpload(Stream fileStream, string fileName)
        {
            // Construct the complete URL to call
            var oneDriveUrl = string.Concat(OneDriveApiBaseUrl, "drive/special/approot:/", fileName, ":/content");

            return await UploadFileViaSimpleUploadInternal(fileStream, oneDriveUrl);
        }

        /// <summary>
        /// Initiates a resumable upload session to the AppFolder on OneDrive. It doesn't perform the actual upload yet.
        /// </summary>
        /// <param name="fileName">Filename to store the uploaded content under</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveUploadSession instance containing the details where to upload the content to</returns>
        protected async Task<OneDriveUploadSession> CreateResumableUploadSessionForAppFolder(string fileName, NameConflictBehavior nameConflictBehavior = NameConflictBehavior.Replace)
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBaseUrl, "drive/special/approot:/", fileName, ":/createUploadSession");
            return await CreateResumableUploadSessionInternal(completeUrl);
        }

        /// <summary>
        /// Uploads a file to the AppFolder on OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="file">FileInfo instance pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="fragmentSizeInBytes">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UploadFileToAppFolderViaResumableUpload(FileInfo file, string fileName, long? fragmentSizeInBytes)
        {
            return await UploadFileToAppFolderViaResumableUpload(file, fileName, fragmentSizeInBytes, NameConflictBehavior.Replace);
        }

        /// <summary>
        /// Uploads a file to the AppFolder on OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="file">FileInfo instance pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="fragmentSizeInBytes">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UploadFileToAppFolderViaResumableUpload(FileInfo file, string fileName, long? fragmentSizeInBytes, NameConflictBehavior nameConflictBehavior)
        {
            // Open the source file for reading
            using (var fileStream = file.OpenRead())
            {
                return await UploadFileToAppFolderViaResumableUpload(fileStream, fileName, fragmentSizeInBytes);
            }
        }

        /// <summary>
        /// Uploads a file to the AppFolder on OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="fileStream">Stream pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="fragmentSizeInByte">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections.</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UploadFileToAppFolderViaResumableUpload(Stream fileStream, string fileName, long? fragmentSizeInByte)
        {
            return await UploadFileToAppFolderViaResumableUpload(fileStream, fileName, fragmentSizeInByte, NameConflictBehavior.Replace);
        }

        /// <summary>
        /// Uploads a file to the AppFolder on OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="fileStream">Stream pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="fragmentSizeInByte">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections.</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UploadFileToAppFolderViaResumableUpload(Stream fileStream, string fileName, long? fragmentSizeInByte, NameConflictBehavior nameConflictBehavior)
        {
            var oneDriveUploadSession = await CreateResumableUploadSessionForAppFolder(fileName, nameConflictBehavior);
            return await UploadFileViaResumableUploadInternal(fileStream, oneDriveUploadSession, fragmentSizeInByte);
        }

        #endregion

        #endregion

        #endregion

        #region File Uploading

        /// <summary>
        /// Uploads the provided file to OneDrive keeping the original filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFile(string filePath, OneDriveItem oneDriveItem, NameConflictBehavior nameConflictBehavior)
        {
            return await UploadFileAs(filePath, null, oneDriveItem, nameConflictBehavior);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="oneDriveFolder">Path to a OneDrive folder where to upload the file to</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFile(string filePath, string oneDriveFolder, NameConflictBehavior nameConflictBehavior)
        {
            var oneDriveItem = await GetItem(oneDriveFolder);
            return await UploadFile(filePath, oneDriveItem, nameConflictBehavior);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="oneDriveFolder">Path to a OneDrive folder where to upload the file to</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileAs(Stream fileStream, string fileName, string oneDriveFolder, NameConflictBehavior nameConflictBehavior)
        {
            var oneDriveItem = await GetItem(oneDriveFolder);
            return await UploadFileAs(fileStream, fileName, oneDriveItem, nameConflictBehavior);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="oneDriveFolder">Path to a OneDrive folder where to upload the file to</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileAs(string filePath, string fileName, string oneDriveFolder, NameConflictBehavior nameConflictBehavior)
        {
            var oneDriveItem = await GetItem(oneDriveFolder);
            return await UploadFileAs(filePath, fileName, oneDriveItem, nameConflictBehavior);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileAs(string filePath, string fileName, OneDriveItem oneDriveItem, NameConflictBehavior nameConflictBehavior)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Provided file could not be found", nameof(filePath));
            }

            // Get a reference to the file to upload
            var fileToUpload = new FileInfo(filePath);

            // If no filename has been provided, use the same filename as the original file has
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = fileToUpload.Name;
            }

            // Verify if the filename does not contain any for OneDrive illegal characters
            if (!ValidFilename(fileName))
            {
                throw new ArgumentException("Provided file contains illegal characters in its filename", nameof(filePath));
            }

            // Use the resumable upload method since the simple upload method doesn't support naming conflict behavior
            return await UploadFileViaResumableUpload(fileToUpload, fileName, oneDriveItem, null, nameConflictBehavior);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileAs(Stream fileStream, string fileName, OneDriveItem oneDriveItem, NameConflictBehavior nameConflictBehavior)
        {
            if (fileStream == null || fileStream == Stream.Null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            if (oneDriveItem == null)
            {
                throw new ArgumentNullException(nameof(oneDriveItem));
            }

            // Verify if the filename does not contain any for OneDrive illegal characters
            if (!ValidFilename(fileName))
            {
                throw new ArgumentException("Provided file contains illegal characters in its filename", nameof(fileName));
            }

            // Use the resumable upload method since the simple upload method doesn't support naming conflict behavior
            return await UploadFileViaResumableUpload(fileStream, fileName, oneDriveItem, null, nameConflictBehavior);
        }

        /// <summary>
        /// Uploads a file to OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="fileStream">Stream pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="oneDriveItem">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <param name="fragmentSizeInBytes">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UploadFileViaResumableUpload(Stream fileStream, string fileName, OneDriveItem oneDriveItem, long? fragmentSizeInBytes, NameConflictBehavior nameConflictBehavior)
        {
            var oneDriveUploadSession = await CreateResumableUploadSession(fileName, oneDriveItem, nameConflictBehavior);
            return oneDriveUploadSession != null ? await UploadFileViaResumableUploadInternal(fileStream, oneDriveUploadSession, fragmentSizeInBytes) : null;
        }

        /// <summary>
        /// Uploads a file to OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="file">FileInfo instance pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="oneDriveItem">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <param name="fragmentSizeInBytes">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections. Provide NULL to use the default.</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UploadFileViaResumableUpload(FileInfo file, string fileName, OneDriveItem oneDriveItem, long? fragmentSizeInBytes, NameConflictBehavior nameConflictBehavior)
        {
            // Open the source file for reading
            using (var fileStream = file.OpenRead())
            {
                return await UploadFileViaResumableUpload(fileStream, fileName, oneDriveItem, fragmentSizeInBytes, nameConflictBehavior);
            }
        }

        /// <summary>
        /// Uploads a file to OneDrive using the resumable method. Better for large files or unstable network connections.
        /// </summary>
        /// <param name="filePath">Path to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="oneDriveItem">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UploadFileViaResumableUpload(string filePath, string fileName, OneDriveItem oneDriveItem, NameConflictBehavior nameConflictBehavior)
        {
            var file = new FileInfo(filePath);
            return await UploadFileViaResumableUpload(file, fileName, oneDriveItem, null, nameConflictBehavior);
        }

        /// <summary>
        /// Initiates a resumable upload session to OneDrive. It doesn't perform the actual upload yet.
        /// </summary>
        /// <param name="fileName">Filename to store the uploaded content under</param>
        /// <param name="oneDriveItem">OneDriveItem container in which the file should be uploaded</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveUploadSession instance containing the details where to upload the content to</returns>
        protected async Task<OneDriveUploadSession> CreateResumableUploadSession(string fileName, OneDriveItem oneDriveItem, NameConflictBehavior nameConflictBehavior)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveItem.RemoteItem != null)
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", oneDriveItem.RemoteItem.ParentReference.DriveId, "/items/", oneDriveItem.RemoteItem.Id, ":/", fileName, ":/createUploadSession");
            }
            else if (oneDriveItem.ParentReference != null && !string.IsNullOrEmpty(oneDriveItem.ParentReference.DriveId))
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", oneDriveItem.ParentReference.DriveId, "/items/", oneDriveItem.Id, ":/", fileName, ":/createUploadSession");
            }
            else if (!string.IsNullOrEmpty(oneDriveItem.WebUrl) && oneDriveItem.WebUrl.Contains("cid="))
            {
                // Item will be uploaded to another drive. Used by OneDrive Personal when using a shared item.
                completeUrl = string.Concat("drives/", oneDriveItem.WebUrl.Remove(0, oneDriveItem.WebUrl.IndexOf("cid=") + 4), "/items/", oneDriveItem.Id, ":/", fileName, ":/createUploadSession");
            }
            else
            {
                // Item will be uploaded to the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveItem.Id, ":/", fileName, ":/createUploadSession");
            }

            completeUrl = ConstructCompleteUrl(completeUrl);

            return await CreateResumableUploadSessionInternal(completeUrl, nameConflictBehavior);
        }

        /// <summary>
        /// Initiates a resumable upload session to OneDrive. It doesn't perform the actual upload yet.
        /// </summary>
        /// <param name="oneDriveUrl">Complete URL to call to create the resumable upload session</param>
        /// <param name="nameConflictBehavior">Defines how to deal with the scenario where a similarly named file already exists at the target location</param>
        /// <returns>OneDriveUploadSession instance containing the details where to upload the content to</returns>
        protected async Task<OneDriveUploadSession> CreateResumableUploadSessionInternal(string oneDriveUrl, NameConflictBehavior nameConflictBehavior = NameConflictBehavior.Replace)
        {
            // Construct the OneDriveUploadSessionItemContainer entity with the upload details
            // Add the conflictbehavior header to always overwrite the file if it already exists on OneDrive
            var uploadItemContainer = new OneDriveUploadSessionItemContainer
            {
                Item = new OneDriveUploadSessionItem
                {
                    FilenameConflictBehavior = nameConflictBehavior
                }
            };   

            // Call the Graph API webservice
            var result = await SendMessageReturnOneDriveItem<OneDriveUploadSession>(uploadItemContainer, HttpMethod.Post, oneDriveUrl, HttpStatusCode.OK);
            return result;
        }

        #endregion

        #region SharePoint Sites

        /// <summary>
        /// Gets the root SharePoint site
        /// </summary>
        /// <returns>SharePointSite instance containing the details of the requested site in SharePoint</returns>
        public virtual async Task<SharePointSite> GetSiteRoot()
        {
            return await GetGraphData<SharePointSite>("sites/root");
        }

        /// <summary>
        /// Gets a SharePoint site by its unique identifier
        /// </summary>
        /// <param name="siteId">Unique identifier of the SharePoint site to retrieve, i.e. tenant.sharepoint.com,42f21fb5-a809-41d6-a97c-64ea0935306f,5a153572-749b-45e8-bae3-4a5e108ffa85</param>
        /// <returns>SharePointSite instance containing the details of the requested site in SharePoint</returns>
        public virtual async Task<SharePointSite> GetSiteById(string siteId)
        {
            return await GetGraphData<SharePointSite>("sites/" + siteId);
        }

        /// <summary>
        /// Gets a SharePoint site by its hostname and path
        /// </summary>
        /// <param name="hostname">Full SharePoint Online domain to request the SharePoint site from, i.e. tenant.sharepoint.com</param>
        /// <param name="sitePath">SharePoint tenant relative URL of the site to retrieve, i.e. /sites/team1</param>
        /// <returns>SharePointSite instance containing the details of the requested site in SharePoint</returns>
        public virtual async Task<SharePointSite> GetSiteByPath(string hostname, string sitePath)
        {
            return await GetGraphData<SharePointSite>("sites/" + hostname + ":/" + sitePath);
        }

        /// <summary>
        /// Gets a SharePoint site belonging to a group
        /// </summary>
        /// <param name="groupId">Unique identifier of group to retrieve the associated SharePoint site for</param>
        /// <returns>SharePointSite instance containing the details of the requested site in SharePoint</returns>
        public virtual async Task<SharePointSite> GetSiteByGroupId(string groupId)
        {
            return await GetGraphData<SharePointSite>("groups/" + groupId + "/sites/root");
        }

        /// <summary>
        /// Retrieves data from the Graph API
        /// </summary>
        /// <typeparam name="T">Type of OneDrive entity to expect to be returned</typeparam>
        /// <param name="url">Url fragment after the Graph base Uri which indicated the type of information to return</param>
        /// <returns>OneDrive entity filled with the information retrieved from the Graph API</returns>
        protected virtual async Task<T> GetGraphData<T>(string url) where T : OneDriveItemBase
        {
            // Construct the complete URL to call
            var completeUrl = url.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) ? url : string.Concat(GraphApiBaseUrl, url);

            return await GetData<T>(completeUrl);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shares a OneDrive item
        /// </summary>
        /// <param name="oneDriveRequestUrl">The OneDrive request url which creates the share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <param name="scope">Scope defining who has access to the shared item (not supported with OneDrive Personal)</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        protected virtual async Task<OneDrivePermission> ShareItemInternal(string oneDriveRequestUrl, OneDriveLinkType linkType, OneDriveSharingScope? scope = null)
        {
            // Construct the complete URL to call
            var completeUrl = ConstructCompleteUrl(oneDriveRequestUrl);

            // Construct the OneDriveRequestShare entity with the sharing details
            var requestShare = new OneDriveRequestShare { SharingType = linkType, Scope = scope };

            // Call the OneDrive webservice
            var result = await SendMessageReturnOneDriveItem<OneDrivePermission>(requestShare, HttpMethod.Post, completeUrl);
            return result;
        }

        /// <summary>
        /// Creates a new folder under the provided parent OneDrive item with the provided name
        /// </summary>
        /// <param name="oneDriveRequestUrl">The OneDrive request url which creates a new folder</param>
        /// <param name="folderName">Name to assign to the new folder</param>
        /// <returns>OneDriveItem entity representing the newly created folder or NULL if the operation fails</returns>
        protected virtual async Task<OneDriveItem> CreateFolderInternal(string oneDriveRequestUrl, string folderName)
        {
            // Construct the complete URL to call
            var completeUrl = ConstructCompleteUrl(oneDriveRequestUrl);            

            // Construct the JSON to send in the POST message
            var newFolder = new OneDriveCreateFolder { Name = folderName, Folder = new object() };

            // Send the webservice request
            var oneDriveItem = await SendMessageReturnOneDriveItem<OneDriveItem>(newFolder, HttpMethod.Post, completeUrl, HttpStatusCode.Created);
            return oneDriveItem;
        }

        /// <summary>
        /// Searches OneDrive by calling the OneDrive API url as provided
        /// </summary>
        /// <param name="searchUrl">OneDrive API url representing the search to execute</param>
        /// <returns>List with OneDriveItem objects resulting from the search query</returns>
        protected async Task<IList<OneDriveItem>> SearchInternal(string searchUrl)
        {
            // Create a list to contain all the search results
            var allResults = new List<OneDriveItem>();

            // Set the URL to execute against the OneDrive API to execute the query
            var nextSearchUrl = searchUrl;

            // Loop through the results for as long as there are more search results to return
            do
            {
                // Execute the search query against the OneDrive API
                var results = await GetData<OneDriveItemCollection>(nextSearchUrl);

                // Add the retrieved results to the list
                allResults.AddRange(results.Collection);

                // Check if there are more search results
                if (results.NextLink == null)
                {
                    // No more search results
                    break;
                }

                // There are more search results. Use the link provided in the response to fetch the next results. Cut off the basic OneDrive API url.
                nextSearchUrl = results.NextLink.Remove(0, OneDriveApiBaseUrl.Length);

            } while (true);

            return allResults;
        }

        /// <summary>
        /// Downloads the contents of the provided OneDriveItem to the location provided
        /// </summary>
        /// <param name="item">OneDriveItem to download its contents of</param>
        /// <param name="completeUrl">Complete URL from where to download the file</param>
        /// <returns>Stream with the downloaded content</returns>
        protected virtual async Task<Stream> DownloadItemInternal(OneDriveItem item, string completeUrl)
        {
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Create an HTTPClient instance to communicate with the REST API of OneDrive
            var client = CreateHttpClient(accessToken.AccessToken);

            // Send the request to the OneDrive API
            var response = await client.GetAsync(completeUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            // Download the file from OneDrive and return the stream
            var downloadStream = await response.Content.ReadAsStreamAsync();
            return downloadStream;
        }

        /// <summary>
        /// Performs a file upload to OneDrive overwriting an existing file using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="filePath">File reference to the file to upload</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UploadFileViaSimpleUpload(string filePath, OneDriveItem oneDriveItem)
        {
            // Read the file to upload
            var fileInfo = new FileInfo(filePath);
            return await UpdateFileViaSimpleUpload(fileInfo, oneDriveItem);
        }

        /// <summary>
        /// Performs a file upload to OneDrive overwriting an existing file using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="file">FileInfo reference to the file to upload</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UpdateFileViaSimpleUpload(FileInfo file, OneDriveItem oneDriveItem)
        {
            using (var fileStream = file.OpenRead())
            {
                return await UpdateFileViaSimpleUpload(fileStream, oneDriveItem);
            }
        }

        /// <summary>
        /// Performs a file upload to OneDrive overwriting an existing file using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="oneDriveItem">OneDriveItem of the existing item that should be updated on OneDrive</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UpdateFileViaSimpleUpload(Stream fileStream, OneDriveItem oneDriveItem)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveItem.RemoteItem != null)
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", oneDriveItem.RemoteItem.ParentReference.DriveId, "/items/", oneDriveItem.Id, "/content");
            }
            else if (oneDriveItem.ParentReference != null && !string.IsNullOrEmpty(oneDriveItem.ParentReference.DriveId))
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", oneDriveItem.ParentReference.DriveId, "/items/", oneDriveItem.Id, "/content");
            }
            else if (!string.IsNullOrEmpty(oneDriveItem.WebUrl) && oneDriveItem.WebUrl.Contains("cid="))
            {
                // Item will be uploaded to another drive. Used by OneDrive Personal when using a shared item.
                completeUrl = string.Concat("drives/", oneDriveItem.WebUrl.Remove(0, oneDriveItem.WebUrl.IndexOf("cid=") + 4), "/items/", oneDriveItem.Id, "/content");
            }
            else
            {
                // Item will be uploaded to the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveItem.Id, "/content");
            }

            completeUrl = ConstructCompleteUrl(completeUrl);

            return await UploadFileViaSimpleUploadInternal(fileStream, completeUrl);
        }

        /// <summary>
        /// Performs a file upload to OneDrive using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="parentFolder">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UploadFileViaSimpleUpload(Stream fileStream, string fileName, OneDriveItem parentFolder)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (parentFolder.RemoteItem != null)
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", parentFolder.RemoteItem.ParentReference.DriveId, "/items/", parentFolder.RemoteItem.Id, "/children/", fileName, "/content");
            }
            else if (parentFolder.ParentReference != null && !string.IsNullOrEmpty(parentFolder.ParentReference.DriveId))
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", parentFolder.ParentReference.DriveId, "/items/", parentFolder.Id, "/children/", fileName, "/content");
            }
            else if(!string.IsNullOrEmpty(parentFolder.WebUrl) && parentFolder.WebUrl.Contains("cid="))
            {
                // Item will be uploaded to another drive. Used by OneDrive Personal when using a shared item.
                completeUrl = string.Concat("drives/", parentFolder.WebUrl.Remove(0, parentFolder.WebUrl.IndexOf("cid=") + 4), "/items/", parentFolder.Id, "/children/", fileName, "/content");
            }
            else
            {
                // Item will be uploaded to the current user its drive
                completeUrl = string.Concat("drive/items/", parentFolder.Id, "/children/", fileName, "/content");
            }

            completeUrl = ConstructCompleteUrl(completeUrl);

            return await UploadFileViaSimpleUploadInternal(fileStream, completeUrl);
        }

        /// <summary>
        /// Performs a file upload to OneDrive using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="oneDriveUrl">The URL to POST the file contents to</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        protected async Task<OneDriveItem> UploadFileViaSimpleUploadInternal(Stream fileStream, string oneDriveUrl)
        { 
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Create an HTTPClient instance to communicate with the REST API of OneDrive
            using (var client = CreateHttpClient(accessToken.AccessToken))
            {
                // Load the content to upload
                using (var content = new StreamContent(fileStream))
                {
                    // Indicate that we're sending binary data
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    // Construct the PUT message towards the webservice
                    using (var request = new HttpRequestMessage(HttpMethod.Put, oneDriveUrl))
                    {
                        // Set the content to upload
                        request.Content = content;

                        // Request the response from the webservice
                        using (var response = await client.SendAsync(request))
                        {
                            // Read the response as a string
                            var responseString = await response.Content.ReadAsStringAsync();

                            // Convert the JSON result to its appropriate type
                            try
                            {
                                var options = new JsonSerializerOptions();
                                options.Converters.Add(new JsonStringEnumConverter());

                                var responseOneDriveItem = JsonSerializer.Deserialize<OneDriveItem>(responseString, options);
                                responseOneDriveItem.OriginalJson = responseString;

                                return responseOneDriveItem;
                            }
                            catch(JsonException e)
                            {
                                throw new Exceptions.InvalidResponseException(responseString, e);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Performs a file upload to OneDrive using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="file">File reference to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UploadFileViaSimpleUpload(FileInfo file, string fileName, OneDriveItem oneDriveItem)
        {          
            // Read the file to upload
            using (var fileStream = file.OpenRead())
            {
                return await UploadFileViaSimpleUpload(fileStream, fileName, oneDriveItem);
            }
        }

        /// <summary>
        /// Performs a file upload to OneDrive using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="filePath">Path to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UploadFileViaSimpleUpload(string filePath, string fileName, OneDriveItem oneDriveItem)
        {
            var file = new FileInfo(filePath);
            return await UploadFileViaSimpleUpload(file, fileName, oneDriveItem);
        }

        /// <summary>
        /// Uploads a file to OneDrive using the resumable method. Better for large files or unstable network connections.
        /// </summary>
        /// <param name="filePath">Path to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="parentFolder">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UploadFileViaResumableUpload(string filePath, string fileName, OneDriveItem parentFolder)
        {
            var file = new FileInfo(filePath);
            return await UploadFileViaResumableUpload(file, fileName, parentFolder, null);
        }

        /// <summary>
        /// Uploads a file to OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="file">FileInfo instance pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="parentFolder">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <param name="fragmentSizeInBytes">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections. Provide NULL to use the default.</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public virtual async Task<OneDriveItem> UploadFileViaResumableUpload(FileInfo file, string fileName, OneDriveItem parentFolder, long? fragmentSizeInBytes)
        {
            // Open the source file for reading
            using (var fileStream = file.OpenRead())
            {
                return await UploadFileViaResumableUpload(fileStream, fileName, parentFolder, fragmentSizeInBytes);
            }
        }

        /// <summary>
        /// Uploads a file to OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="fileStream">Stream pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="parentFolder">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <param name="fragmentSizeInBytes">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public virtual async Task<OneDriveItem> UploadFileViaResumableUpload(Stream fileStream, string fileName, OneDriveItem parentFolder, long? fragmentSizeInBytes)
        {
            var oneDriveUploadSession = await CreateResumableUploadSession(fileName, parentFolder);
            return await UploadFileViaResumableUploadInternal(fileStream, oneDriveUploadSession, fragmentSizeInBytes);
        }

        /// <summary>
        /// Uploads a file to OneDrive updating the contents of an existing file using the resumable method. Better for large files or unstable network connections.
        /// </summary>
        /// <param name="filePath">Path to the file to upload</param>
        /// <param name="oneDriveItem">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UpdateFileViaResumableUpload(string filePath, OneDriveItem oneDriveItem)
        {
            var file = new FileInfo(filePath);
            return await UpdateFileViaResumableUpload(file, oneDriveItem, null);
        }

        /// <summary>
        /// Uploads a file to OneDrive updating the contents of an existing file using the resumable file upload method
        /// </summary>
        /// <param name="file">FileInfo instance pointing to the file to upload</param>
        /// <param name="oneDriveItem">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <param name="fragmentSizeInBytes">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections. Provide NULL to use the default.</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public virtual async Task<OneDriveItem> UpdateFileViaResumableUpload(FileInfo file, OneDriveItem oneDriveItem, long? fragmentSizeInBytes)
        {
            // Open the source file for reading
            using (var fileStream = file.OpenRead())
            {
                return await UpdateFileViaResumableUpload(fileStream, oneDriveItem, fragmentSizeInBytes);
            }
        }

        /// <summary>
        /// Uploads a file to OneDrive updating the contents of an existing file using the resumable file upload method
        /// </summary>
        /// <param name="fileStream">Stream pointing to the file to upload</param>
        /// <param name="oneDriveItem">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <param name="fragmentSizeInBytes">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public virtual async Task<OneDriveItem> UpdateFileViaResumableUpload(Stream fileStream, OneDriveItem oneDriveItem, long? fragmentSizeInBytes)
        {
            var oneDriveUploadSession = await CreateResumableUploadSession(oneDriveItem);
            return await UploadFileViaResumableUploadInternal(fileStream, oneDriveUploadSession, fragmentSizeInBytes);
        }

        /// <summary>
        /// Uploads a file to OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="fileStream">Stream pointing to the file to upload</param>
        /// <param name="oneDriveUploadSession">Upload session under which the upload will be performed</param>
        /// <param name="fragmentSizeInBytes">Size in bytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections.</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        protected virtual async Task<OneDriveItem> UploadFileViaResumableUploadInternal(Stream fileStream, OneDriveUploadSession oneDriveUploadSession, long? fragmentSizeInBytes)
        {
            if(fileStream == null)
            {
                throw new ArgumentNullException("fileStream");
            }
            if(oneDriveUploadSession == null)
            {
                throw new ArgumentNullException("oneDriveUploadSession");
            }

            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Amount of bytes succesfully sent
            long totalBytesSent = 0;

            // Used for retrying failed transmissions
            var transferAttemptCount = 0;
            const int transferMaxAttempts = 3;

            do
            {
                // Keep a counter how many times it has been attempted to send this file
                transferAttemptCount++;

                // Start sending the file from the first byte
                long currentPosition = 0;

                // Defines a buffer which will be filled with bytes from the original file and then sent off to the OneDrive webservice
                var fragmentBuffer = new byte[fragmentSizeInBytes ?? ResumableUploadChunkSizeInBytes];

                // Create an HTTPClient instance to communicate with the REST API of OneDrive to perform the upload 
                using (var client = CreateHttpClient(accessToken.AccessToken))
                {
                    // Keep looping through the source file length until we've sent all bytes to the OneDrive webservice
                    while (currentPosition < fileStream.Length)
                    {
                        var fragmentSuccessful = true;

                        // Define the end position in the file bytes based on the buffer size we're using to send fragments of the file to OneDrive
                        var endPosition = currentPosition + fragmentBuffer.LongLength;

                        // Make sure our end position isn't further than the file size in which case it would be the last fragment of the file to be sent
                        if (endPosition > fileStream.Length) endPosition = fileStream.Length;

                        // Define how many bytes should be read from the source file
                        var amountOfBytesToSend = (int) (endPosition - currentPosition);

                        // Copy the bytes from the source file into the buffer
                        await fileStream.ReadAsync(fragmentBuffer, 0, amountOfBytesToSend);

                        // Load the content to upload
                        using (var content = new ByteArrayContent(fragmentBuffer, 0, amountOfBytesToSend))
                        {
                            // Indicate that we're sending binary data
                            content.Headers.Add("Content-Type", "application/octet-stream");

                            // Provide information to OneDrive which range of bytes we're going to send and the total amount of bytes the file exists out of
                            content.Headers.Add("Content-Range", string.Concat("bytes ", currentPosition, "-", endPosition - 1, "/", fileStream.Length));

                            // Construct the PUT message towards the webservice containing the binary data
                            using (var request = new HttpRequestMessage(HttpMethod.Put, oneDriveUploadSession.UploadUrl))
                            {
                                // Set the binary content to upload
                                request.Content = content;

                                // Send the data to the webservice
                                using (var response = await client.SendAsync(request))
                                {
                                    // Check the response code
                                    switch (response.StatusCode)
                                    {
                                        // Fragment has been received, awaiting next fragment
                                        case HttpStatusCode.Accepted:
                                            // Move the current position pointer to the end of the fragment we've just sent so we continue from there with the next upload
                                            currentPosition = endPosition;
                                            totalBytesSent += amountOfBytesToSend;

                                            // Trigger event
                                            UploadProgressChanged?.Invoke(this, new OneDriveUploadProgressChangedEventArgs(totalBytesSent, fileStream.Length));
                                            break;

                                        // All fragments have been received, the file did already exist and has been overwritten
                                        case HttpStatusCode.OK:
                                        // All fragments have been received, the file has been created
                                        case HttpStatusCode.Created:
                                            // Read the response as a string
                                            var responseString = await response.Content.ReadAsStringAsync();

                                            // Convert the JSON result to its appropriate type
                                            try
                                            {
                                                var options = new JsonSerializerOptions();
                                                options.Converters.Add(new JsonStringEnumConverter());

                                                var responseOneDriveItem = JsonSerializer.Deserialize<OneDriveItem>(responseString, options);
                                                responseOneDriveItem.OriginalJson = responseString;

                                                return responseOneDriveItem;
                                            }
                                            catch (JsonException e)
                                            {
                                                throw new Exceptions.InvalidResponseException(responseString, e);
                                            }

                                        // All other status codes are considered to indicate a failed fragment transmission and will be retried
                                        default:
                                            fragmentSuccessful = false;
                                            break;
                                    }
                                }
                            }
                        }

                        // Check if the fragment was successful, if not, retry the complete upload
                        if (!fragmentSuccessful)
                            break;
                    }
                }
            } while (transferAttemptCount < transferMaxAttempts);

            // Request failed
            return null;
        }

        /// <summary>
        /// Initiates a resumable upload session to OneDrive to overwrite an existing file. It doesn't perform the actual upload yet.
        /// </summary>
        /// <param name="oneDriveItem">OneDriveItem item for which updated content will be uploaded</param>
        /// <returns>OneDriveUploadSession instance containing the details where to upload the content to</returns>
        protected virtual async Task<OneDriveUploadSession> CreateResumableUploadSession(OneDriveItem oneDriveItem)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveItem.RemoteItem != null)
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", oneDriveItem.RemoteItem.ParentReference.DriveId, "/items/", oneDriveItem.RemoteItem.Id, "/createUploadSession");
            }
            else if (oneDriveItem.ParentReference != null && !string.IsNullOrEmpty(oneDriveItem.ParentReference.DriveId))
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", oneDriveItem.ParentReference.DriveId, "/items/", oneDriveItem.Id, "/createUploadSession");
            }
            else if (!string.IsNullOrEmpty(oneDriveItem.WebUrl) && oneDriveItem.WebUrl.Contains("cid="))
            {
                // Item will be uploaded to another drive. Used by OneDrive Personal when using a shared item.
                completeUrl = string.Concat("drives/", oneDriveItem.WebUrl.Remove(0, oneDriveItem.WebUrl.IndexOf("cid=") + 4), "/items/", oneDriveItem.Id, "/createUploadSession");
            }
            else
            {
                // Item will be uploaded to the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveItem.Id, "/createUploadSession");
            }

            completeUrl = ConstructCompleteUrl(completeUrl);

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
        /// Initiates a resumable upload session to OneDrive. It doesn't perform the actual upload yet.
        /// </summary>
        /// <param name="fileName">Filename to store the uploaded content under</param>
        /// <param name="oneDriveFolder">OneDriveItem container in which the file should be uploaded</param>
        /// <returns>OneDriveUploadSession instance containing the details where to upload the content to</returns>
        protected virtual async Task<OneDriveUploadSession> CreateResumableUploadSession(string fileName, OneDriveItem oneDriveFolder)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveFolder.RemoteItem != null)
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", oneDriveFolder.RemoteItem.ParentReference.DriveId, "/items/", oneDriveFolder.RemoteItem.Id, ":/", fileName, ":/createUploadSession");
            }
            else if (oneDriveFolder.ParentReference != null && !string.IsNullOrEmpty(oneDriveFolder.ParentReference.DriveId))
            {
                // Item will be uploaded to another drive
                completeUrl = string.Concat("drives/", oneDriveFolder.ParentReference.DriveId, "/items/", oneDriveFolder.Id, ":/", fileName, ":/createUploadSession");
            }
            else if (!string.IsNullOrEmpty(oneDriveFolder.WebUrl) && oneDriveFolder.WebUrl.Contains("cid="))
            {
                // Item will be uploaded to another drive. Used by OneDrive Personal when using a shared item.
                completeUrl = string.Concat("drives/", oneDriveFolder.WebUrl.Remove(0, oneDriveFolder.WebUrl.IndexOf("cid=") + 4), "/items/", oneDriveFolder.Id, ":/", fileName, ":/createUploadSession");
            }
            else
            {
                // Item will be uploaded to the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveFolder.Id, ":/", fileName, ":/createUploadSession");
            }

            completeUrl = ConstructCompleteUrl(completeUrl);

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
        /// Retrieves data from the OneDrive API
        /// </summary>
        /// <typeparam name="T">Type of OneDrive entity to expect to be returned</typeparam>
        /// <param name="url">Url fragment after the OneDrive base Uri which indicated the type of information to return</param>
        /// <returns>OneDrive entity filled with the information retrieved from the OneDrive API</returns>
        protected virtual async Task<T> GetData<T>(string url) where T : OneDriveItemBase
        {
            // Construct the complete URL to call
            var completeUrl = ConstructCompleteUrl(url);

            // Call the OneDrive webservice
            var result = await SendMessageReturnOneDriveItem<T>("", HttpMethod.Get, completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Sends a HTTP DELETE to OneDrive to delete a file
        /// </summary>
        /// <param name="oneDriveUrl">The OneDrive API url to call to delete an item</param>
        /// <returns>True if successful, false if failed</returns>
        protected virtual async Task<bool> DeleteItemInternal(string oneDriveUrl)
        {
            // Construct the complete URL to call
            var completeUrl = ConstructCompleteUrl(oneDriveUrl);

            // Call the OneDrive webservice
            var result = await SendMessageReturnBool(null, HttpMethod.Delete, completeUrl, HttpStatusCode.NoContent);
            return result;
        }

        /// <summary>
        /// Sends a HTTP POST to OneDrive to copy an item on OneDrive
        /// </summary>
        /// <param name="oneDriveSource">The OneDrive Item to be copied</param>
        /// <param name="oneDriveDestinationParent">The OneDrive parent item to copy the item into</param>
        /// <param name="destinationName">The name of the item at the destination where it will be copied to</param>
        /// <returns>True if successful, false if failed</returns>
        protected virtual async Task<bool> CopyItemInternal(OneDriveItem oneDriveSource, OneDriveItem oneDriveDestinationParent, string destinationName)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveSource.RemoteItem != null)
            {
                // Item will be copied from another drive
                completeUrl = string.Concat("drives/", oneDriveSource.RemoteItem.ParentReference.DriveId, "/items/", oneDriveSource.RemoteItem.Id, "/copy");
            }
            else if (!string.IsNullOrEmpty(oneDriveSource.ParentReference.DriveId))
            {
                // Item will be coped from another drive
                completeUrl = string.Concat("drives/", oneDriveSource.ParentReference.DriveId, "/items/", oneDriveSource.Id, "/copy");
            }
            else
            {
                // Item will be copied frp, the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveSource.Id, "/copy");
            }
            completeUrl = ConstructCompleteUrl(completeUrl);

            // Construct the OneDriveParentItemReference entity with the item to be copied details
            var requestBody = new OneDriveParentItemReference
            {
                ParentReference = new OneDriveItemReference
                {
                    Id = oneDriveDestinationParent.Id
                },
                Name = destinationName
            };

            // Call the OneDrive webservice
            var result = await SendMessageReturnBool(requestBody, HttpMethod.Post, completeUrl, HttpStatusCode.Accepted, true);
            return result;
        }

        /// <summary>
        /// Sends a HTTP PATCH to OneDrive to move an item on OneDrive
        /// </summary>
        /// <param name="oneDriveSource">The OneDrive Item to be moved</param>
        /// <param name="oneDriveDestinationParent">The OneDrive parent item to move the item to</param>
        /// <returns>True if successful, false if failed</returns>
        protected virtual async Task<bool> MoveItemInternal(OneDriveItem oneDriveSource, OneDriveItem oneDriveDestinationParent)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveSource.RemoteItem != null)
            {
                // Item to copy is shared from another drive
                completeUrl = string.Concat("drives/", oneDriveSource.RemoteItem.ParentReference.DriveId, "/items/", oneDriveSource.RemoteItem.Id);
            }
            else
            {
                // Item to copy resides on the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveSource.Id);
            }

            completeUrl = ConstructCompleteUrl(completeUrl);

            // Construct the OneDriveParentItemReference entity with the item to be moved details
            var requestBody = new OneDriveParentItemReference
            {
                ParentReference = new OneDriveItemReference
                {
                    Id = oneDriveDestinationParent.Id,
                    DriveId = oneDriveDestinationParent.ParentReference.DriveId
                },
            };

            // Call the OneDrive webservice
            var result = await SendMessageReturnBool(requestBody, new HttpMethod("PATCH"), completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Sends a HTTP PATCH to OneDrive to rename an item on OneDrive
        /// </summary>
        /// <param name="oneDriveSource">The OneDrive Item to be renamed</param>
        /// <param name="name">The new name to give to the OneDrive item</param>
        /// <returns>True if successful, false if failed</returns>
        protected virtual async Task<bool> RenameItemInternal(OneDriveItem oneDriveSource, string name)
        {
            // Construct the complete URL to call
            string completeUrl;
            if (oneDriveSource.RemoteItem != null)
            {
                // Item to copy is shared from another drive
                completeUrl = string.Concat("drives/", oneDriveSource.RemoteItem.ParentReference.DriveId, "/items/", oneDriveSource.RemoteItem.Id);
            }
            else
            {
                // Item to copy resides on the current user its drive
                completeUrl = string.Concat("drive/items/", oneDriveSource.Id);
            }

            completeUrl = ConstructCompleteUrl(completeUrl);

            // Construct the OneDriveItem entity with the item to be renamed details
            var requestBody = new OneDriveItem
            {
                Name = name
            };

            // Call the OneDrive webservice
            var result = await SendMessageReturnBool(requestBody, new HttpMethod("PATCH"), completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Sends a message to the OneDrive webservice and returns a OneDriveBaseItem with the response
        /// </summary>
        /// <typeparam name="T">OneDriveBaseItem type of the expected response</typeparam>
        /// <param name="oneDriveItem">OneDriveBaseItem of the message to send to the webservice</param>
        /// <param name="httpMethod">HttpMethod to use to send with the webservice (i.e. POST, GET, PUT, etc.)</param>
        /// <param name="url">Url of the OneDrive webservice to send the message to</param>
        /// <param name="expectedHttpStatusCode">The expected Http result status code. Optional. If provided and the webservice returns a different response, the return type will be NULL to indicate failure.</param>
        /// <returns>Typed OneDrive entity with the result from the webservice</returns>
        protected virtual async Task<T> SendMessageReturnOneDriveItem<T>(OneDriveItemBase oneDriveItem, HttpMethod httpMethod, string url, HttpStatusCode? expectedHttpStatusCode = null) where T : OneDriveItemBase
        {
            var bodyText = oneDriveItem != null ? JsonSerializer.Serialize(oneDriveItem) : null;

            return await SendMessageReturnOneDriveItem<T>(bodyText, httpMethod, url, expectedHttpStatusCode);
        }

        /// <summary>
        /// Sends a message to the OneDrive webservice and returns a OneDriveBaseItem with the response
        /// </summary>
        /// <typeparam name="T">OneDriveBaseItem type of the expected response</typeparam>
        /// <param name="bodyText">String with the message to send to the webservice</param>
        /// <param name="httpMethod">HttpMethod to use to send with the webservice (i.e. POST, GET, PUT, etc.)</param>
        /// <param name="url">Url of the OneDrive webservice to send the message to</param>
        /// <param name="expectedHttpStatusCode">The expected Http result status code. Optional. If provided and the webservice returns a different response, the return type will be NULL to indicate failure.</param>
        /// <returns>Typed OneDrive entity with the result from the webservice</returns>
        protected virtual async Task<T> SendMessageReturnOneDriveItem<T>(string bodyText, HttpMethod httpMethod, string url, HttpStatusCode? expectedHttpStatusCode = null) where T : OneDriveItemBase
        {
            var responseString = await SendMessageReturnString(bodyText, httpMethod, url, expectedHttpStatusCode);

            // Validate output was generated
            if (string.IsNullOrEmpty(responseString)) return null;

            // Convert the JSON result to its appropriate type
            try
            {
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter());

                var responseOneDriveItem = JsonSerializer.Deserialize<T>(responseString, options);
                responseOneDriveItem.OriginalJson = responseString;

                return responseOneDriveItem;
            }
            catch (JsonException e)
            {
                throw new Exceptions.InvalidResponseException(responseString, e);
            }
        }

        /// <summary>
        /// Sends a message to the OneDrive webservice and returns a string with the response
        /// </summary>
        /// <param name="bodyText">String with the message to send to the webservice</param>
        /// <param name="httpMethod">HttpMethod to use to send with the webservice (i.e. POST, GET, PUT, etc.)</param>
        /// <param name="url">Url of the OneDrive webservice to send the message to</param>
        /// <param name="expectedHttpStatusCode">The expected Http result status code. Optional. If provided and the webservice returns a different response, the return type will be NULL to indicate failure.</param>
        /// <returns>String containing the response of the webservice</returns>
        protected virtual async Task<string> SendMessageReturnString(string bodyText, HttpMethod httpMethod, string url, HttpStatusCode? expectedHttpStatusCode = null)
        {
            using (var response = await SendMessageReturnHttpResponse(bodyText, httpMethod, url))
            {
                if (!expectedHttpStatusCode.HasValue || (expectedHttpStatusCode.HasValue && response != null && response.StatusCode == expectedHttpStatusCode.Value))
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return responseString;
                }
                return null;
            }
        }

        /// <summary>
        /// Sends a message to the OneDrive webservice and returns a bool indicating if the response matched the expected HTTP status code result
        /// </summary>
        /// <param name="oneDriveItem">OneDriveBaseItem of the message to send to the webservice</param>
        /// <param name="httpMethod">HttpMethod to use to send with the webservice (i.e. POST, GET, PUT, etc.)</param>
        /// <param name="url">Url of the OneDrive webservice to send the message to</param>
        /// <param name="expectedHttpStatusCode">The expected Http result status code. Optional. If provided and the webservice returns a different response, the return type will be NULL to indicate failure.</param>
        /// <param name="preferRespondAsync">Provide true if the Prefer Async header should be sent along with the request. This is required for some requests. Optional, default = false = do not send the async header.</param>
        /// <returns>Bool indicating if the HTTP response status from the webservice matched the provided expectedHttpStatusCode</returns>
        protected virtual async Task<bool> SendMessageReturnBool(OneDriveItemBase oneDriveItem, HttpMethod httpMethod, string url, HttpStatusCode expectedHttpStatusCode, bool preferRespondAsync = false)
        {
            string bodyText = null;
            if (oneDriveItem != null)
            {
                bodyText = JsonSerializer.Serialize(oneDriveItem);
            }

            using (var response = await SendMessageReturnHttpResponse(bodyText, httpMethod, url, preferRespondAsync))
            {
                return response != null && response.StatusCode == expectedHttpStatusCode;
            }
        }

        /// <summary>
        /// Sends a message to the OneDrive webservice and returns the HttpResponse instance
        /// </summary>
        /// <param name="bodyText">String with the message to send to the webservice</param>
        /// <param name="httpMethod">HttpMethod to use to send with the webservice (i.e. POST, GET, PUT, etc.)</param>
        /// <param name="url">Url of the OneDrive webservice to send the message to</param>
        /// <param name="preferRespondAsync">Provide true if the Prefer Async header should be sent along with the request. This is required for some requests. Optional, default = false = do not send the async header.</param>
        /// <returns>HttpResponse of the webservice call. Note that the caller needs to dispose the returned instance.</returns>
        protected virtual async Task<HttpResponseMessage> SendMessageReturnHttpResponse(string bodyText, HttpMethod httpMethod, string url, bool preferRespondAsync = false)
        {
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Create an HTTPClient instance to communicate with the REST API of OneDrive
            using (var client = CreateHttpClient(accessToken.AccessToken))
            {
                // Load the content to upload
                using (var content = new StringContent(bodyText ?? "", Encoding.UTF8, "application/json"))
                {
                    // Construct the message towards the webservice
                    using (var request = new HttpRequestMessage(httpMethod, url))
                    {
                        if (preferRespondAsync)
                        {
                            // Add a header to prefer the operation to happen while we continue processing our code
                            request.Headers.Add("Prefer", "respond-async");
                        }

                        // Check if a body to send along with the request has been provided
                        if (!string.IsNullOrEmpty(bodyText) && httpMethod != HttpMethod.Get)
                        {
                            // Set the content to send along in the message body with the request
                            request.Content = content;
                        }

                        // Request the response from the webservice
                        var response = await client.SendAsync(request);
                        return response;
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates a new HttpClient preconfigured for use. Note that the caller is responsible for disposing this object.
        /// </summary>
        /// <param name="bearerToken">Bearer token to add to the HTTP Client for authorization (optional)</param>
        /// <returns>HttpClient instance</returns>
        protected HttpClient CreateHttpClient(string bearerToken = null)
        {
            // Define the HttpClient settings
            var httpClientHandler = new HttpClientHandler
            {
                UseDefaultCredentials = ProxyCredential == null,
                UseProxy = ProxyConfiguration != null,
                Proxy = ProxyConfiguration
            };

            // Check if we need specific credentials for the proxy
            if (ProxyCredential != null && httpClientHandler.Proxy != null)
            {
                httpClientHandler.Proxy.Credentials = ProxyCredential;
            }

            // Create the new HTTP Client
            var httpClient = new HttpClient(httpClientHandler);

            if (!string.IsNullOrEmpty(bearerToken))
            {
                // Provide the access token through a bearer authorization header
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", bearerToken);
            }

            return httpClient;
        }

        /// <summary>
        /// Constructs the complete Url to be called based on the part of the url provided that contains the command
        /// </summary>
        /// <param name="commandUrl">Part of the URL to call that contains the command to execute for the API that is being called</param>
        /// <returns>Full URL to call the API</returns>
        protected virtual string ConstructCompleteUrl(string commandUrl)
        {
            if (commandUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                return commandUrl;
            }
            return string.Concat(commandUrl.StartsWith("drives/", StringComparison.InvariantCultureIgnoreCase) ? GraphApiBaseUrl : OneDriveApiBaseUrl, commandUrl);
        }

        #endregion
    }
}
