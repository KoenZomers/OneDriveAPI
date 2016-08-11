using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using KoenZomers.OneDrive.Api.Entities;
using KoenZomers.OneDrive.Api.Enums;
using KoenZomers.OneDrive.Api.Helpers;
using Newtonsoft.Json;

namespace KoenZomers.OneDrive.Api
{
    /// <summary>
    /// Base OneDrive API functionality that is valid for either the Consumer OneDrive or the OneDrive for Business platform
    /// </summary>
    public abstract class OneDriveApi
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
        /// Defines if a proxy should be used to connect to the OneDrive API
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        /// If provided, this proxy will be used for communication with the OneDrive API. If not provided but UseProxy is set to true, the default system proxy will be used.
        /// </summary>
        public WebProxy ProxyConfiguration { get; set; }

        /// <summary>
        /// Authorization token used for requesting tokens
        /// </summary>
        public string AuthorizationToken { get; private set; }

        /// <summary>
        /// Access Token for communicating with OneDrive
        /// </summary>
        public OneDriveAccessToken AccessToken { get; protected set; }

        /// <summary>
        /// Date and time until which the access token should be valid based on the information provided by the oAuth provider
        /// </summary>
        public DateTime? AccessTokenValidUntil { get; protected set; }

        /// <summary>
        /// Base URL of the OneDrive API
        /// </summary>
        protected string OneDriveApiBasicUrl { get; set; }

        /// <summary>
        /// Defines the maximum allowed file size that can be used for basic uploads
        /// </summary>
        public static long MaximumBasicFileUploadSizeInBytes = 5 * 1024;

        #endregion

        #region Abstract Properties

        /// <summary>
        /// The url to provide as the redirect URL after successful authentication
        /// </summary>
        public abstract string AuthenticationRedirectUrl { get; set; }

        /// <summary>
        /// String formatted Uri that needs to be called to authenticate
        /// </summary>
        protected abstract string AuthenticateUri { get; }

        /// <summary>
        /// The url where an access token can be obtained
        /// </summary>
        protected abstract string AccessTokenUri { get; }

