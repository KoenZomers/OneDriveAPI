using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using KoenZomers.OneDrive.Api;
using KoenZomers.OneDrive.Api.Entities;
using KoenZomers.OneDrive.Api.Enums;

namespace AuthenticatorApp
{
    public partial class MainForm : Form
    {
        #region Properties

        /// <summary>
        /// Application configuration
        /// </summary>
        private readonly Configuration _configuration;

        /// <summary>
        /// OneDriveApi instance to work with
        /// </summary>
        public OneDriveApi OneDriveApi;

        /// <summary>
        /// The refresh token stored in the App Config
        /// </summary>
        public string RefreshToken;

        #endregion

        public MainForm()
        {
            InitializeComponent();
            _configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            RefreshToken = _configuration.AppSettings.Settings["OneDriveApiRefreshToken"].Value;

            RefreshTokenTextBox.Text = RefreshToken;
            OneDriveTypeCombo.SelectedIndex = 0;
        }

        /// <summary>
        /// Creates a new instance of the OneDrive API
        /// </summary>
        private void InitiateOneDriveApi()
        {
            // Define the type of OneDrive API to instantiate based on the dropdown list selection    
            switch (OneDriveTypeCombo.SelectedIndex)
            {
                case 0:
                    OneDriveApi = new OneDriveConsumerApi(_configuration.AppSettings.Settings["OneDriveConsumerApiClientID"].Value, _configuration.AppSettings.Settings["OneDriveConsumerApiClientSecret"].Value);
                    if(!string.IsNullOrEmpty(_configuration.AppSettings.Settings["OneDriveConsumerApiRedirectUri"].Value))
                    {
                        OneDriveApi.AuthenticationRedirectUrl = _configuration.AppSettings.Settings["OneDriveConsumerApiRedirectUri"].Value;
                    }
                    break;

                case 1:
                    OneDriveApi = new OneDriveForBusinessO365Api(_configuration.AppSettings.Settings["OneDriveForBusinessO365ApiClientID"].Value, _configuration.AppSettings.Settings["OneDriveForBusinessO365ApiClientSecret"].Value);
                    break;

                case 2:
                    OneDriveApi = new OneDriveGraphApi(_configuration.AppSettings.Settings["GraphApiApplicationId"].Value);
                    break;
            }

            OneDriveApi.ProxyConfiguration = UseProxyCheckBox.Checked ? System.Net.WebRequest.DefaultWebProxy : null;
        }

        private async void AuthenticationBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            // Get the currently displayed URL and show it in the textbox
            CurrentUrlTextBox.Text = e.Url.ToString();            

            // Check if the current URL contains the authorization token
            AuthorizationCodeTextBox.Text = OneDriveApi.GetAuthorizationTokenFromUrl(e.Url.ToString());

            // Verify if an authorization token was successfully extracted
            if (!string.IsNullOrEmpty(AuthorizationCodeTextBox.Text))
            {
                // Get an access token based on the authorization token that we now have
                await OneDriveApi.GetAccessToken();
                if (OneDriveApi.AccessToken != null)
                {
                    // Show the access token information in the textboxes
                    AccessTokenTextBox.Text = OneDriveApi.AccessToken.AccessToken;
                    RefreshTokenTextBox.Text = OneDriveApi.AccessToken.RefreshToken;
                    AccessTokenValidTextBox.Text = OneDriveApi.AccessTokenValidUntil.HasValue ? OneDriveApi.AccessTokenValidUntil.Value.ToString("dd-MM-yyyy HH:mm:ss") : "Not valid";
                    
                    // Store the refresh token in the AppSettings so next time you don't have to log in anymore
                    _configuration.AppSettings.Settings["OneDriveApiRefreshToken"].Value = RefreshTokenTextBox.Text;
                    _configuration.Save(ConfigurationSaveMode.Modified);
                    return;
                }
            }

            // If we're on this page, but we didn't get an authorization token, it means that we just signed out, proceed with signing in again
            if (CurrentUrlTextBox.Text.StartsWith(OneDriveApi.SignoutUri))
            {
                var authenticateUri = OneDriveApi.GetAuthenticationUri();
                AuthenticationBrowser.Navigate(authenticateUri);
            }
        }

        private void Step1Button_Click(object sender, EventArgs e)
        {
            // Reset any possible access tokens we may already have
            AccessTokenTextBox.Text = string.Empty;

            // Create a new instance of the OneDriveApi framework
            InitiateOneDriveApi();

            // First sign the current user out to make sure he/she needs to authenticate again
            var signoutUri = OneDriveApi.GetSignOutUri();
            AuthenticationBrowser.Navigate(signoutUri);
        }

