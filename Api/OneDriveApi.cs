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
    public class OneDriveApi
    {
        #region Properties

        /// <summary>
        /// The oAuth 2.0 Application Client ID
        /// Create one at https://account.live.com/developers/applications/index
        /// </summary>
        public readonly string ClientId;

        /// <summary>
        /// The oAuth 2.0 Application Client Secret
        /// Create one at https://account.live.com/developers/applications/index
        /// </summary>
        public readonly string ClientSecret;

        /// <summary>
        /// Authorization token used for requesting tokens
        /// </summary>
        public string AuthorizationToken { get; private set; }

        /// <summary>
        /// Access Token for communicating with OneDrive
        /// </summary>
        public OneDriveAccessToken AccessToken { get; private set; }

        /// <summary>
        /// Date and time until which the access token should be valid based on the information provided by the oAuth provider
        /// </summary>
        public DateTime? AccessTokenValidUntil { get; private set; }

        #endregion

        #region Constants

        /// <summary>
        /// The url to provide as the redirect URL after successful authentication
        /// </summary>
        private const string AuthenticationRedirectUrl = "https://login.live.com/oauth20_desktop.srf";

        /// <summary>
        /// String formatted Uri that needs to be called to authenticate
        /// </summary>
        private const string AuthenticateUri = "https://login.live.com/oauth20_authorize.srf?client_id={0}&scope={1}&response_type=code&redirect_uri=" + AuthenticationRedirectUrl;

        /// <summary>
        /// String formatted Uri that can be called to sign out from the OneDrive API
        /// </summary>
        private const string SignoutUri = "https://login.live.com/oauth20_logout.srf?client_id={0}&redirect_uri=" + AuthenticationRedirectUrl;

        /// <summary>
        /// The url where an access token can be obtained
        /// </summary>
        private const string AccessTokenUri = "https://login.live.com/oauth20_token.srf";

        /// <summary>
        /// Base URL of the OneDrive API
        /// </summary>
        private const string OneDriveApiBasicUrl = "https://api.onedrive.com/v1.0/";

        /// <summary>
        /// Defines the maximum allowed file size that can be used for basic uploads
        /// </summary>
        public static long MaximumBasicFileUploadSizeInBytes = 5 * 1024;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new instance of the OneDriveApi
        /// </summary>
        /// <param name="clientId">OneDrive Client ID to use to connect</param>
        /// <param name="clientSecret">OneDrive Client Secret to use to connect</param>
        public OneDriveApi(string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
        }

        #endregion

        #region Public Methods - Authentication

        /// <summary>
        /// Instantiates a new instance of the OneDriveApi
        /// </summary>
        /// <param name="clientId">OneDrive Client ID to use to connect</param>
        /// <param name="clientSecret">OneDrive Client Secret to use to connect</param>
        /// <param name="refreshToken">Refreshtoken to use to get an access token</param>
        public static async Task<OneDriveApi> GetOneDriveApiFromRefreshToken(string clientId, string clientSecret, string refreshToken)
        {
            var oneDriveApi = new OneDriveApi(clientId, clientSecret);
            oneDriveApi.AccessToken = await oneDriveApi.GetAccessTokenFromRefreshToken(refreshToken);

            return oneDriveApi;
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
        /// Returns the Uri that needs to be called to authenticate to the OneDrive API
        /// </summary>
        /// <param name="scopes">String with one or more scopes separated with a space to which you want to request access. See https://msdn.microsoft.com/en-us/library/office/dn631845.aspx for the scopes that you can use.</param>
        /// <returns>Uri that needs to be called in a browser to authenticate to the OneDrive API</returns>
        public Uri GetAuthenticationUri(string scopes)
        {
            var uri = string.Format(AuthenticateUri, ClientId, scopes);
            return new Uri(uri);
        }

        /// <summary>
        /// Returns the Uri that needs to be called to authenticate to the OneDrive API using the default scope of "wl.signin wl.offline_access onedrive.readwrite"
        /// </summary>
        /// <returns>Uri that needs to be called in a browser to authenticate to the OneDrive API</returns>
        public Uri GetAuthenticationUri()
        {
            var uri = string.Format(AuthenticateUri, ClientId, "wl.signin wl.offline_access onedrive.readwrite");
            return new Uri(uri);
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
            if (!url.StartsWith(string.Concat(AuthenticationRedirectUrl, "?")))
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
                    return await GetAccessTokenFromRefreshToken(AccessToken.RefreshToken);
                }
            }

            // No access token is available, check if we have an authorization token
            if (string.IsNullOrEmpty(AuthorizationToken))
            {
                // No access token, no authorization token, we need to authorize first which can't be done without an UI
                return null;
            }

            // No access token but we have an authorization token, request the access token
            return await GetAccessTokenFromAuthorizationToken(AuthorizationToken);
        }

        #endregion

        #region Public Methods - Validate

        /// <summary>
        /// Validates if the provided filename is valid to be used on OneDrive
        /// </summary>
        /// <param name="filename">Filename to validate</param>
        /// <returns>True if filename is valid to be used, false if it isn't</returns>
        public static bool ValidFilename(string filename)
        {
            char[] restrictedCharacters = { '\\', '/', ':', '*', '?', '<', '>', '|' };
            return filename.IndexOfAny(restrictedCharacters) == -1;
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
        /// <returns></returns>
        public async Task<OneDriveItemCollection> GetChildrenByPath(string path)
        {
            return await GetData<OneDriveItemCollection>(string.Concat("drive/root:/", path, ":/children"));
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
        /// <param name="filePath">Full path to the file to upload</param>
        /// <param name="fileName">Filename to assign to the file on OneDrive</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>OneDriveItem representing the uploaded file when successful or NULL when the upload failed</returns>
        public async Task<OneDriveItem> UploadFileAs(string filePath, string fileName, OneDriveItem oneDriveItem)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("Provided file could not be found", "filePath");
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
                throw new ArgumentException("Provided file contains illegal characters in its filename", "filePath");
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
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, oneDriveRequestUrl);

            // Construct the request to send to the OneDrive API
            var request = WebRequest.CreateHttp(completeUrl);
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers["Authorization"] = string.Concat("bearer ", accessToken.AccessToken);

            // Construct the JSON to send in the POST message
            var newFolder = new OneDriveRequestShare { SharingType = linkType };
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            var bodyText = JsonConvert.SerializeObject(newFolder, settings);

            // Add the JSON to the message request body
            var stream = await request.GetRequestStreamAsync();
            var requestWriter = new StreamWriter(stream);
            await requestWriter.WriteAsync(bodyText);
            await requestWriter.FlushAsync();

            // Send the request and await the response
            var response = await request.GetResponseAsync();
            var httpResponse = response as HttpWebResponse;

            // Verify the response outcome
            if (httpResponse == null || httpResponse.StatusCode != HttpStatusCode.Created)
            {
                return null;
            }

            // Parse the JSON response into an entity
            var responseBodyStreamReader = new StreamReader(httpResponse.GetResponseStream());
            var responseBody = await responseBodyStreamReader.ReadToEndAsync();
            var result = JsonConvert.DeserializeObject<OneDrivePermission>(responseBody);
            result.OriginalJson = responseBody;

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
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, oneDriveRequestUrl);

            // Construct the request to send to the OneDrive API
            var request = WebRequest.CreateHttp(completeUrl);
            request.Method = "POST";
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Headers["Authorization"] = string.Concat("bearer ", accessToken.AccessToken);

            // Construct the JSON to send in the POST message
            var newFolder = new OneDriveCreateFolder { Name = folderName, Folder = new object()};
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            var bodyText = JsonConvert.SerializeObject(newFolder, settings);
            
            // Add the JSON to the message request body
            var stream = await request.GetRequestStreamAsync();
            var requestWriter = new StreamWriter(stream);
            await requestWriter.WriteAsync(bodyText);
            await requestWriter.FlushAsync();

            // Send the request and await the response
            var response = await request.GetResponseAsync();
            var httpResponse = response as HttpWebResponse;

            // Verify the response outcome
            if (httpResponse == null || httpResponse.StatusCode != HttpStatusCode.Created)
            {
                return null;
            }

            // Parse the JSON response into an entity
            var responseBodyStreamReader = new StreamReader(httpResponse.GetResponseStream());
            var responseBody = await responseBodyStreamReader.ReadToEndAsync();
            var result = JsonConvert.DeserializeObject<OneDriveItem>(responseBody);
            result.OriginalJson = responseBody;

            return result;
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
            var client = CreateHttpClient();

            // Provide the access token through a bearer authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken.AccessToken);

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
        /// <param name="file">File reference to the file to upload</param>
        /// <param name="fileName">The filename under which the file should be stored on OneDrive</param>
        /// <param name="oneDriveItem">OneDriveItem of the folder to which the file should be uploaded</param>
        /// <returns>The resulting OneDrive item representing the uploaded file</returns>
        public async Task<OneDriveItem> UploadFileViaSimpleUpload(FileInfo file, string fileName, OneDriveItem oneDriveItem)
        {
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Construct the complete URL to call
            var oneDriveUrl = string.Concat(OneDriveApiBasicUrl, "drive/items/", oneDriveItem.Id, "/children/", fileName, "/content");

            // Read the file to upload
            using (var fileStream = file.OpenRead())
            {
                // Create the PUT request to push the file
                var request = WebRequest.CreateHttp(oneDriveUrl);
                request.ContentType = "application/octet-stream";
                request.Method = "PUT";
                request.Accept = "application/json";
                request.Headers["Authorization"] = string.Concat("bearer ", accessToken.AccessToken);

                // Construct the request body with the file contents
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await CopyWithProgressAsync(fileStream, requestStream);

                    // Await the server response
                    using (var httpResponse = await request.GetResponseAsync())
                    {
                        using (var stream = httpResponse.GetResponseStream())
                        {
                            if (stream == null)
                            {
                                return null;
                            }

                            using (var reader = new StreamReader(stream))
                            {
                                var result = await reader.ReadToEndAsync();

                                // Convert the JSON results to its appropriate type
                                var content = JsonConvert.DeserializeObject<OneDriveItem>(result);
                                content.OriginalJson = result;

                                return content;
                            }
                        }
                    }
                }
            }
        }

        private static async Task<long> CopyWithProgressAsync(Stream source, Stream destination, long sourceLength = 0, int bufferSize = 64 * 1024)
        {
            long bytesWritten = 0;
            long totalBytesToWrite = sourceLength;

            byte[] copyBuffer = new byte[bufferSize];
            int read;
            while ((read = await source.ReadAsync(copyBuffer, 0, copyBuffer.Length)) > 0)
            {
                await destination.WriteAsync(copyBuffer, 0, read);
                bytesWritten += read;

                //System.Diagnostics.Debug.WriteLine("CopyWithProgress: {0} / {1}", bytesWritten, totalBytesToWrite);
                //if (null != progressReport)
                //{
                //    int percentComplete = 0;
                //    if (sourceLength > 0)
                //        percentComplete = (int)((bytesWritten / (double)totalBytesToWrite) * 100);
                //    progressReport(percentComplete, bytesWritten, totalBytesToWrite);
                //}
            }

            await destination.FlushAsync();

            return bytesWritten;
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
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Construct the URL to initiate the upload
            var oneDriveUrl = string.Concat(OneDriveApiBasicUrl, "drive/items/", oneDriveItem.Id, ":/", fileName, ":/upload.createSession");

            // Open the source file for reading
            using (var source = file.OpenRead())
            {
                // Create the inintial POST request to the OneDrive service to announce the upload
                var request = WebRequest.CreateHttp(oneDriveUrl);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Headers["Authorization"] = string.Concat("bearer ", accessToken.AccessToken);

                // Add the conflictbehavior header to always overwrite the file if it already exists on OneDrive
                var uploadItemContainer = new OneDriveUploadSessionItemContainer
                {
                    Item = new OneDriveUploadSessionItem
                    {
                        FilenameConflictBehavior = NameConflictBehavior.Replace
                    }
                };

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                var bodyText = JsonConvert.SerializeObject(uploadItemContainer, settings);

                var requestStream = await request.GetRequestStreamAsync();
                var writer = new StreamWriter(requestStream, Encoding.UTF8, 1024*1024, true);
                await writer.WriteAsync(bodyText);
                await writer.FlushAsync();

                var response = await request.GetResponseAsync();
                var httpResponse = response as HttpWebResponse;

                if (httpResponse == null || httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var uploadSessionResult = await ParseJsonResponse<OneDriveUploadSession>(httpResponse);

                // Start sending the file from the first byte
                long currentPosition = 0;

                // Defines a buffer which will be filled with bytes from the original file and then sent off to the OneDrive webservice
                var fragmentBuffer = new byte[fragmentSizeInKiloByte * 1000];
                
                // Keep looping through the source file length until we've sent all bytes to the OneDrive webservice
                while (currentPosition < source.Length)
                {
                    // Define the end position in the file bytes based on the buffer size we're using to send fragments of the file to OneDrive
                    var endPosition = currentPosition + fragmentBuffer.LongLength;
                    
                    // Make sure our end position isn't further than the file size in which case it would be the last fragment of the file to be sent
                    if (endPosition > source.Length) endPosition = source.Length;

                    // Define how many bytes should be read from the source file
                    var amountOfBytesToSend = (int) (endPosition - currentPosition);

                    // Copy the bytes from the source file into the buffer
                    await source.ReadAsync(fragmentBuffer, 0, amountOfBytesToSend);

                    // Create the PUT HTTP request to upload the fragment
                    var uploadFragmentRequest = WebRequest.CreateHttp(uploadSessionResult.UploadUrl);
                    uploadFragmentRequest.Method = "PUT";
                    uploadFragmentRequest.ContentLength = amountOfBytesToSend;

                    // Provide information to OneDrive which range of bytes we're going to send and the total amount of bytes the file exists out of
                    uploadFragmentRequest.Headers["Content-Range"] = string.Concat("bytes ", currentPosition, "-", endPosition - 1, "/", source.Length);
                    
                    // Provide the access token to authorize this request
                    uploadFragmentRequest.Headers["Authorization"] = string.Concat("bearer ", accessToken.AccessToken);
                    
                    // Used for retrying failed fragment transmissions
                    var fragmentSuccessful = false;
                    var fragmentAttemptCount = 0;
                    const int fragmentMaxAttempts = 3;

                    do
                    {
                        // Keep a counter how many times it has been attempted to send this fragment
                        fragmentAttemptCount++;
          
                        // Copy the buffer contents to the HTTP stream
                        using (var uploadFragmentRequestStream = await uploadFragmentRequest.GetRequestStreamAsync())
                        {
                            try
                            {
                                await uploadFragmentRequestStream.WriteAsync(fragmentBuffer, 0, amountOfBytesToSend);
                            }
                            catch (WebException)
                            {
                                // Do nothing. This exception will be thrown when trying to upload a file that already exists at the
                                // target location. We still did get a response though, so we continue to try to parse the response.
                            }
                        }

                        // Await the server response
                        using (var uploadFragmentResponse = await uploadFragmentRequest.GetResponseAsync())
                        {
                            using (var uploadFragmentResponseHttpResponse = uploadFragmentResponse as HttpWebResponse)
                            {
                                if (uploadFragmentResponseHttpResponse == null)
                                {
                                    return null;
                                }

                                switch (uploadFragmentResponseHttpResponse.StatusCode)
                                {
                                    // Fragment has been received, awaiting next fragment
                                    case HttpStatusCode.Accepted:
                                        // Move the current position pointer to the end of the fragment we've just sent so we continue from there with the next upload
                                        currentPosition = endPosition;
                                        fragmentSuccessful = true;
                                        break;

                                    // All fragments have been received, the file did already exist and has been overwritten
                                    case HttpStatusCode.OK:
                                    // All fragments have been received, the file has been created
                                    case HttpStatusCode.Created:
                                        var content = await ParseJsonResponse<OneDriveItem>(uploadFragmentResponseHttpResponse);
                                        return content;

                                    // All other status codes are considered to indicate a failed fragment transmission and will be retried
                                }
                            }
                        }
                    } while (!fragmentSuccessful && fragmentAttemptCount < fragmentMaxAttempts);

                    // Verify if we got out of the retry loop because a fragment exceeded its maximum retry count. In that case we abort the complete upload.
                    if (fragmentAttemptCount == fragmentMaxAttempts)
                    {
                        // Abort the complete upload
                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the contents of the provided WebResponse and parses the JSON contained in it into the provided OneDrive entity type
        /// </summary>
        /// <typeparam name="T">OneDrive entity type to parse the JSON response into</typeparam>
        /// <param name="webResponse">WebResponse containing the OneDrive JSON response</param>
        /// <returns>Typed OneDrive entity based on the JSON contained in the WebResponse</returns>
        private static async Task<T> ParseJsonResponse<T>(WebResponse webResponse) where T : OneDriveItemBase
        {
            using (var stream = webResponse.GetResponseStream())
            {
                if (stream == null)
                {
                    return null;
                }

                var reader = new StreamReader(stream);
                var result = await reader.ReadToEndAsync();

                // Convert the JSON results to its appropriate type
                var content = JsonConvert.DeserializeObject<T>(result);
                content.OriginalJson = result;

                return content;
            }
        }

        //internal async Task<ODDataModel> PutFileFragment(Uri serviceUri, byte[] fragment, ContentRange requestRange)
        //{
        //    var request = await CreateHttpRequestAsync(serviceUri, ApiConstants.HttpPut);
        //    request.ContentRange = requestRange.ToContentRangeHeaderValue();

        //    var stream = await request.GetRequestStreamAsync();
        //    await stream.WriteAsync(fragment, 0, (int)requestRange.BytesInRange);

        //    var response = await request.GetResponseAsync();
        //    if (response.StatusCode == HttpStatusCode.Accepted)
        //    {
        //        return await response.ConvertToDataModel<ODUploadSession>();
        //    }
        //    else if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
        //    {
        //        return await response.ConvertToDataModel<ODItem>();
        //    }
        //    else
        //    {
        //        var exception = await response.ToException();
        //        throw exception;
        //    }
        //}

        /// <summary>
        /// Gets an access token from the provided authorization token
        /// </summary>
        /// <param name="authorizationToken">Authorization token</param>
        /// <returns>Access token for OneDrive or NULL if unable to retrieve an access token</returns>
        private async Task<OneDriveAccessToken> GetAccessTokenFromAuthorizationToken(string authorizationToken)
        {
            var queryBuilder = new QueryStringBuilder();
            queryBuilder.Add("client_id", ClientId);
            queryBuilder.Add("redirect_uri", AuthenticationRedirectUrl);
            queryBuilder.Add("client_secret", ClientSecret);
            queryBuilder.Add("code", authorizationToken);
            queryBuilder.Add("grant_type", "authorization_code");
            return await PostToTokenEndPoint(queryBuilder);
        }

        /// <summary>
        /// Gets an access token from the provided refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <returns>Access token for OneDrive or NULL if unable to retrieve an access token</returns>
        private async Task<OneDriveAccessToken> GetAccessTokenFromRefreshToken(string refreshToken)
        {
            var queryBuilder = new QueryStringBuilder();
            queryBuilder.Add("client_id", ClientId);
            queryBuilder.Add("redirect_uri", AuthenticationRedirectUrl);
            queryBuilder.Add("client_secret", ClientSecret);
            queryBuilder.Add("refresh_token", refreshToken);
            queryBuilder.Add("grant_type", "refresh_token");
            return await PostToTokenEndPoint(queryBuilder);
        }

        /// <summary>
        /// Retrieves data from the OneDrive API
        /// </summary>
        /// <typeparam name="T">Type of OneDrive entity to expect to be returned</typeparam>
        /// <param name="url">Url fragment after the OneDrive base Uri which indicated the type of information to return</param>
        /// <returns>OneDrive entity filled with the information retrieved from the OneDrive API</returns>
        private async Task<T> GetData<T>(string url) where T : OneDriveItemBase
        {
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();
            
            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, url);

            // Create an HTTPClient instance to communicate with the REST API of OneDrive
            var client = CreateHttpClient();

            // Provide the access token through a bearer authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", accessToken.AccessToken);
            
            // Send the request to the OneDrive API
            var response = await client.GetAsync(completeUrl);

            // Verify if the response was successful
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            // Retrieve the results from the OneDrive API
            var result = await response.Content.ReadAsStringAsync();
            
            // Convert the JSON results to its appropriate type
            var content = JsonConvert.DeserializeObject<T>(result);
            content.OriginalJson = result;
            
            return content;
        }

        /// <summary>
        /// Sends a HTTP POST to the OneDrive Token EndPoint
        /// </summary>
        /// <param name="queryBuilder">The querystring parameters to send in the POST body</param>
        /// <returns>Access token for OneDrive or NULL if unable to retrieve an access token</returns>
        private async Task<OneDriveAccessToken> PostToTokenEndPoint(QueryStringBuilder queryBuilder)
        {
            var request = WebRequest.CreateHttp(AccessTokenUri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            var stream = await request.GetRequestStreamAsync();
            var requestWriter = new StreamWriter(stream);
            await requestWriter.WriteAsync(queryBuilder.ToString());
            await requestWriter.FlushAsync();
            var response = await request.GetResponseAsync();
            var httpResponse = response as HttpWebResponse;
            
            if (httpResponse == null || httpResponse.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
          
            var responseBodyStreamReader = new StreamReader(httpResponse.GetResponseStream());
            var responseBody = await responseBodyStreamReader.ReadToEndAsync();
            var appTokenResult = JsonConvert.DeserializeObject<OneDriveAccessToken>(responseBody);

            AccessToken = appTokenResult;
            AccessTokenValidUntil = DateTime.Now.AddSeconds(appTokenResult.AccessTokenExpirationDuration);

            return appTokenResult;
        }

        /// <summary>
        /// Sends a HTTP DELETE to OneDrive to delete a file
        /// </summary>
        /// <param name="oneDriveUrl">The OneDrive API url to call to delete an item</param>
        /// <returns>True if successful, false if failed</returns>
        private async Task<bool> DeleteItemInternal(string oneDriveUrl)
        {
            // Get an access token to perform the request to OneDrive
            var accessToken = await GetAccessToken();

            // Construct the complete URL to call
            var completeUrl = string.Concat(OneDriveApiBasicUrl, oneDriveUrl);

            var request = WebRequest.CreateHttp(completeUrl);
            request.Method = "DELETE";
            request.Headers["Authorization"] = string.Concat("bearer ", accessToken.AccessToken);
            var response = await request.GetResponseAsync();
            var httpResponse = response as HttpWebResponse;

            return httpResponse != null && httpResponse.StatusCode == HttpStatusCode.NoContent;
        }

        /// <summary>
        /// Instantiates a new HttpClient preconfigured for use
        /// </summary>
        /// <returns>HttpClient instance</returns>
        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient(new HttpClientHandler {UseDefaultCredentials = true});
            return httpClient;
        }

        #endregion

    }
}
