namespace AuthenticatorApp
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
            this.OneDriveCommandsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // AuthenticationBrowser
            // 
            this.AuthenticationBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AuthenticationBrowser.Location = new System.Drawing.Point(9, 93);
            this.AuthenticationBrowser.Margin = new System.Windows.Forms.Padding(2);
            this.AuthenticationBrowser.MinimumSize = new System.Drawing.Size(15, 16);
            this.AuthenticationBrowser.Name = "AuthenticationBrowser";
            this.AuthenticationBrowser.Size = new System.Drawing.Size(762, 318);
            this.AuthenticationBrowser.TabIndex = 0;
            this.AuthenticationBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.AuthenticationBrowser_Navigated);
            // 
            // Step1Button
            // 
            this.Step1Button.Location = new System.Drawing.Point(9, 10);
            this.Step1Button.Margin = new System.Windows.Forms.Padding(2);
            this.Step1Button.Name = "Step1Button";
            this.Step1Button.Size = new System.Drawing.Size(80, 33);
            this.Step1Button.TabIndex = 1;
            this.Step1Button.Text = "Authorize";
            this.Step1Button.UseVisualStyleBackColor = true;
            this.Step1Button.Click += new System.EventHandler(this.Step1Button_Click);
            // 
            // CurrentUrlTextBox
            // 
            this.CurrentUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CurrentUrlTextBox.Location = new System.Drawing.Point(9, 436);
            this.CurrentUrlTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.CurrentUrlTextBox.Name = "CurrentUrlTextBox";
            this.CurrentUrlTextBox.Size = new System.Drawing.Size(763, 20);
            this.CurrentUrlTextBox.TabIndex = 4;
            // 
            // CurrentUrlLabel
            // 
            this.CurrentUrlLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CurrentUrlLabel.AutoSize = true;
            this.CurrentUrlLabel.Location = new System.Drawing.Point(10, 420);
            this.CurrentUrlLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.CurrentUrlLabel.Name = "CurrentUrlLabel";
            this.CurrentUrlLabel.Size = new System.Drawing.Size(66, 13);
            this.CurrentUrlLabel.TabIndex = 5;
            this.CurrentUrlLabel.Text = "Current URL";
            // 
            // AuthorizationCodeLabel
            // 
            this.AuthorizationCodeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AuthorizationCodeLabel.AutoSize = true;
            this.AuthorizationCodeLabel.Location = new System.Drawing.Point(9, 462);
            this.AuthorizationCodeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.AuthorizationCodeLabel.Name = "AuthorizationCodeLabel";
            this.AuthorizationCodeLabel.Size = new System.Drawing.Size(96, 13);
            this.AuthorizationCodeLabel.TabIndex = 7;
            this.AuthorizationCodeLabel.Text = "Authorization Code";
            // 
            // AuthorizationCodeTextBox
            // 
            this.AuthorizationCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AuthorizationCodeTextBox.Location = new System.Drawing.Point(8, 477);
            this.AuthorizationCodeTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.AuthorizationCodeTextBox.Name = "AuthorizationCodeTextBox";
            this.AuthorizationCodeTextBox.Size = new System.Drawing.Size(763, 20);
            this.AuthorizationCodeTextBox.TabIndex = 6;
            // 
            // AccessTokenLabel
            // 
            this.AccessTokenLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AccessTokenLabel.AutoSize = true;
            this.AccessTokenLabel.Location = new System.Drawing.Point(9, 570);
            this.AccessTokenLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.AccessTokenLabel.Name = "AccessTokenLabel";
            this.AccessTokenLabel.Size = new System.Drawing.Size(76, 13);
            this.AccessTokenLabel.TabIndex = 9;
            this.AccessTokenLabel.Text = "Access Token";
            // 
            // AccessTokenTextBox
            // 
            this.AccessTokenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AccessTokenTextBox.Location = new System.Drawing.Point(8, 586);
            this.AccessTokenTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.AccessTokenTextBox.Name = "AccessTokenTextBox";
            this.AccessTokenTextBox.Size = new System.Drawing.Size(763, 20);
            this.AccessTokenTextBox.TabIndex = 8;
            this.AccessTokenTextBox.TextChanged += new System.EventHandler(this.AccessTokenTextBox_TextChanged);
            // 
            // JsonResultTextBox
            // 
            this.JsonResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.JsonResultTextBox.Location = new System.Drawing.Point(9, 93);
            this.JsonResultTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.JsonResultTextBox.Multiline = true;
            this.JsonResultTextBox.Name = "JsonResultTextBox";
            this.JsonResultTextBox.Size = new System.Drawing.Size(762, 318);
            this.JsonResultTextBox.TabIndex = 10;
            this.JsonResultTextBox.Visible = false;
            // 
            // RefreshTokenButton
            // 
            this.RefreshTokenButton.Location = new System.Drawing.Point(8, 48);
            this.RefreshTokenButton.Margin = new System.Windows.Forms.Padding(2);
            this.RefreshTokenButton.Name = "RefreshTokenButton";
            this.RefreshTokenButton.Size = new System.Drawing.Size(80, 33);
            this.RefreshTokenButton.TabIndex = 12;
            this.RefreshTokenButton.Text = "Refresh";
            this.RefreshTokenButton.UseVisualStyleBackColor = true;
            this.RefreshTokenButton.Click += new System.EventHandler(this.RefreshTokenButton_Click);
            // 
            // RefreshTokenLabel
            // 
            this.RefreshTokenLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RefreshTokenLabel.AutoSize = true;
            this.RefreshTokenLabel.Location = new System.Drawing.Point(10, 499);
            this.RefreshTokenLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.RefreshTokenLabel.Name = "RefreshTokenLabel";
            this.RefreshTokenLabel.Size = new System.Drawing.Size(78, 13);
            this.RefreshTokenLabel.TabIndex = 14;
            this.RefreshTokenLabel.Text = "Refresh Token";
            // 
            // RefreshTokenTextBox
            // 
            this.RefreshTokenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RefreshTokenTextBox.Location = new System.Drawing.Point(9, 514);
            this.RefreshTokenTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.RefreshTokenTextBox.Name = "RefreshTokenTextBox";
            this.RefreshTokenTextBox.Size = new System.Drawing.Size(763, 20);
            this.RefreshTokenTextBox.TabIndex = 13;
            // 
            // AccessTokenValidLabel
            // 
            this.AccessTokenValidLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AccessTokenValidLabel.AutoSize = true;
            this.AccessTokenValidLabel.Location = new System.Drawing.Point(9, 535);
            this.AccessTokenValidLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.AccessTokenValidLabel.Name = "AccessTokenValidLabel";
            this.AccessTokenValidLabel.Size = new System.Drawing.Size(118, 13);
            this.AccessTokenValidLabel.TabIndex = 16;
            this.AccessTokenValidLabel.Text = "Access Token Valid Till";
            // 
            // AccessTokenValidTextBox
            // 
            this.AccessTokenValidTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AccessTokenValidTextBox.Location = new System.Drawing.Point(8, 550);
            this.AccessTokenValidTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.AccessTokenValidTextBox.Name = "AccessTokenValidTextBox";
            this.AccessTokenValidTextBox.Size = new System.Drawing.Size(763, 20);
            this.AccessTokenValidTextBox.TabIndex = 15;
            // 
            // OneDriveCommandsPanel
            // 
            this.OneDriveCommandsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.OneDriveCommandsPanel.Location = new System.Drawing.Point(93, 9);
            this.OneDriveCommandsPanel.Margin = new System.Windows.Forms.Padding(2);
            this.OneDriveCommandsPanel.Name = "OneDriveCommandsPanel";
            this.OneDriveCommandsPanel.Size = new System.Drawing.Size(677, 78);
            this.OneDriveCommandsPanel.TabIndex = 17;
            // 
            // ShareButton
            // 
            this.ShareButton.Location = new System.Drawing.Point(590, 2);
            this.ShareButton.Margin = new System.Windows.Forms.Padding(2);
            this.ShareButton.Name = "ShareButton";
            this.ShareButton.Size = new System.Drawing.Size(80, 33);
            this.ShareButton.TabIndex = 26;
            this.ShareButton.Text = "Share Item";
            this.ShareButton.UseVisualStyleBackColor = true;
            this.ShareButton.Click += new System.EventHandler(this.ShareButton_Click);
            // 
            // CreateFolderButton
            // 
            this.CreateFolderButton.Location = new System.Drawing.Point(506, 38);
            this.CreateFolderButton.Margin = new System.Windows.Forms.Padding(2);
            this.CreateFolderButton.Name = "CreateFolderButton";
            this.CreateFolderButton.Size = new System.Drawing.Size(80, 33);
            this.CreateFolderButton.TabIndex = 25;
            this.CreateFolderButton.Text = "Create Folder";
            this.CreateFolderButton.UseVisualStyleBackColor = true;
            this.CreateFolderButton.Click += new System.EventHandler(this.CreateFolderButton_Click);
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(506, 2);
            this.DeleteButton.Margin = new System.Windows.Forms.Padding(2);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(80, 33);
            this.DeleteButton.TabIndex = 24;
            this.DeleteButton.Text = "Delete";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // SearchButton
            // 
            this.SearchButton.Location = new System.Drawing.Point(422, 38);
            this.SearchButton.Margin = new System.Windows.Forms.Padding(2);
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(80, 33);
            this.SearchButton.TabIndex = 23;
            this.SearchButton.Text = "Search";
            this.SearchButton.UseVisualStyleBackColor = true;
            this.SearchButton.Click += new System.EventHandler(this.SearchButton_Click);
            // 
            // DownloadButton
            // 
            this.DownloadButton.Location = new System.Drawing.Point(422, 2);
            this.DownloadButton.Margin = new System.Windows.Forms.Padding(2);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(80, 33);
            this.DownloadButton.TabIndex = 22;
            this.DownloadButton.Text = "Download";
            this.DownloadButton.UseVisualStyleBackColor = true;
            this.DownloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
            // 
            // GetByIdButton
            // 
            this.GetByIdButton.Location = new System.Drawing.Point(338, 39);
            this.GetByIdButton.Margin = new System.Windows.Forms.Padding(2);
            this.GetByIdButton.Name = "GetByIdButton";
            this.GetByIdButton.Size = new System.Drawing.Size(80, 33);
            this.GetByIdButton.TabIndex = 21;
            this.GetByIdButton.Text = "Get By Id";
            this.GetByIdButton.UseVisualStyleBackColor = true;
            this.GetByIdButton.Click += new System.EventHandler(this.GetByIdButton_Click);
            // 
            // GetByPathButton
            // 
            this.GetByPathButton.Location = new System.Drawing.Point(338, 2);
            this.GetByPathButton.Margin = new System.Windows.Forms.Padding(2);
            this.GetByPathButton.Name = "GetByPathButton";
            this.GetByPathButton.Size = new System.Drawing.Size(80, 33);
            this.GetByPathButton.TabIndex = 20;
            this.GetByPathButton.Text = "Get by Path";
            this.GetByPathButton.UseVisualStyleBackColor = true;
            this.GetByPathButton.Click += new System.EventHandler(this.GetByPathButton_Click);
            // 
            // UploadButton
            // 
            this.UploadButton.Location = new System.Drawing.Point(254, 38);
            this.UploadButton.Margin = new System.Windows.Forms.Padding(2);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(80, 33);
            this.UploadButton.TabIndex = 19;
            this.UploadButton.Text = "Upload";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // GetPublicButton
            // 
            this.GetPublicButton.Location = new System.Drawing.Point(254, 2);
            this.GetPublicButton.Margin = new System.Windows.Forms.Padding(2);
            this.GetPublicButton.Name = "GetPublicButton";
            this.GetPublicButton.Size = new System.Drawing.Size(80, 33);
            this.GetPublicButton.TabIndex = 18;
            this.GetPublicButton.Text = "Public";
            this.GetPublicButton.UseVisualStyleBackColor = true;
            this.GetPublicButton.Click += new System.EventHandler(this.GetPublicButton_Click);
            // 
            // GetPhotos
            // 
            this.GetPhotos.Location = new System.Drawing.Point(170, 39);
            this.GetPhotos.Margin = new System.Windows.Forms.Padding(2);
            this.GetPhotos.Name = "GetPhotos";
            this.GetPhotos.Size = new System.Drawing.Size(80, 33);
            this.GetPhotos.TabIndex = 17;
            this.GetPhotos.Text = "Photos";
            this.GetPhotos.UseVisualStyleBackColor = true;
            this.GetPhotos.Click += new System.EventHandler(this.GetPhotos_Click);
            // 
            // GetCameraRollButton
            // 
            this.GetCameraRollButton.Location = new System.Drawing.Point(170, 2);
            this.GetCameraRollButton.Margin = new System.Windows.Forms.Padding(2);
            this.GetCameraRollButton.Name = "GetCameraRollButton";
            this.GetCameraRollButton.Size = new System.Drawing.Size(80, 33);
            this.GetCameraRollButton.TabIndex = 16;
            this.GetCameraRollButton.Text = "Camera Roll";
            this.GetCameraRollButton.UseVisualStyleBackColor = true;
            this.GetCameraRollButton.Click += new System.EventHandler(this.GetCameraRollButton_Click);
            // 
            // GetDocumentsButton
            // 
            this.GetDocumentsButton.Location = new System.Drawing.Point(86, 38);
            this.GetDocumentsButton.Margin = new System.Windows.Forms.Padding(2);
            this.GetDocumentsButton.Name = "GetDocumentsButton";
            this.GetDocumentsButton.Size = new System.Drawing.Size(80, 33);
            this.GetDocumentsButton.TabIndex = 15;
            this.GetDocumentsButton.Text = "Documents";
            this.GetDocumentsButton.UseVisualStyleBackColor = true;
            this.GetDocumentsButton.Click += new System.EventHandler(this.GetDocumentsButton_Click);
            // 
            // GetRootChildren
            // 
            this.GetRootChildren.Location = new System.Drawing.Point(86, 2);
            this.GetRootChildren.Margin = new System.Windows.Forms.Padding(2);
            this.GetRootChildren.Name = "GetRootChildren";
            this.GetRootChildren.Size = new System.Drawing.Size(80, 33);
            this.GetRootChildren.TabIndex = 14;
            this.GetRootChildren.Text = "Root Children";
            this.GetRootChildren.UseVisualStyleBackColor = true;
            this.GetRootChildren.Click += new System.EventHandler(this.GetRootChildren_Click);
            // 
            // GetRoodFolderButton
            // 
            this.GetRoodFolderButton.Location = new System.Drawing.Point(2, 38);
            this.GetRoodFolderButton.Margin = new System.Windows.Forms.Padding(2);
            this.GetRoodFolderButton.Name = "GetRoodFolderButton";
            this.GetRoodFolderButton.Size = new System.Drawing.Size(80, 33);
            this.GetRoodFolderButton.TabIndex = 12;
            this.GetRoodFolderButton.Text = "Root Folder";
            this.GetRoodFolderButton.UseVisualStyleBackColor = true;
            this.GetRoodFolderButton.Click += new System.EventHandler(this.GetRoodFolderButton_Click);
            // 
            // GetDriveButton
            // 
            this.GetDriveButton.Location = new System.Drawing.Point(2, 2);
            this.GetDriveButton.Margin = new System.Windows.Forms.Padding(2);
            this.GetDriveButton.Name = "GetDriveButton";
            this.GetDriveButton.Size = new System.Drawing.Size(80, 33);
            this.GetDriveButton.TabIndex = 13;
            this.GetDriveButton.Text = "Drive";
            this.GetDriveButton.UseVisualStyleBackColor = true;
            this.GetDriveButton.Click += new System.EventHandler(this.GetDriveButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 618);
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
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(292, 265);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OneDrive API Test";
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
    }
}