        /// <summary>
        /// String formatted Uri that can be called to sign out from the OneDrive API
        /// </summary>
        public abstract string SignoutUri { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new instance of a OneDrive API
        /// </summary>
        /// <param name="clientId">OneDrive Client ID to use to connect</param>
        /// <param name="clientSecret">OneDrive Client Secret to use to connect</param>
        protected OneDriveApi(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        #endregion

        #region Public Methods - Authentication

        /// <summary>
        /// Returns the Uri that needs to be called to authenticate to the OneDrive API
        /// </summary>
        /// <returns>Uri that needs to be called in a browser to authenticate to the OneDrive API</returns>
        public abstract Uri GetAuthenticationUri();

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
        /// Tries to retrieve an access token based on the tokens already available in this OneDrive instance
        /// </summary>
        /// <returns>OneDrive access token or NULL if unable to get an access token</returns>
        public async Task<OneDriveAccessToken> GetAccessToken()
        {
            // Check if we have an access token
            if (AccessToken != null)
            {
                // We have an access token, check if its still valid
                if (AccessTokenValidUntil.HasValue && AccessTokenValidUntil.Value > DateTime.Now)
                {
                    // Access token is still valid, use it
                    return AccessToken;
                }

                // Access token is no longer valid, check if we have a refresh token to request a new access token
                if (!string.IsNullOrEmpty(AccessToken.RefreshToken))
                {
                    // We have a refresh token, request a new access token using it
                    AccessToken = await GetAccessTokenFromRefreshToken(AccessToken.RefreshToken);
                    return AccessToken;
                }
            }

            // No access token is available, check if we have an authorization token
            if (string.IsNullOrEmpty(AuthorizationToken))
            {
                // No access token, no authorization token, we need to authorize first which can't be done without an UI
                return null;
            }

            // No access token but we have an authorization token, request the access token
            AccessToken = await GetAccessTokenFromAuthorizationToken(AuthorizationToken);
            AccessTokenValidUntil = DateTime.Now.AddSeconds(AccessToken.AccessTokenExpirationDuration);
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
        /// Sends a HTTP POST to the OneDrive Token EndPoint
        /// </summary>
        /// <param name="queryBuilder">The querystring parameters to send in the POST body</param>
        /// <returns>Access token for OneDrive or NULL if unable to retrieve an access token</returns>
        /// <exception cref="Exceptions.TokenRetrievalFailedException">Thrown when unable to retrieve a valid access token</exception>
        protected async Task<OneDriveAccessToken> PostToTokenEndPoint(QueryStringBuilder queryBuilder)
        {
            if (string.IsNullOrEmpty(AccessTokenUri))
            {
                throw new InvalidOperationException("AccessTokenUri has not been set");
            }

            // Create an HTTPClient instance to communicate with the REST API of OneDrive
            using (var client = CreateHttpClient())
            {
                // Load the content to upload
                using (var content = new StringContent(queryBuilder.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded"))
                {
                    // Construct the message towards the webservice
                    using (var request = new HttpRequestMessage(HttpMethod.Post, AccessTokenUri))
                    {
                        // Set the content to send along in the message body with the request
                        request.Content = content;

                        // Request the response from the webservice
                        var response = await client.SendAsync(request);
                        var responseBody = await response.Content.ReadAsStringAsync();

                        // Verify if the request was successful (response status 200-299)
                        if (response.IsSuccessStatusCode)
                        {
                            // Successfully retrieved token, parse it from the response
                            var appTokenResult = JsonConvert.DeserializeObject<OneDriveAccessToken>(responseBody);

                            return appTokenResult;
                        }

                        // Not able to retrieve a token, parse the error and throw it as an exception
                        OneDriveError errorResult;
                        try
                        {
                            // Try to parse the response as a OneDrive API error message
                            errorResult = JsonConvert.DeserializeObject<OneDriveError>(responseBody);
                        }
                        catch(Exception ex)
                        {
                            throw new Exceptions.TokenRetrievalFailedException(innerException: ex);
                        }

                        throw new Exceptions.TokenRetrievalFailedException(message: errorResult.ErrorDescription, errorDetails: errorResult);
                    }
                }
            }       
        }

        /// <summary>
        /// Authenticates to OneDrive using the provided Refresh Token
        /// </summary>
        /// <param name="refreshToken">Refreshtoken to use to authenticate to OneDrive</param>
        public async Task AuthenticateUsingRefreshToken(string refreshToken)
        {
            AccessToken = await GetAccessTokenFromRefreshToken(refreshToken);
            AccessTokenValidUntil = DateTime.Now.AddSeconds(AccessToken.AccessTokenExpirationDuration);
        }

        /// <summary>
        /// Gets an access token from the provided authorization token
        /// </summary>
        /// <param name="authorizationToken">Authorization token</param>
        /// <returns>Access token for OneDrive or NULL if unable to retrieve an access token</returns>
        protected abstract Task<OneDriveAccessToken> GetAccessTokenFromAuthorizationToken(string authorizationToken);

        /// <summary>
        /// Gets an access token from the provided refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>Access token for OneDrive or NULL if unable to retrieve an access token</returns>
        protected abstract Task<OneDriveAccessToken> GetAccessTokenFromRefreshToken(string refreshToken);

        #endregion

        #region Public Methods - Validate

        /// <summary>
        /// Validates if the provided filename is valid to be used on OneDrive
        /// </summary>
        /// <param name="filename">Filename to validate</param>
        /// <returns>True if filename is valid to be used, false if it isn't</returns>
        public static bool ValidFilename(string filename)
        {
            return true;
        }

        #endregion

        #region Public Methods - Getting content

        /// <summary>
        /// Retrieves the OneDrive drive information
        /// </summary>
        public async Task<OneDriveDrive> GetDrive()
        {
            return await GetData<OneDriveDrive>("drive");
        }

        /// <summary>
        /// Retrieves the OneDrive root folder
        /// </summary>
        public async Task<OneDriveItem> GetDriveRoot()
        {
            return await GetData<OneDriveItem>("drive/root");
        }

        /// <summary>
        /// Retrieves the children under the OneDrive root folder
        /// </summary>
        public async Task<OneDriveItemCollection> GetDriveRootChildren()
        {
            return await GetData<OneDriveItemCollection>("drive/root/children");
        }

        /// <summary>
        /// Retrieves the children under the provided OneDrive path
        /// </summary>
        /// <param name="path">Path within OneDrive to retrieve the child items of</param>
        /// <returns>OneDriveItemCollection containing all items in the requested folder</returns>
        public async Task<OneDriveItemCollection> GetChildrenByPath(string path)
        {
            return await GetData<OneDriveItemCollection>(string.Concat("drive/root:/", path, ":/children"));
        }

        /// <summary>
        /// Retrieves the children under the OneDrive folder with the provided id
        /// </summary>
        /// <param name="id">Unique identifier of the folder under which to retrieve the child items</param>
        /// <returns>OneDriveItemCollection containing all items in the requested folder</returns>
        public async Task<OneDriveItemCollection> GetChildrenByFolderId(string id)
        {
            return await GetData<OneDriveItemCollection>(string.Concat("drive/items/", id, "/children"));
        }

        /// <summary>
        /// Retrieves the children under the provided OneDrive Item
        /// </summary>
        /// <param name="item">OneDrive item to retrieve the child items of</param>
        /// <returns></returns>
        public async Task<OneDriveItemCollection> GetChildrenByParentItem(OneDriveItem item)
        {
            return await GetData<OneDriveItemCollection>(string.Concat("drive/items/", item.Id, "/children"));
        }

        /// <summary>
        /// Retrieves the OneDrive Item
        /// </summary>
        /// <param name="path">Path of the OneDrive item to retrieve</param>
        /// <returns></returns>
        public async Task<OneDriveItem> GetItem(string path)
        {
            return await GetData<OneDriveItem>(string.Concat("drive/root:/", path));
        }

        /// <summary>
        /// Retrieves the OneDrive Item
        /// </summary>
        /// <param name="id">Unique identifier of the OneDrive item to retrieve</param>
        /// <returns></returns>
        public async Task<OneDriveItem> GetItemById(string id)
        {
            return await GetData<OneDriveItem>(string.Concat("drive/items/", id));
        }

        /// <summary>
        /// Retrieves the OneDrive folder item or creates it if it doesn't exist yet
        /// </summary>
        /// <param name="path">Path of the OneDrive folder to retrieve or create</param>
        /// <returns></returns>
        public async Task<OneDriveItem> GetFolderOrCreate(string path)
        {
            // Try to get the folder
            var folder = await GetData<OneDriveItem>(string.Concat("drive/root:/", path));

            if (folder != null)
            {
                // Folder found, return it
                return folder;
            }

            // Folder not found, create it
            var folderName = path.Contains("/") ? path.Remove(0, path.LastIndexOf("/", StringComparison.Ordinal) + 1) : path;
            var parentPath = path.Contains("/") ? path.Remove(path.Length - folderName.Length - 1) : "";
            folder = await CreateFolder(parentPath, folderName);

            return folder;
        }

        /// <summary>
        /// Retrieves the items in the CameraRoll folder
        /// </summary>
        public async Task<OneDriveItemCollection> GetDriveCameraRollFolder()
        {
            return await GetData<OneDriveItemCollection>("drive/special/cameraroll");
        }

        /// <summary>
        /// Retrieves the items in the Documents folder
        /// </summary>
        public async Task<OneDriveItemCollection> GetDriveDocumentsFolder()
        {
            return await GetData<OneDriveItemCollection>("drive/special/documents");
        }

        /// <summary>
        /// Retrieves the items in the Photos folder
        /// </summary>
        public async Task<OneDriveItemCollection> GetDrivePhotosFolder()
        {
            return await GetData<OneDriveItemCollection>("drive/special/photos");
        }

        /// <summary>
        /// Retrieves the items in the Public folder
        /// </summary>
        public async Task<OneDriveItemCollection> GetDrivePublicFolder()
        {
            return await GetData<OneDriveItemCollection>("drive/special/public");
        }

        /// <summary>
        /// Searches for items on OneDrive with the provided query
        /// </summary>
        /// <param name="query">Search query to use</param>
        /// <returns>All OneDrive items resulting from the search</returns>
        public async Task<IList<OneDriveItem>> Search(string query)
        {
            return await SearchInternal(string.Concat("drive/root/view.search?q=", query));
        }

        /// <summary>
        /// Searches for items on OneDrive in the provided path with the provided query
        /// </summary>
        /// <param name="query">Search query to use</param>
        /// <param name="path">OneDrive path where to search in</param>
        /// <returns>All OneDrive items resulting from the search</returns>
        public async Task<IList<OneDriveItem>> Search(string query, string path)
        {
            return await SearchInternal(string.Concat("drive/root:/", path, "/view.search?q=", query));
        }

        /// <summary>
        /// Searches for items on OneDrive in the provided path with the provided query
        /// </summary>
        /// <param name="query">Search query to use</param>
        /// <param name="oneDriveItem">OneDrive item representing a folder to search in</param>
        /// <returns>All OneDrive items resulting from the search</returns>
        public async Task<IList<OneDriveItem>> Search(string query, OneDriveItem oneDriveItem)
        {
            return await SearchInternal(string.Concat("drive/items/", oneDriveItem.Id, "/view.search?q=", query));
        }

        /// <summary>
        /// Deletes the provided OneDriveItem from OneDrive
        /// </summary>
        /// <param name="oneDriveItem">The OneDriveItem reference to delete from OneDrive</param>
        public async Task<bool> Delete(OneDriveItem oneDriveItem)
        {
            return await DeleteItemInternal(string.Concat("drive/items/", oneDriveItem.Id));
        }

        /// <summary>
        /// Deletes the provided OneDriveItem from OneDrive
        /// </summary>
        /// <param name="oneDriveItemPath">The path to the OneDrive item to delete from OneDrive</param>
        public async Task<bool> Delete(string oneDriveItemPath)
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
        public async Task<bool> Copy(string oneDriveSourceItemPath, string oneDriveDestinationItemPath, string destinationName = null)
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
        public async Task<bool> Copy(OneDriveItem oneDriveSourceItem, OneDriveItem oneDriveDestinationItem, string destinationName = null)
        {
            return await CopyItemInternal(oneDriveSourceItem, oneDriveDestinationItem, destinationName);
        }

        /// <summary>
        /// Moves the provided OneDriveItem to the provided destination on OneDrive
        /// </summary>
        /// <param name="oneDriveSourceItemPath">The path to the OneDrive Item to be moved</param>
        /// <param name="oneDriveDestinationItemPath">The path to the OneDrive parent item to move the item into</param>
        /// <returns>True if successful, false if failed</returns>
        public async Task<bool> Move(string oneDriveSourceItemPath, string oneDriveDestinationItemPath)
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
        public async Task<bool> Move(OneDriveItem oneDriveSourceItem, OneDriveItem oneDriveDestinationItem)
        {
            return await MoveItemInternal(oneDriveSourceItem, oneDriveDestinationItem);
        }

        /// <summary>
        /// Renames the provided OneDriveItem to the provided name
        /// </summary>
        /// <param name="oneDriveItemPath">The path to the OneDrive Item to be renamed</param>
        /// <param name="name">The new name to assign to the OneDrive item</param>
        /// <returns>True if successful, false if failed</returns>
        public async Task<bool> Rename(string oneDriveItemPath, string name)
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
        public async Task<bool> Rename(OneDriveItem oneDriveItemPath, string name)
        {
            return await RenameItemInternal(oneDriveItemPath, name);
        }

        /// <summary>
        /// Downloads the contents of the item on OneDrive at the provided path to the folder provided keeping the original filename
        /// </summary>
        /// <param name="path">Path to an item on OneDrive to download its contents of</param>
        /// <param name="saveTo">Path where to save the file to. The same filename as used on OneDrive will be used to save the file under.</param>
        /// <returns>True if download was successful, false if it failed</returns>
        public async Task<bool> DownloadItem(string path, string saveTo)
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
        public async Task<bool> DownloadItem(OneDriveItem oneDriveItem, string saveTo)
        {
            return await DownloadItemAndSaveAs(oneDriveItem, Path.Combine(saveTo, oneDriveItem.Name));
        }

        /// <summary>
        /// Downloads the contents of the item on OneDrive at the provided path to the full path provided
        /// </summary>
        /// <param name="path">Path to an item on OneDrive to download its contents of</param>
        /// <param name="saveAs">Full path including filename where to store the downloaded file</param>
        /// <returns>True if download was successful, false if it failed</returns>
        public async Task<bool> DownloadItemAndSaveAs(string path, string saveAs)
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
        public async Task<bool> DownloadItemAndSaveAs(OneDriveItem oneDriveItem, string saveAs)
        {
            using (var stream = await DownloadItemInternal(oneDriveItem))
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
        public async Task<Stream> DownloadItem(string path)
        {
            var oneDriveItem = await GetItem(path);
            return await DownloadItem(oneDriveItem);
        }

        /// <summary>
        /// Downloads the contents of the provided OneDriveItem and returns the contents as a stream
        /// </summary>
        /// <param name="oneDriveItem">OneDriveItem to download its contents of</param>
        /// <returns>Stream with the contents of the item on OneDrive</returns>
        public async Task<Stream> DownloadItem(OneDriveItem oneDriveItem)
        {
            return await DownloadItemInternal(oneDriveItem);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="oneDriveFolder">Path to a OneDrive folder where to upload the file to</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFile(string filePath, string oneDriveFolder)
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
        public async Task<OneDriveItem> UploadFileAs(string filePath, string fileName, string oneDriveFolder)
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
        public async Task<OneDriveItem> UploadFileAs(Stream fileStream, string fileName, string oneDriveFolder)
        {
            var oneDriveItem = await GetItem(oneDriveFolder);
            return await UploadFileAs(fileStream, fileName, oneDriveItem);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileAs(string filePath, string fileName, OneDriveItem oneDriveItem)
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
                return await UploadFileViaSimpleUpload(fileToUpload, fileName, oneDriveItem);
            }

            // Use the resumable upload method
            return await UploadFileViaResumableUpload(fileToUpload, fileName, oneDriveItem);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive using the provided filename
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileAs(Stream fileStream, string fileName, OneDriveItem oneDriveItem)
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

            // Verify which upload method should be used
            if (fileStream.Length <= MaximumBasicFileUploadSizeInBytes)
            {
                // Use the basic upload method                
                return await UploadFileViaSimpleUpload(fileStream, fileName, oneDriveItem);
            }

            // Use the resumable upload method
            return await UploadFileViaResumableUpload(fileStream, fileName, oneDriveItem);
        }

        /// <summary>
        /// Uploads the provided file to OneDrive keeping the original filename
        /// </summary>
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFile(string filePath, OneDriveItem oneDriveItem)
        {
            return await UploadFileAs(filePath, null, oneDriveItem);
        }

        /// <summary>
        /// Creates a new folder under the provided parent OneDrive item with the provided name
        /// </summary>
        /// <param name="parentPath">The path to the OneDrive folder under which the folder should be created</param>
        /// <param name="folderName">Name to assign to the new folder</param>
        /// <returns>OneDriveItem entity representing the newly created folder or NULL if the operation fails</returns>
        public async Task<OneDriveItem> CreateFolder(string parentPath, string folderName)
        {
            return await CreateFolderInternal(string.Concat("drive/root:/", parentPath, ":/children"), folderName);
        }

        /// <summary>
        /// Creates a new folder under the provided parent OneDrive item with the provided name
        /// </summary>
        /// <param name="parentItem">The OneDrive item under which the folder should be created</param>
        /// <param name="folderName">Name to assign to the new folder</param>
        /// <returns>OneDriveItem entity representing the newly created folder or NULL if the operation fails</returns>
        public async Task<OneDriveItem> CreateFolder(OneDriveItem parentItem, string folderName)
        {
            return await CreateFolderInternal(string.Concat("drive/items/", parentItem.Id, "/children"), folderName);
        }

        /// <summary>
        /// Shares a OneDrive item
        /// </summary>
        /// <param name="itemPath">The path to the OneDrive item to share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        public async Task<OneDrivePermission> ShareItem(string itemPath, OneDriveLinkType linkType)
        {
            return await ShareItemInternal(string.Concat("drive/root:/", itemPath, ":/action.createLink"), linkType);
        }

        /// <summary>
        /// Shares a OneDrive item
        /// </summary>
        /// <param name="item">The OneDrive item to share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        public async Task<OneDrivePermission> ShareItem(OneDriveItem item, OneDriveLinkType linkType)
        {
            return await ShareItemInternal(string.Concat("drive/items/", item.Id, "/action.createLink"), linkType);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shares a OneDrive item
        /// </summary>
        /// <param name="oneDriveRequestUrl">The OneDrive request url which creates the share</param>
        /// <param name="linkType">Type of sharing to request</param>
        /// <returns>OneDrivePermission entity representing the share or NULL if the operation fails</returns>
        private async Task<OneDrivePermission> ShareItemInternal(string oneDriveRequestUrl, OneDriveLinkType linkType)
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, oneDriveRequestUrl);

            // Construct the OneDriveRequestShare entity with the sharing details
            var requestShare = new OneDriveRequestShare { SharingType = linkType };

            // Call the OneDrive webservice
            var result = await SendMessageReturnOneDriveItem<OneDrivePermission>(requestShare, HttpMethod.Post, completeUrl, HttpStatusCode.Created);
            return result;
        }

        /// <summary>
        /// Creates a new folder under the provided parent OneDrive item with the provided name
        /// </summary>
        /// <param name="oneDriveRequestUrl">The OneDrive request url which creates a new folder</param>
        /// <param name="folderName">Name to assign to the new folder</param>
        /// <returns>OneDriveItem entity representing the newly created folder or NULL if the operation fails</returns>
        private async Task<OneDriveItem> CreateFolderInternal(string oneDriveRequestUrl, string folderName)
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, oneDriveRequestUrl);            

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
        private async Task<IList<OneDriveItem>> SearchInternal(string searchUrl)
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
                nextSearchUrl = results.NextLink.Remove(0, OneDriveApiBasicUrl.Length);

            } while (true);

            return allResults;
        }

        /// <summary>
        /// Downloads the contents of the provided OneDriveItem to the location provided
        /// </summary>
        /// <param name="item">OneDriveItem to download its contents of</param>
        /// <returns>Stream with the downloaded content</returns>
        private async Task<Stream> DownloadItemInternal(OneDriveItem item)
        {
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, "drive/items/", item.Id, "/content");

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
        /// Performs a file upload to OneDrive using the simple OneDrive API. Best for small files on reliable network connections.
        /// </summary>
        /// <param name="fileStream">Stream to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UploadFileViaSimpleUpload(Stream fileStream, string fileName, OneDriveItem oneDriveItem)
        {
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Construct the complete URL to call
            var oneDriveUrl = string.Concat(OneDriveApiBasicUrl, "drive/items/", oneDriveItem.Id, "/children/", fileName, "/content");

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
                            var responseOneDriveItem = JsonConvert.DeserializeObject<OneDriveItem>(responseString);
                            responseOneDriveItem.OriginalJson = responseString;

                            return responseOneDriveItem;
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
        /// <param name="oneDriveItem">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <returns></returns>
        public async Task<OneDriveItem> UploadFileViaResumableUpload(string filePath, string fileName, OneDriveItem oneDriveItem)
        {
            var file = new FileInfo(filePath);
            return await UploadFileViaResumableUpload(file, fileName, oneDriveItem);
        }

        /// <summary>
        /// Uploads a file to OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="file">FileInfo instance pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="oneDriveItem">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <param name="fragmentSizeInKiloByte">Size in kilobytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections. Default is 5000 which means 5 MB fragments will be used.</param>
        /// <returns></returns>
        public async Task<OneDriveItem> UploadFileViaResumableUpload(FileInfo file, string fileName, OneDriveItem oneDriveItem, short fragmentSizeInKiloByte = 5000)
        {
            // Open the source file for reading
            using (var fileStream = file.OpenRead())
            {
                return await UploadFileViaResumableUpload(fileStream, fileName, oneDriveItem, fragmentSizeInKiloByte);
            }
        }

        /// <summary>
        /// Initiates a resumable upload session to OneDrive. It doesn't perform the actual upload yet.
        /// </summary>
        /// <param name="fileName">Filename to store the uploaded content under</param>
        /// <param name="oneDriveItem">OneDriveItem container in which the file should be uploaded</param>
        /// <returns>OneDriveUploadSession instance containing the details where to upload the content to</returns>
        internal async Task<OneDriveUploadSession> CreateResumableUploadSession(string fileName, OneDriveItem oneDriveItem)
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, "drive/items/", oneDriveItem.Id, ":/", fileName, ":/upload.createSession");

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
        /// Uploads a file to OneDrive using the resumable file upload method
        /// </summary>
        /// <param name="fileStream">Stream pointing to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="oneDriveItem">OneDrive item representing the folder to which the file should be uploaded</param>
        /// <param name="fragmentSizeInKiloByte">Size in kilobytes of the fragments to use for uploading. Higher numbers are faster but require more stable connections, lower numbers are slower but work better with unstable connections. Default is 5000 which means 5 MB fragments will be used.</param>
        /// <returns>OneDriveItem instance representing the uploaded item</returns>
        public async Task<OneDriveItem> UploadFileViaResumableUpload(Stream fileStream, string fileName, OneDriveItem oneDriveItem, short fragmentSizeInKiloByte = 5000)
        {
            // Create a resumable upload session with OneDrive
            var uploadSessionResult = await CreateResumableUploadSession(fileName, oneDriveItem);

            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

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
                var fragmentBuffer = new byte[fragmentSizeInKiloByte*1000];

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
                            using (var request = new HttpRequestMessage(HttpMethod.Put, uploadSessionResult.UploadUrl))
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
                                            break;

                                        // All fragments have been received, the file did already exist and has been overwritten
                                        case HttpStatusCode.OK:
                                        // All fragments have been received, the file has been created
                                        case HttpStatusCode.Created:
                                            // Read the response as a string
                                            var responseString = await response.Content.ReadAsStringAsync();

                                            // Convert the JSON result to its appropriate type
                                            var responseOneDriveItem = JsonConvert.DeserializeObject<OneDriveItem>(responseString);
                                            responseOneDriveItem.OriginalJson = responseString;

                                            return responseOneDriveItem;

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
        /// Retrieves data from the OneDrive API
        /// </summary>
        /// <typeparam name="T">Type of OneDrive entity to expect to be returned</typeparam>
        /// <param name="url">Url fragment after the OneDrive base Uri which indicated the type of information to return</param>
        /// <returns>OneDrive entity filled with the information retrieved from the OneDrive API</returns>
        private async Task<T> GetData<T>(string url) where T : OneDriveItemBase
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, url);

            // Call the OneDrive webservice
            var result = await SendMessageReturnOneDriveItem<T>("", HttpMethod.Get, completeUrl, HttpStatusCode.OK);
            return result;
        }

        /// <summary>
        /// Sends a HTTP DELETE to OneDrive to delete a file
        /// </summary>
        /// <param name="oneDriveUrl">The OneDrive API url to call to delete an item</param>
        /// <returns>True if successful, false if failed</returns>
        private async Task<bool> DeleteItemInternal(string oneDriveUrl)
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, oneDriveUrl);

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
        private async Task<bool> CopyItemInternal(OneDriveItem oneDriveSource, OneDriveItem oneDriveDestinationParent, string destinationName)
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, "drive/items/", oneDriveSource.Id, "/action.copy");

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
        private async Task<bool> MoveItemInternal(OneDriveItem oneDriveSource, OneDriveItem oneDriveDestinationParent)
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, "drive/items/", oneDriveSource.Id);

            // Construct the OneDriveParentItemReference entity with the item to be moved details
            var requestBody = new OneDriveParentItemReference
            {
                ParentReference = new OneDriveItemReference
                {
                    Id = oneDriveDestinationParent.Id
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
        private async Task<bool> RenameItemInternal(OneDriveItem oneDriveSource, string name)
        {
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, "drive/items/", oneDriveSource.Id);

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
        private async Task<T> SendMessageReturnOneDriveItem<T>(OneDriveItemBase oneDriveItem, HttpMethod httpMethod, string url, HttpStatusCode? expectedHttpStatusCode = null) where T : OneDriveItemBase
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            var bodyText = JsonConvert.SerializeObject(oneDriveItem, settings);

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
        private async Task<T> SendMessageReturnOneDriveItem<T>(string bodyText, HttpMethod httpMethod, string url, HttpStatusCode? expectedHttpStatusCode = null) where T : OneDriveItemBase
        {
            var responseString = await SendMessageReturnString(bodyText, httpMethod, url, expectedHttpStatusCode);

            // Validate output was generated
            if (string.IsNullOrEmpty(responseString)) return null;

            // Convert the JSON string result to its appropriate type
            var responseOneDriveItem = JsonConvert.DeserializeObject<T>(responseString);
            responseOneDriveItem.OriginalJson = responseString;

            return responseOneDriveItem;
        }

        /// <summary>
        /// Sends a message to the OneDrive webservice and returns a string with the response
        /// </summary>
        /// <param name="bodyText">String with the message to send to the webservice</param>
        /// <param name="httpMethod">HttpMethod to use to send with the webservice (i.e. POST, GET, PUT, etc.)</param>
        /// <param name="url">Url of the OneDrive webservice to send the message to</param>
        /// <param name="expectedHttpStatusCode">The expected Http result status code. Optional. If provided and the webservice returns a different response, the return type will be NULL to indicate failure.</param>
        /// <returns>String containing the response of the webservice</returns>
        private async Task<string> SendMessageReturnString(string bodyText, HttpMethod httpMethod, string url, HttpStatusCode? expectedHttpStatusCode = null)
        {
            using (var response = await SendMessageReturnHttpResponse(bodyText, httpMethod, url))
            {
                if (expectedHttpStatusCode.HasValue && response != null && response.StatusCode == expectedHttpStatusCode.Value)
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
        private async Task<bool> SendMessageReturnBool(OneDriveItemBase oneDriveItem, HttpMethod httpMethod, string url, HttpStatusCode expectedHttpStatusCode, bool preferRespondAsync = false)
        {
            string bodyText = null;
            if (oneDriveItem != null)
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                bodyText = JsonConvert.SerializeObject(oneDriveItem, settings);
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
        private async Task<HttpResponseMessage> SendMessageReturnHttpResponse(string bodyText, HttpMethod httpMethod, string url, bool preferRespondAsync = false)
        {
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Create an HTTPClient instance to communicate with the REST API of OneDrive
            using (var client = CreateHttpClient(accessToken.AccessToken))
            {
                // Load the content to upload
                using (var content = new StringContent(bodyText, Encoding.UTF8, "application/json"))
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
                UseDefaultCredentials = true,
            };

            // Attach a proxy if set on this API instance
            if (ProxyConfiguration != null || UseProxy)
            {
                httpClientHandler.UseProxy = true;
            }
            if (ProxyConfiguration != null)
            {
                httpClientHandler.Proxy = ProxyConfiguration;
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

        #endregion
    }
}