        private async void RefreshTokenButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(RefreshTokenTextBox.Text))
            {
                MessageBox.Show("You need to enter a refresh token first in the refresh token field in order to be able to retrieve a new access token based on a refresh token.", "OneDrive API", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Create a new instance of the OneDriveApi framework
            InitiateOneDriveApi();

            // Get a new access token based on the refresh token entered in the textbox
            await OneDriveApi.AuthenticateUsingRefreshToken(RefreshTokenTextBox.Text);

            if (OneDriveApi.AccessToken != null)
            {
                // Display the information of the new access token in the textboxes
                AccessTokenTextBox.Text = OneDriveApi.AccessToken.AccessToken;
                RefreshTokenTextBox.Text = OneDriveApi.AccessToken.RefreshToken;
                AccessTokenValidTextBox.Text = OneDriveApi.AccessTokenValidUntil.HasValue ? OneDriveApi.AccessTokenValidUntil.Value.ToString("dd-MM-yyyy HH:mm:ss") : "Not valid";
            }
        }

        private void AccessTokenTextBox_TextChanged(object sender, EventArgs e)
        {
            var accessTokenAvailable = !string.IsNullOrEmpty(((TextBox) sender).Text);
            OneDriveCommandsPanel.Enabled = accessTokenAvailable;
            AuthenticationBrowser.Visible = !accessTokenAvailable;
            JsonResultTextBox.Visible = accessTokenAvailable;
        }

        private async void GetDriveButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDrive();
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void GetRoodFolderButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDriveRoot();
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void GetRootChildren_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDriveRootChildren();
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void GetDocumentsButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDriveDocumentsFolder();
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void GetCameraRollButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDriveCameraRollFolder();
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void GetPhotos_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDrivePhotosFolder();
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void GetPublicButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDrivePublicFolder();
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void UploadButton_Click(object sender, EventArgs e)
        {
            var fileToUpload = SelectLocalFile();

            // Reset the output field
            JsonResultTextBox.Text = $"Starting upload{Environment.NewLine}";

            // Define the anonynous method to respond to the file upload progress events
            EventHandler <OneDriveUploadProgressChangedEventArgs> progressHandler = delegate(object s, OneDriveUploadProgressChangedEventArgs a) { JsonResultTextBox.Text += $"Uploading - {a.BytesSent} bytes sent / {a.TotalBytes} bytes total ({a.ProgressPercentage}%){Environment.NewLine}"; };

            // Subscribe to the upload progress event
            OneDriveApi.UploadProgressChanged += progressHandler;

            // Upload the file to the root of the OneDrive
            var data = await OneDriveApi.UploadFile(fileToUpload, await OneDriveApi.GetDriveRoot());

            // Unsubscribe from the upload progress event
            OneDriveApi.UploadProgressChanged -= progressHandler;

            // Display the result of the upload
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private string SelectLocalFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Upload to OneDrive";
            dialog.Filter = "All Files (*.*)|*.*";
            dialog.CheckFileExists = true;
            var response = dialog.ShowDialog();

            return response != DialogResult.OK ? null : dialog.FileName;
        }

        private async void GetByPathButton_Click(object sender, EventArgs e)
        {            
            var data = await OneDriveApi.GetChildrenByPath("Demo");
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void GetByIdButton_Click(object sender, EventArgs e)
        {
            var data1 = await OneDriveApi.GetChildrenByFolderId("E499210E61A71FF3!3635");
            JsonResultTextBox.Text = data1 != null ? data1.OriginalJson : "Not found";
        }

        private async void DownloadButton_Click(object sender, EventArgs e)
        {
            var item = await OneDriveApi.GetItem("Test.txt");
            if (item != null)
            {
                using (var stream = await OneDriveApi.DownloadItem(item))
                {
                    using (var writer = new StreamReader(stream))
                    {
                        JsonResultTextBox.Text = await writer.ReadToEndAsync();
                    }
                }
            }
            else
            {
                JsonResultTextBox.Text = "Unable to find Test.txt in the OneDrive root";
            }
        }
        private async void DownloadToButton_Click(object sender, EventArgs e)
        {
            var item = await OneDriveApi.GetItem("Test.txt");
            if (item != null)
            {
                var localFolder = new FileInfo(Application.ExecutablePath).DirectoryName;
                var success = await OneDriveApi.DownloadItem(item, localFolder);
                JsonResultTextBox.Text = success ? "Downloaded successfully to " + localFolder : "Download failed";
            }
            else
            {
                JsonResultTextBox.Text = "Unable to find Test.txt in the OneDrive root";
            }
        }

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            var searchQuery = "photo";
            var data = await OneDriveApi.Search(searchQuery);
            JsonResultTextBox.Text = data.Count.ToString();
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.Delete("Test.txt");
            JsonResultTextBox.Text = data.ToString();
        }

        private async void CreateFolderButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetFolderOrCreate("Test");
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void ShareButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.ShareItem("Test", OneDriveLinkType.Edit);
            JsonResultTextBox.Text = data != null ? data.OriginalJson : "Not available";
        }

        private async void CopyButton_Click(object sender, EventArgs e)
        {
            var success = await OneDriveApi.Copy("Test.txt", "Test", "Copied Test.txt");
            JsonResultTextBox.Text = success ? "Copy Successfull" : "Copy Failed";
        }

        private async void MoveButton_Click(object sender, EventArgs e)
        {
            var success = await OneDriveApi.Move("Test.txt", "Test");
            JsonResultTextBox.Text = success ? "Move Successfull" : "Move Failed";
        }

        private async void RenameButton_Click(object sender, EventArgs e)
        {
            var success = await OneDriveApi.Rename("Test.txt", "Renamed Test.txt");
            JsonResultTextBox.Text = success ? "Rename Successfull" : "Rename Failed";
        }

        private async void SharedWithMeButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetSharedWithMe();
            JsonResultTextBox.Text = data.OriginalJson;
        }
    }
}

