namespace KoenZomers.OneDrive.AuthenticatorApp
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.AuthenticationBrowser = new System.Windows.Forms.WebBrowser();
            this.Step1Button = new System.Windows.Forms.Button();
            this.CurrentUrlTextBox = new System.Windows.Forms.TextBox();
            this.CurrentUrlLabel = new System.Windows.Forms.Label();
            this.AuthorizationCodeLabel = new System.Windows.Forms.Label();
            this.AuthorizationCodeTextBox = new System.Windows.Forms.TextBox();
            this.AccessTokenLabel = new System.Windows.Forms.Label();
            this.AccessTokenTextBox = new System.Windows.Forms.TextBox();
            this.JsonResultTextBox = new System.Windows.Forms.TextBox();
            this.RefreshTokenButton = new System.Windows.Forms.Button();
            this.RefreshTokenLabel = new System.Windows.Forms.Label();
            this.RefreshTokenTextBox = new System.Windows.Forms.TextBox();
            this.AccessTokenValidLabel = new System.Windows.Forms.Label();
            this.AccessTokenValidTextBox = new System.Windows.Forms.TextBox();
            this.OneDriveCommandsPanel = new System.Windows.Forms.Panel();
            this.GetOtherDriveButton = new System.Windows.Forms.Button();
            this.GetChildrenFromOtherDriveButton = new System.Windows.Forms.Button();
            this.GetFilesInFolderInAppFolderButton = new System.Windows.Forms.Button();
            this.DownloadFromAppFolderButton = new System.Windows.Forms.Button();
            this.UploadToAppFolderButton = new System.Windows.Forms.Button();
            this.AppFolderCreateFolderButton = new System.Windows.Forms.Button();
            this.GetAppFolderFilesButton = new System.Windows.Forms.Button();
            this.GetAppFolderMetadataButton = new System.Windows.Forms.Button();
            this.RemovePermissionsButton = new System.Windows.Forms.Button();
            this.ChangePermissionButton = new System.Windows.Forms.Button();
            this.AddPermissionButton = new System.Windows.Forms.Button();
            this.GetPermissionsButton = new System.Windows.Forms.Button();
            this.RootSiteButton = new System.Windows.Forms.Button();
            this.SharedWithMeButton = new System.Windows.Forms.Button();
            this.RenameButton = new System.Windows.Forms.Button();
            this.MoveButton = new System.Windows.Forms.Button();
            this.CopyButton = new System.Windows.Forms.Button();
            this.DownloadToButton = new System.Windows.Forms.Button();
            this.ShareButton = new System.Windows.Forms.Button();
            this.CreateFolderButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.SearchButton = new System.Windows.Forms.Button();
            this.DownloadButton = new System.Windows.Forms.Button();
            this.GetByIdButton = new System.Windows.Forms.Button();
            this.GetByPathButton = new System.Windows.Forms.Button();
            this.UploadButton = new System.Windows.Forms.Button();
            this.GetPublicButton = new System.Windows.Forms.Button();
            this.GetPhotos = new System.Windows.Forms.Button();
            this.GetCameraRollButton = new System.Windows.Forms.Button();
            this.GetDocumentsButton = new System.Windows.Forms.Button();
            this.GetRootChildren = new System.Windows.Forms.Button();
            this.GetRoodFolderButton = new System.Windows.Forms.Button();
            this.GetDriveButton = new System.Windows.Forms.Button();
            this.UseProxyCheckBox = new System.Windows.Forms.CheckBox();
            this.OneDriveTypeCombo = new System.Windows.Forms.ComboBox();
            this.OneDriveCommandsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // AuthenticationBrowser
            // 
            this.AuthenticationBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AuthenticationBrowser.Location = new System.Drawing.Point(14, 252);
            this.AuthenticationBrowser.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AuthenticationBrowser.MinimumSize = new System.Drawing.Size(22, 25);
            this.AuthenticationBrowser.Name = "AuthenticationBrowser";
            this.AuthenticationBrowser.ScriptErrorsSuppressed = true;
            this.AuthenticationBrowser.Size = new System.Drawing.Size(1588, 380);
            this.AuthenticationBrowser.TabIndex = 0;
            this.AuthenticationBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.AuthenticationBrowser_Navigated);
            // 
            // Step1Button
            // 
            this.Step1Button.Location = new System.Drawing.Point(14, 182);
            this.Step1Button.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Step1Button.Name = "Step1Button";
            this.Step1Button.Size = new System.Drawing.Size(120, 51);
            this.Step1Button.TabIndex = 1;
            this.Step1Button.Text = "Authorize";
            this.Step1Button.UseVisualStyleBackColor = true;
            this.Step1Button.Click += new System.EventHandler(this.Step1Button_Click);
            // 
            // CurrentUrlTextBox
            // 
            this.CurrentUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CurrentUrlTextBox.Location = new System.Drawing.Point(14, 671);
            this.CurrentUrlTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CurrentUrlTextBox.Name = "CurrentUrlTextBox";
            this.CurrentUrlTextBox.Size = new System.Drawing.Size(1588, 26);
            this.CurrentUrlTextBox.TabIndex = 4;
            // 
            // CurrentUrlLabel
            // 
            this.CurrentUrlLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CurrentUrlLabel.AutoSize = true;
            this.CurrentUrlLabel.Location = new System.Drawing.Point(15, 646);
            this.CurrentUrlLabel.Name = "CurrentUrlLabel";
            this.CurrentUrlLabel.Size = new System.Drawing.Size(99, 20);
            this.CurrentUrlLabel.TabIndex = 5;
            this.CurrentUrlLabel.Text = "Current URL";
            // 
            // AuthorizationCodeLabel
            // 
            this.AuthorizationCodeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AuthorizationCodeLabel.AutoSize = true;
            this.AuthorizationCodeLabel.Location = new System.Drawing.Point(14, 711);
            this.AuthorizationCodeLabel.Name = "AuthorizationCodeLabel";
            this.AuthorizationCodeLabel.Size = new System.Drawing.Size(145, 20);
            this.AuthorizationCodeLabel.TabIndex = 7;
            this.AuthorizationCodeLabel.Text = "Authorization Code";
            // 
            // AuthorizationCodeTextBox
            // 
            this.AuthorizationCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AuthorizationCodeTextBox.Location = new System.Drawing.Point(12, 734);
            this.AuthorizationCodeTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AuthorizationCodeTextBox.Name = "AuthorizationCodeTextBox";
            this.AuthorizationCodeTextBox.Size = new System.Drawing.Size(1588, 26);
            this.AuthorizationCodeTextBox.TabIndex = 6;
            // 
            // AccessTokenLabel
            // 
            this.AccessTokenLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AccessTokenLabel.AutoSize = true;
            this.AccessTokenLabel.Location = new System.Drawing.Point(14, 878);
            this.AccessTokenLabel.Name = "AccessTokenLabel";
            this.AccessTokenLabel.Size = new System.Drawing.Size(109, 20);
            this.AccessTokenLabel.TabIndex = 9;
            this.AccessTokenLabel.Text = "Access Token";
            // 
            // AccessTokenTextBox
            // 
            this.AccessTokenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AccessTokenTextBox.Location = new System.Drawing.Point(12, 901);
            this.AccessTokenTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AccessTokenTextBox.Name = "AccessTokenTextBox";
            this.AccessTokenTextBox.Size = new System.Drawing.Size(1588, 26);
            this.AccessTokenTextBox.TabIndex = 8;
            this.AccessTokenTextBox.TextChanged += new System.EventHandler(this.AccessTokenTextBox_TextChanged);
            // 
            // JsonResultTextBox
            // 
            this.JsonResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.JsonResultTextBox.Location = new System.Drawing.Point(14, 252);
            this.JsonResultTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.JsonResultTextBox.Multiline = true;
            this.JsonResultTextBox.Name = "JsonResultTextBox";
            this.JsonResultTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.JsonResultTextBox.Size = new System.Drawing.Size(1587, 376);
            this.JsonResultTextBox.TabIndex = 10;
            this.JsonResultTextBox.Visible = false;
            // 
            // RefreshTokenButton
            // 
            this.RefreshTokenButton.Location = new System.Drawing.Point(140, 182);
            this.RefreshTokenButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RefreshTokenButton.Name = "RefreshTokenButton";
            this.RefreshTokenButton.Size = new System.Drawing.Size(120, 51);
            this.RefreshTokenButton.TabIndex = 12;
            this.RefreshTokenButton.Text = "Refresh";
            this.RefreshTokenButton.UseVisualStyleBackColor = true;
            this.RefreshTokenButton.Click += new System.EventHandler(this.RefreshTokenButton_Click);
            // 
            // RefreshTokenLabel
            // 
            this.RefreshTokenLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RefreshTokenLabel.AutoSize = true;
            this.RefreshTokenLabel.Location = new System.Drawing.Point(15, 768);
            this.RefreshTokenLabel.Name = "RefreshTokenLabel";
            this.RefreshTokenLabel.Size = new System.Drawing.Size(114, 20);
            this.RefreshTokenLabel.TabIndex = 14;
            this.RefreshTokenLabel.Text = "Refresh Token";
            // 
            // RefreshTokenTextBox
            // 
            this.RefreshTokenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RefreshTokenTextBox.Location = new System.Drawing.Point(14, 791);
            this.RefreshTokenTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RefreshTokenTextBox.Name = "RefreshTokenTextBox";
            this.RefreshTokenTextBox.Size = new System.Drawing.Size(1588, 26);
            this.RefreshTokenTextBox.TabIndex = 13;
            // 
            // AccessTokenValidLabel
            // 
            this.AccessTokenValidLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AccessTokenValidLabel.AutoSize = true;
            this.AccessTokenValidLabel.Location = new System.Drawing.Point(14, 822);
            this.AccessTokenValidLabel.Name = "AccessTokenValidLabel";
            this.AccessTokenValidLabel.Size = new System.Drawing.Size(170, 20);
            this.AccessTokenValidLabel.TabIndex = 16;
            this.AccessTokenValidLabel.Text = "Access Token Valid Till";
            // 
            // AccessTokenValidTextBox
            // 
            this.AccessTokenValidTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AccessTokenValidTextBox.Location = new System.Drawing.Point(12, 846);
            this.AccessTokenValidTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AccessTokenValidTextBox.Name = "AccessTokenValidTextBox";
            this.AccessTokenValidTextBox.Size = new System.Drawing.Size(1588, 26);
            this.AccessTokenValidTextBox.TabIndex = 15;
            // 
            // OneDriveCommandsPanel
            // 
            this.OneDriveCommandsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OneDriveCommandsPanel.Controls.Add(this.GetOtherDriveButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetChildrenFromOtherDriveButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetFilesInFolderInAppFolderButton);
            this.OneDriveCommandsPanel.Controls.Add(this.DownloadFromAppFolderButton);
            this.OneDriveCommandsPanel.Controls.Add(this.UploadToAppFolderButton);
            this.OneDriveCommandsPanel.Controls.Add(this.AppFolderCreateFolderButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetAppFolderFilesButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetAppFolderMetadataButton);
            this.OneDriveCommandsPanel.Controls.Add(this.RemovePermissionsButton);
            this.OneDriveCommandsPanel.Controls.Add(this.ChangePermissionButton);
            this.OneDriveCommandsPanel.Controls.Add(this.AddPermissionButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetPermissionsButton);
            this.OneDriveCommandsPanel.Controls.Add(this.RootSiteButton);
            this.OneDriveCommandsPanel.Controls.Add(this.SharedWithMeButton);
            this.OneDriveCommandsPanel.Controls.Add(this.RenameButton);
            this.OneDriveCommandsPanel.Controls.Add(this.MoveButton);
            this.OneDriveCommandsPanel.Controls.Add(this.CopyButton);
            this.OneDriveCommandsPanel.Controls.Add(this.DownloadToButton);
            this.OneDriveCommandsPanel.Controls.Add(this.ShareButton);
            this.OneDriveCommandsPanel.Controls.Add(this.CreateFolderButton);
            this.OneDriveCommandsPanel.Controls.Add(this.DeleteButton);
            this.OneDriveCommandsPanel.Controls.Add(this.SearchButton);
            this.OneDriveCommandsPanel.Controls.Add(this.DownloadButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetByIdButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetByPathButton);
            this.OneDriveCommandsPanel.Controls.Add(this.UploadButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetPublicButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetPhotos);
            this.OneDriveCommandsPanel.Controls.Add(this.GetCameraRollButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetDocumentsButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetRootChildren);
            this.OneDriveCommandsPanel.Controls.Add(this.GetRoodFolderButton);
            this.OneDriveCommandsPanel.Controls.Add(this.GetDriveButton);
            this.OneDriveCommandsPanel.Enabled = false;
            this.OneDriveCommandsPanel.Location = new System.Drawing.Point(313, 11);
            this.OneDriveCommandsPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.OneDriveCommandsPanel.Name = "OneDriveCommandsPanel";
            this.OneDriveCommandsPanel.Size = new System.Drawing.Size(1289, 238);
            this.OneDriveCommandsPanel.TabIndex = 17;
            // 
            // GetOtherDriveButton
            // 
            this.GetOtherDriveButton.Location = new System.Drawing.Point(890, 170);
            this.GetOtherDriveButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetOtherDriveButton.Name = "GetOtherDriveButton";
            this.GetOtherDriveButton.Size = new System.Drawing.Size(120, 51);
            this.GetOtherDriveButton.TabIndex = 44;
            this.GetOtherDriveButton.Text = "Other Drive";
            this.GetOtherDriveButton.UseVisualStyleBackColor = true;
            this.GetOtherDriveButton.Click += new System.EventHandler(this.GetOtherDriveButton_Click);
            // 
            // GetChildrenFromOtherDriveButton
            // 
            this.GetChildrenFromOtherDriveButton.Location = new System.Drawing.Point(890, 118);
            this.GetChildrenFromOtherDriveButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetChildrenFromOtherDriveButton.Name = "GetChildrenFromOtherDriveButton";
            this.GetChildrenFromOtherDriveButton.Size = new System.Drawing.Size(120, 51);
            this.GetChildrenFromOtherDriveButton.TabIndex = 43;
            this.GetChildrenFromOtherDriveButton.Text = "Items o/share";
            this.GetChildrenFromOtherDriveButton.UseVisualStyleBackColor = true;
            this.GetChildrenFromOtherDriveButton.Click += new System.EventHandler(this.GetChildrenFromOtherDriveButton_Click);
            // 
            // GetFilesInFolderInAppFolderButton
            // 
            this.GetFilesInFolderInAppFolderButton.Location = new System.Drawing.Point(512, 172);
            this.GetFilesInFolderInAppFolderButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetFilesInFolderInAppFolderButton.Name = "GetFilesInFolderInAppFolderButton";
            this.GetFilesInFolderInAppFolderButton.Size = new System.Drawing.Size(120, 51);
            this.GetFilesInFolderInAppFolderButton.TabIndex = 42;
            this.GetFilesInFolderInAppFolderButton.Text = "AppFl. Fil in F";
            this.GetFilesInFolderInAppFolderButton.UseVisualStyleBackColor = true;
            this.GetFilesInFolderInAppFolderButton.Click += new System.EventHandler(this.GetFilesInFolderInAppFolderButton_Click);
            // 
            // DownloadFromAppFolderButton
            // 
            this.DownloadFromAppFolderButton.Location = new System.Drawing.Point(638, 61);
            this.DownloadFromAppFolderButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.DownloadFromAppFolderButton.Name = "DownloadFromAppFolderButton";
            this.DownloadFromAppFolderButton.Size = new System.Drawing.Size(120, 51);
            this.DownloadFromAppFolderButton.TabIndex = 41;
            this.DownloadFromAppFolderButton.Tag = "Create a new folder inside the AppFolder";
            this.DownloadFromAppFolderButton.Text = "AppFl. Downl";
            this.DownloadFromAppFolderButton.UseVisualStyleBackColor = true;
            this.DownloadFromAppFolderButton.Click += new System.EventHandler(this.DownloadFromAppFolderButton_Click);
            // 
            // UploadToAppFolderButton
            // 
            this.UploadToAppFolderButton.Location = new System.Drawing.Point(638, 5);
            this.UploadToAppFolderButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.UploadToAppFolderButton.Name = "UploadToAppFolderButton";
            this.UploadToAppFolderButton.Size = new System.Drawing.Size(120, 51);
            this.UploadToAppFolderButton.TabIndex = 40;
            this.UploadToAppFolderButton.Tag = "Create a new folder inside the AppFolder";
            this.UploadToAppFolderButton.Text = "AppFol. Upl.";
            this.UploadToAppFolderButton.UseVisualStyleBackColor = true;
            this.UploadToAppFolderButton.Click += new System.EventHandler(this.UploadToAppFolderButton_Click);
            // 
            // AppFolderCreateFolderButton
            // 
            this.AppFolderCreateFolderButton.Location = new System.Drawing.Point(512, 118);
            this.AppFolderCreateFolderButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AppFolderCreateFolderButton.Name = "AppFolderCreateFolderButton";
            this.AppFolderCreateFolderButton.Size = new System.Drawing.Size(120, 51);
            this.AppFolderCreateFolderButton.TabIndex = 39;
            this.AppFolderCreateFolderButton.Tag = "";
            this.AppFolderCreateFolderButton.Text = "AppFl. N Fold";
            this.AppFolderCreateFolderButton.UseVisualStyleBackColor = true;
            this.AppFolderCreateFolderButton.Click += new System.EventHandler(this.AppFolderCreateFolderButton_Click);
            // 
            // GetAppFolderFilesButton
            // 
            this.GetAppFolderFilesButton.Location = new System.Drawing.Point(512, 61);
            this.GetAppFolderFilesButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetAppFolderFilesButton.Name = "GetAppFolderFilesButton";
            this.GetAppFolderFilesButton.Size = new System.Drawing.Size(120, 51);
            this.GetAppFolderFilesButton.TabIndex = 38;
            this.GetAppFolderFilesButton.Text = "AppFol. Files";
            this.GetAppFolderFilesButton.UseVisualStyleBackColor = true;
            this.GetAppFolderFilesButton.Click += new System.EventHandler(this.GetAppFolderFilesButton_Click);
            // 
            // GetAppFolderMetadataButton
            // 
            this.GetAppFolderMetadataButton.Location = new System.Drawing.Point(512, 5);
            this.GetAppFolderMetadataButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetAppFolderMetadataButton.Name = "GetAppFolderMetadataButton";
            this.GetAppFolderMetadataButton.Size = new System.Drawing.Size(120, 51);
            this.GetAppFolderMetadataButton.TabIndex = 37;
            this.GetAppFolderMetadataButton.Text = "AppFol. Meta";
            this.GetAppFolderMetadataButton.UseVisualStyleBackColor = true;
            this.GetAppFolderMetadataButton.Click += new System.EventHandler(this.GetAppFolderMetadataButton_Click);
            // 
            // RemovePermissionsButton
            // 
            this.RemovePermissionsButton.Location = new System.Drawing.Point(1143, 115);
            this.RemovePermissionsButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RemovePermissionsButton.Name = "RemovePermissionsButton";
            this.RemovePermissionsButton.Size = new System.Drawing.Size(120, 51);
            this.RemovePermissionsButton.TabIndex = 36;
            this.RemovePermissionsButton.Text = "Rem. permis.";
            this.RemovePermissionsButton.UseVisualStyleBackColor = true;
            this.RemovePermissionsButton.Click += new System.EventHandler(this.RemovePermissionsButton_Click);
            // 
            // ChangePermissionButton
            // 
            this.ChangePermissionButton.Location = new System.Drawing.Point(1143, 172);
            this.ChangePermissionButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ChangePermissionButton.Name = "ChangePermissionButton";
            this.ChangePermissionButton.Size = new System.Drawing.Size(120, 51);
            this.ChangePermissionButton.TabIndex = 35;
            this.ChangePermissionButton.Text = "Ch. permis.";
            this.ChangePermissionButton.UseVisualStyleBackColor = true;
            this.ChangePermissionButton.Click += new System.EventHandler(this.ChangePermissionButton_Click);
            // 
            // AddPermissionButton
            // 
            this.AddPermissionButton.Location = new System.Drawing.Point(1143, 60);
            this.AddPermissionButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.AddPermissionButton.Name = "AddPermissionButton";
            this.AddPermissionButton.Size = new System.Drawing.Size(120, 51);
            this.AddPermissionButton.TabIndex = 34;
            this.AddPermissionButton.Text = "Add permis.";
            this.AddPermissionButton.UseVisualStyleBackColor = true;
            this.AddPermissionButton.Click += new System.EventHandler(this.AddPermissionButton_Click);
            // 
            // GetPermissionsButton
            // 
            this.GetPermissionsButton.Location = new System.Drawing.Point(1143, 5);
            this.GetPermissionsButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetPermissionsButton.Name = "GetPermissionsButton";
            this.GetPermissionsButton.Size = new System.Drawing.Size(120, 51);
            this.GetPermissionsButton.TabIndex = 33;
            this.GetPermissionsButton.Text = "Get permis.";
            this.GetPermissionsButton.UseVisualStyleBackColor = true;
            this.GetPermissionsButton.Click += new System.EventHandler(this.GetPermissionsButton_Click);
            // 
            // RootSiteButton
            // 
            this.RootSiteButton.Location = new System.Drawing.Point(1017, 5);
            this.RootSiteButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RootSiteButton.Name = "RootSiteButton";
            this.RootSiteButton.Size = new System.Drawing.Size(120, 51);
            this.RootSiteButton.TabIndex = 32;
            this.RootSiteButton.Text = "Root Site";
            this.RootSiteButton.UseVisualStyleBackColor = true;
            this.RootSiteButton.Click += new System.EventHandler(this.RootSiteButton_Click);
            // 
            // SharedWithMeButton
            // 
            this.SharedWithMeButton.Location = new System.Drawing.Point(891, 5);
            this.SharedWithMeButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SharedWithMeButton.Name = "SharedWithMeButton";
            this.SharedWithMeButton.Size = new System.Drawing.Size(120, 51);
            this.SharedWithMeButton.TabIndex = 31;
            this.SharedWithMeButton.Text = "Shared w/me";
            this.SharedWithMeButton.UseVisualStyleBackColor = true;
            this.SharedWithMeButton.Click += new System.EventHandler(this.SharedWithMeButton_Click);
            // 
            // RenameButton
            // 
            this.RenameButton.Location = new System.Drawing.Point(764, 170);
            this.RenameButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.RenameButton.Name = "RenameButton";
            this.RenameButton.Size = new System.Drawing.Size(120, 51);
            this.RenameButton.TabIndex = 30;
            this.RenameButton.Text = "Rename";
            this.RenameButton.UseVisualStyleBackColor = true;
            this.RenameButton.Click += new System.EventHandler(this.RenameButton_Click);
            // 
            // MoveButton
            // 
            this.MoveButton.Location = new System.Drawing.Point(764, 5);
            this.MoveButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MoveButton.Name = "MoveButton";
            this.MoveButton.Size = new System.Drawing.Size(120, 51);
            this.MoveButton.TabIndex = 29;
            this.MoveButton.Text = "Move";
            this.MoveButton.UseVisualStyleBackColor = true;
            this.MoveButton.Click += new System.EventHandler(this.MoveButton_Click);
            // 
            // CopyButton
            // 
            this.CopyButton.Location = new System.Drawing.Point(764, 61);
            this.CopyButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CopyButton.Name = "CopyButton";
            this.CopyButton.Size = new System.Drawing.Size(120, 51);
            this.CopyButton.TabIndex = 28;
            this.CopyButton.Text = "Copy";
            this.CopyButton.UseVisualStyleBackColor = true;
            this.CopyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // DownloadToButton
            // 
            this.DownloadToButton.Location = new System.Drawing.Point(386, 61);
            this.DownloadToButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.DownloadToButton.Name = "DownloadToButton";
            this.DownloadToButton.Size = new System.Drawing.Size(120, 51);
            this.DownloadToButton.TabIndex = 27;
            this.DownloadToButton.Text = "Download To";
            this.DownloadToButton.UseVisualStyleBackColor = true;
            this.DownloadToButton.Click += new System.EventHandler(this.DownloadToButton_Click);
            // 
            // ShareButton
            // 
            this.ShareButton.Location = new System.Drawing.Point(890, 61);
            this.ShareButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ShareButton.Name = "ShareButton";
            this.ShareButton.Size = new System.Drawing.Size(120, 51);
            this.ShareButton.TabIndex = 26;
            this.ShareButton.Text = "Share Item";
            this.ShareButton.UseVisualStyleBackColor = true;
            this.ShareButton.Click += new System.EventHandler(this.ShareButton_Click);
            // 
            // CreateFolderButton
            // 
            this.CreateFolderButton.Location = new System.Drawing.Point(386, 172);
            this.CreateFolderButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CreateFolderButton.Name = "CreateFolderButton";
            this.CreateFolderButton.Size = new System.Drawing.Size(120, 51);
            this.CreateFolderButton.TabIndex = 25;
            this.CreateFolderButton.Text = "Create Folder";
            this.CreateFolderButton.UseVisualStyleBackColor = true;
            this.CreateFolderButton.Click += new System.EventHandler(this.CreateFolderButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(764, 116);
            this.DeleteButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(120, 51);
            this.DeleteButton.TabIndex = 24;
            this.DeleteButton.Text = "Delete";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // SearchButton
            // 
            this.SearchButton.Location = new System.Drawing.Point(8, 172);
            this.SearchButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(120, 51);
            this.SearchButton.TabIndex = 23;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // DownloadButton
            // 
            this.DownloadButton.Location = new System.Drawing.Point(386, 5);
            this.DownloadButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(120, 51);
            this.DownloadButton.TabIndex = 22;
            this.DownloadButton.Text = "Download Str";
            this.DownloadButton.UseVisualStyleBackColor = true;
            this.DownloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
            // 
            // GetByIdButton
            // 
            this.GetByIdButton.Location = new System.Drawing.Point(8, 115);
            this.GetByIdButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetByIdButton.Name = "GetByIdButton";
            this.GetByIdButton.Size = new System.Drawing.Size(120, 51);
            this.GetByIdButton.TabIndex = 21;
            this.GetByIdButton.Text = "Get By Id";
            this.GetByIdButton.UseVisualStyleBackColor = true;
            this.GetByIdButton.Click += new System.EventHandler(this.GetByIdButton_Click);
            // 
            // GetByPathButton
            // 
            this.GetByPathButton.Location = new System.Drawing.Point(8, 61);
            this.GetByPathButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetByPathButton.Name = "GetByPathButton";
            this.GetByPathButton.Size = new System.Drawing.Size(120, 51);
            this.GetByPathButton.TabIndex = 20;
            this.GetByPathButton.Text = "Get by Path";
            this.GetByPathButton.UseVisualStyleBackColor = true;
            this.GetByPathButton.Click += new System.EventHandler(this.GetByPathButton_Click);
            // 
            // UploadButton
            // 
            this.UploadButton.Location = new System.Drawing.Point(386, 115);
            this.UploadButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(120, 51);
            this.UploadButton.TabIndex = 19;
            this.UploadButton.Text = "Upload";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // GetPublicButton
            // 
            this.GetPublicButton.Location = new System.Drawing.Point(260, 5);
            this.GetPublicButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetPublicButton.Name = "GetPublicButton";
            this.GetPublicButton.Size = new System.Drawing.Size(120, 51);
            this.GetPublicButton.TabIndex = 18;
            this.GetPublicButton.Text = "Public";
            this.GetPublicButton.UseVisualStyleBackColor = true;
            this.GetPublicButton.Click += new System.EventHandler(this.GetPublicButton_Click);
            // 
            // GetPhotos
            // 
            this.GetPhotos.Location = new System.Drawing.Point(260, 60);
            this.GetPhotos.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetPhotos.Name = "GetPhotos";
            this.GetPhotos.Size = new System.Drawing.Size(120, 51);
            this.GetPhotos.TabIndex = 17;
            this.GetPhotos.Text = "Photos";
            this.GetPhotos.UseVisualStyleBackColor = true;
            this.GetPhotos.Click += new System.EventHandler(this.GetPhotos_Click);
            // 
            // GetCameraRollButton
            // 
            this.GetCameraRollButton.Location = new System.Drawing.Point(260, 115);
            this.GetCameraRollButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetCameraRollButton.Name = "GetCameraRollButton";
            this.GetCameraRollButton.Size = new System.Drawing.Size(120, 51);
            this.GetCameraRollButton.TabIndex = 16;
            this.GetCameraRollButton.Text = "Camera Roll";
            this.GetCameraRollButton.UseVisualStyleBackColor = true;
            this.GetCameraRollButton.Click += new System.EventHandler(this.GetCameraRollButton_Click);
            // 
            // GetDocumentsButton
            // 
            this.GetDocumentsButton.Location = new System.Drawing.Point(260, 170);
            this.GetDocumentsButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetDocumentsButton.Name = "GetDocumentsButton";
            this.GetDocumentsButton.Size = new System.Drawing.Size(120, 51);
            this.GetDocumentsButton.TabIndex = 15;
            this.GetDocumentsButton.Text = "Documents";
            this.GetDocumentsButton.UseVisualStyleBackColor = true;
            this.GetDocumentsButton.Click += new System.EventHandler(this.GetDocumentsButton_Click);
            // 
            // GetRootChildren
            // 
            this.GetRootChildren.Location = new System.Drawing.Point(134, 5);
            this.GetRootChildren.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetRootChildren.Name = "GetRootChildren";
            this.GetRootChildren.Size = new System.Drawing.Size(120, 51);
            this.GetRootChildren.TabIndex = 14;
            this.GetRootChildren.Text = "Root Children";
            this.GetRootChildren.UseVisualStyleBackColor = true;
            this.GetRootChildren.Click += new System.EventHandler(this.GetRootChildren_Click);
            // 
            // GetRoodFolderButton
            // 
            this.GetRoodFolderButton.Location = new System.Drawing.Point(134, 60);
            this.GetRoodFolderButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetRoodFolderButton.Name = "GetRoodFolderButton";
            this.GetRoodFolderButton.Size = new System.Drawing.Size(120, 51);
            this.GetRoodFolderButton.TabIndex = 12;
            this.GetRoodFolderButton.Text = "Root Folder";
            this.GetRoodFolderButton.UseVisualStyleBackColor = true;
            this.GetRoodFolderButton.Click += new System.EventHandler(this.GetRoodFolderButton_Click);
            // 
            // GetDriveButton
            // 
            this.GetDriveButton.Location = new System.Drawing.Point(8, 5);
            this.GetDriveButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.GetDriveButton.Name = "GetDriveButton";
            this.GetDriveButton.Size = new System.Drawing.Size(120, 51);
            this.GetDriveButton.TabIndex = 13;
            this.GetDriveButton.Text = "Drive";
            this.GetDriveButton.UseVisualStyleBackColor = true;
            this.GetDriveButton.Click += new System.EventHandler(this.GetDriveButton_Click);
            // 
            // UseProxyCheckBox
            // 
            this.UseProxyCheckBox.AutoSize = true;
            this.UseProxyCheckBox.Location = new System.Drawing.Point(19, 140);
            this.UseProxyCheckBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.UseProxyCheckBox.Name = "UseProxyCheckBox";
            this.UseProxyCheckBox.Size = new System.Drawing.Size(106, 24);
            this.UseProxyCheckBox.TabIndex = 18;
            this.UseProxyCheckBox.Text = "Use Proxy";
            this.UseProxyCheckBox.UseVisualStyleBackColor = true;
            // 
            // OneDriveTypeCombo
            // 
            this.OneDriveTypeCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OneDriveTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.OneDriveTypeCombo.FormattingEnabled = true;
            this.OneDriveTypeCombo.Items.AddRange(new object[] {
            "Consumer OneDrive",
            "OneDrive for Business O365",
            "Graph API (Consumer & Business)"});
            this.OneDriveTypeCombo.Location = new System.Drawing.Point(17, 15);
            this.OneDriveTypeCombo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.OneDriveTypeCombo.Name = "OneDriveTypeCombo";
            this.OneDriveTypeCombo.Size = new System.Drawing.Size(289, 28);
            this.OneDriveTypeCombo.TabIndex = 19;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1616, 951);
            this.Controls.Add(this.OneDriveTypeCombo);
            this.Controls.Add(this.UseProxyCheckBox);
            this.Controls.Add(this.AccessTokenValidLabel);
            this.Controls.Add(this.AccessTokenValidTextBox);
            this.Controls.Add(this.RefreshTokenLabel);
            this.Controls.Add(this.RefreshTokenTextBox);
            this.Controls.Add(this.RefreshTokenButton);
            this.Controls.Add(this.JsonResultTextBox);
            this.Controls.Add(this.AccessTokenLabel);
            this.Controls.Add(this.AccessTokenTextBox);
            this.Controls.Add(this.AuthorizationCodeLabel);
            this.Controls.Add(this.AuthorizationCodeTextBox);
            this.Controls.Add(this.CurrentUrlLabel);
            this.Controls.Add(this.CurrentUrlTextBox);
            this.Controls.Add(this.Step1Button);
            this.Controls.Add(this.AuthenticationBrowser);
            this.Controls.Add(this.OneDriveCommandsPanel);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(426, 374);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OneDrive API Test";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.OneDriveCommandsPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser AuthenticationBrowser;
        private System.Windows.Forms.Button Step1Button;
        private System.Windows.Forms.TextBox CurrentUrlTextBox;
        private System.Windows.Forms.Label CurrentUrlLabel;
        private System.Windows.Forms.Label AuthorizationCodeLabel;
        private System.Windows.Forms.TextBox AuthorizationCodeTextBox;
        private System.Windows.Forms.Label AccessTokenLabel;
        private System.Windows.Forms.TextBox AccessTokenTextBox;
        private System.Windows.Forms.TextBox JsonResultTextBox;
        private System.Windows.Forms.Button RefreshTokenButton;
        private System.Windows.Forms.Label RefreshTokenLabel;
        private System.Windows.Forms.TextBox RefreshTokenTextBox;
        private System.Windows.Forms.Label AccessTokenValidLabel;
        private System.Windows.Forms.TextBox AccessTokenValidTextBox;
        private System.Windows.Forms.Panel OneDriveCommandsPanel;
        private System.Windows.Forms.Button GetRootChildren;
        private System.Windows.Forms.Button GetRoodFolderButton;
        private System.Windows.Forms.Button GetDriveButton;
        private System.Windows.Forms.Button GetDocumentsButton;
        private System.Windows.Forms.Button GetCameraRollButton;
        private System.Windows.Forms.Button GetPhotos;
        private System.Windows.Forms.Button GetPublicButton;
        private System.Windows.Forms.Button UploadButton;
        private System.Windows.Forms.Button GetByPathButton;
        private System.Windows.Forms.Button GetByIdButton;
        private System.Windows.Forms.Button DownloadButton;
        private System.Windows.Forms.Button SearchButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button CreateFolderButton;
        private System.Windows.Forms.Button ShareButton;
        private System.Windows.Forms.Button DownloadToButton;
        private System.Windows.Forms.Button CopyButton;
        private System.Windows.Forms.Button MoveButton;
        private System.Windows.Forms.CheckBox UseProxyCheckBox;
        private System.Windows.Forms.ComboBox OneDriveTypeCombo;
        private System.Windows.Forms.Button RenameButton;
        private System.Windows.Forms.Button SharedWithMeButton;
        private System.Windows.Forms.Button RootSiteButton;
        private System.Windows.Forms.Button GetPermissionsButton;
        private System.Windows.Forms.Button AddPermissionButton;
        private System.Windows.Forms.Button ChangePermissionButton;
        private System.Windows.Forms.Button RemovePermissionsButton;
        private System.Windows.Forms.Button GetAppFolderMetadataButton;
        private System.Windows.Forms.Button GetAppFolderFilesButton;
        private System.Windows.Forms.Button AppFolderCreateFolderButton;
        private System.Windows.Forms.Button UploadToAppFolderButton;
        private System.Windows.Forms.Button DownloadFromAppFolderButton;
        private System.Windows.Forms.Button GetFilesInFolderInAppFolderButton;
        private System.Windows.Forms.Button GetChildrenFromOtherDriveButton;
        private System.Windows.Forms.Button GetOtherDriveButton;
    }
}

