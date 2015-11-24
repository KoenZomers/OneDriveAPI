using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using KoenZomers.OneDrive.Api;
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
        /// The client ID of the OneDrive API as registered with Microsoft
        /// </summary>
        public string ClientId;

        /// <summary>
        /// The client secret of the OneDrive API as registered with Microsoft
        /// </summary>
        public string ClientSecret;

        /// <summary>
        /// The refresh token stored in the App Config
        /// </summary>
        public string RefreshToken;

        #endregion

        public MainForm()
        {
            InitializeComponent();
            _configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            ClientId = _configuration.AppSettings.Settings["OneDriveApiClientID"].Value;
            ClientSecret = _configuration.AppSettings.Settings["OneDriveApiClientSecret"].Value;
            RefreshToken = _configuration.AppSettings.Settings["OneDriveApiRefreshToken"].Value;

            RefreshTokenTextBox.Text = RefreshToken;
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
            if (CurrentUrlTextBox.Text.StartsWith("https://login.live.com/oauth20_desktop.srf"))
            {
                var authenticateUri = OneDriveApi.GetAuthenticationUri("wl.offline_access wl.skydrive_update");
                AuthenticationBrowser.Navigate(authenticateUri);
            }
        }

        private void Step1Button_Click(object sender, EventArgs e)
        {
            // Create a new instance of the OneDriveApi framework
            OneDriveApi = new OneDriveApi(ClientId, ClientSecret);

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

            // Get a new access token based on the refresh token entered in the textbox
            OneDriveApi = await OneDriveApi.GetOneDriveApiFromRefreshToken(ClientId, ClientSecret, RefreshTokenTextBox.Text);

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
            JsonResultTextBox.Text = data.OriginalJson;
        }

        private async void GetRoodFolderButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDriveRoot();
            JsonResultTextBox.Text = data.OriginalJson;
        }

        private async void GetRootChildren_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDriveRootChildren();
            JsonResultTextBox.Text = data.OriginalJson;
        }

        private async void GetDocumentsButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDriveDocumentsFolder();
            JsonResultTextBox.Text = data.OriginalJson;
        }

        private async void GetCameraRollButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDriveCameraRollFolder();
            JsonResultTextBox.Text = data.OriginalJson;
        }

        private async void GetPhotos_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDrivePhotosFolder();
            JsonResultTextBox.Text = data.OriginalJson;
        }

        private async void GetPublicButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.GetDrivePublicFolder();
            JsonResultTextBox.Text = data.OriginalJson;
        }

        private async void UploadButton_Click(object sender, EventArgs e)
        {
            var fileToUpload = SelectLocalFile();
            var data = await OneDriveApi.UploadFile(fileToUpload, "");
            JsonResultTextBox.Text = data.OriginalJson;
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
            var data = await OneDriveApi.GetChildrenByPath("E-books");
            JsonResultTextBox.Text = data.OriginalJson;
        }

        private async void GetByIdButton_Click(object sender, EventArgs e)
        {
            var data1 = await OneDriveApi.GetChildrenByPath("Drivers");
            var data2 = await OneDriveApi.GetChildrenByParentItem(data1.Collection[0]);
            JsonResultTextBox.Text = data2.OriginalJson;
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
            JsonResultTextBox.Text = data.OriginalJson;
        }

        private async void ShareButton_Click(object sender, EventArgs e)
        {
            var data = await OneDriveApi.ShareItem("Test", OneDriveLinkType.Edit);
            JsonResultTextBox.Text = data.OriginalJson;
        }
    }
}

