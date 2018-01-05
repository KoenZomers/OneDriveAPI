# OneDriveAPI
OneDrive API in C#

Easy to use lightweight framework to communicate with the OneDrive and OneDrive for Business through either the Azure AD (api.onedrive.com & tenant-my.sharepoint.com/_api or the Azure AD V2.0 (graph.microsoft.com) endpoint. It allows communication through one unified piece of code with:

- OneDrive Personal
- OneDrive for Business
- SharePoint Online (yet to come, work in progess)
- SharePoint Server 2016 (yet to come, work in progess)

The code contains a fully working demo Windows Forms application which shows you exactly how to use all of the functionality exposed in the OneDrive API.

![](./Images/SolutionExplorer.png)

To get an instance to a OneDrive or OneDrive for Business through the Microsoft Graph API using Azure AD v2.0 (recommended), simply use:

```C#
KoenZomers.OneDrive.Api oneDrive = new OneDriveGraphApi(applicationId);
```

OR to get an instance to a OneDrive Consumer account, simply use:

```C#
KoenZomers.OneDrive.Api oneDrive = new OneDriveConsumerApi(clientId, clientSecret);
```

OR to get an instance to a OneDrive for Business account, simply use:

```C#
KoenZomers.OneDrive.Api oneDrive = new OneDriveForBusinessO365Api(clientId, clientSecret);
```

If you're not sure which of these to use, go with the Microsoft Graph using the Azure AD v2.0 endpoint.

If you want it to work through a HTTPS proxy, simply provide the proxy configuration by setting the ProxyConfiguration property:

```C#
oneDrive.ProxyConfiguration = System.Net.WebRequest.DefaultWebProxy;
```

In order to get a new access token from the refresh token you already got from authenticating to OneDrive or OneDrive for Business, simply use:

```C#
oneDrive.AuthenticateUsingRefreshToken("yourrefreshtoken");
```

If you don't have a refresh token yet, you will have to go through an interactive browser logon to perform authentication and get the refresh token. Check the DemoApplication to see how this works.

Once you have an authenticated OneDrive session, you can simply use for example:

- Getting all files in the root: _oneDrive.GetDriveRootChildren();_
- Downloading a file: _oneDrive.DownloadItemAndSaveAs("fileOnOneDrive.txt", "c:\temp\file.txt");_
- Uploading a file: _oneDrive.UploadFile("c:\temp\file.txt", "fileOnOneDrive.txt");_
- And many more operations...

Let me know in case you run into other things that no longer work because of this update and I'll be happy to look into it.

## Available via NuGet
You can also pull this API in as a NuGet package by adding "KoenZomers.OneDrive.Api" or running:

Install-Package KoenZomers.OneDrive.Api

Package statistics:
https://www.nuget.org/packages/KoenZomers.OneDrive.Api

## Version History

2.0.4.3 - January 5, 2018

- Fixed issue with ShareItem methods returning NULL if the item was already shared
- Added option to provide a OneDriveSharingScope to ShareItem when connecting through Graph API or to a OneDrive for Business to choose between an anonymous link or a link that only people in the same organisation can use. The scope is not supported with OneDrive Personal.

2.0.4.2 - January 5, 2018

- Fixed issues with some methods using /sites/ while prepedning /me

2.0.4.1 - January 5, 2018

- Modified all existing methods (i.e. get child items, renaming, copying) to also be able to handle items shared from another drive. Be sure to use the methods that accept a OneDriveItem type if you're dealing with shared items. Methods taking a string path are only for items stored on the current user its OneDrive.

2.0.4.0 - January 5, 2018

- Added the following methods to get items from items shared from other drives: GetDrive, GetChildrenFromDriveByFolderId, GetAllChildrenFromDriveByFolderId, GetItemFromDriveById
- Updated DownloadItem and all UploadItem methods to also work when providing a OneDriveItem which is retrieved through one of the above methods to download or upload an item residing on another drive
- Breaking change: GetSharedWithMe now returns a collection of OneDriveItem entities instead of the OneDriveSharedWithMeItem type. The OneDriveSharedWithMeItem type is no longer being used.

2.0.3.1 - December 31, 2017

- Bugfix: when you would provide proxy credentials but not a proxy server, the API would throw an exception when trying to connect

2.0.3.0 - October 31, 2017

- Added the following methods to work with [AppFolders](https://docs.microsoft.com/en-us/onedrive/developer/rest-api/concepts/special-folders-appfolder): GetAppFolderMetadata, GetAppFolderChildren, GetAllAppFolderChildren, CreateAppFolderFolder, GetAppFolderFolderOrCreate, UploadFileToAppFolder, UploadFileToAppFolderAs, UploadFileToAppFolderViaSimpleUpload, UploadFileToAppFolderViaResumableUpload. All of these only work when connecting through the Graph API. To download files from the AppFolder, you can use the regular DownloadItem methods. To upload a file to a subfolder of the AppFolder you can use the regular UploadFile methods.
- Added comments to the sourcecode behind every button in the demo application to explain what that specific button/scenario will do

2.0.2.0 - October 30, 2017

- Added the following methods to work with permissions on OneDrive items: AddPermission, ChangePermission, RemovePermission, ListPermissions. All of these only work when connecting through the Graph API.

2.0.1.0 - August 23, 2017

- Adjusted the functionality behind GetFolderOrCreate so that it also accepts multipaths. I.e. when you call GetFolderOrCreate("Files\Work\Contracts") it will now ensure that all the folders Files, Work and Contracts exist and return the instance of Contracts. Feature request from Vincent van Hulst.

2.0.0.0 - August 21, 2017

- Added support for utilizing the Microsoft Graph API to access files from both Consumer OneDrive as well as OneDrive for Business sites through 1 unified authentication process
- Changed the default limit for deciding between the Simple Upload and Resumable Upload from 5 MB to 4 MB [as per Microsoft recommendations](https://dev.onedrive.com/items/upload_put.htm)
- All methods in the base OneDrive class are now virtual so you can easily override them in your inherited code if you wish to do so
- Added methods to query for basic SharePoint Online site data as exposed by the [Graph API v1.0](https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/sharepoint). Only the beta of the Graph API supports working with list items and files on SharePoint Online. As the beta is not intended for production use, I haven't implemented these methods.

1.6.7.0 - August 18, 2017

- Compiled both the API and demo application against the .NET Framework 4.5.2 since 4.5 has gone out of support by Microsoft
- Added event UploadProgressChanged which reports back on the upload progress. This is implemented in the demo application under the Upload button. Be sure to upload a file larger than 5 MB to see it working as it's only used with the UploadFileViaResumableUpload method. Thanks to [sza110](https://github.com/sza110) for writing [this functionality](https://github.com/sza110/OneDriveAPI/commit/5ae44e089ef1e61b6672bee16d66b6a89917d241).

1.6.6.2 & 1.6.6.3 - June 29, 2017

- Wrapped the upload response with a new InvalidResponseException type which contains the message received from OneDrive when it is unable to parse it back to the expected type.

[Version History](./VersionHistory.md)

## Register your own Client ID / Client Secret

If you wish to use the OneDrive API, you need to register your own Client ID / Client Secret. Depending on whether you want to target OneDrive for Consumers or OneDrive for Business, follow the steps below to do so.

### OneDrive via Azure AD v2.0 / Microsoft Graph (recommended)

1. Go to https://apps.dev.microsoft.com
2. Log in with your Microsoft or school or work account
3. Click on "Add an app" next to "Converged applications"
4. Give it any name you'd like and uncheck the "Let us help you get started" box
5. Click Create
6. Copy the Application Id shown on the screen and use it in this app for the GraphApiApplicationId App.config AppSetting. Make sure you set the redirect URL to https://login.microsoftonline.com/common/oauth2/nativeclient. Allow implicit flow can stay unchecked, logout URL can be left empty, Microsoft Graph Permissions not need to be specified here.

### OneDrive for Consumers via Azure AD / api.onedrive.com

** NOTE: Microsoft updated the web interface, so the steps below have been updated to reflect this (August 9, 2016) **

1. Go to https://account.live.com/developers/applications/index
2. Log in with your Microsoft Account
3. Click on "Add an app" next to the "My applications" section
4. Give it any name you would like and click on "Create application"
5. It will show you the Application Id which you have copy to the App.config OneDriveConsumerApiClientID field
6. Click on "Generate New Password" under "Application Secrets"
7. Copy the generated password from the "New password generated" dialog and paste it to the App.config OneDriveConsumerApiClientSecret field and click OK
8. Click on "Add Platform" under "Platforms"
9. Click on "Web"
10. Ensure the "Allow Implicit Flow" is checked, enter a URL in the "Redirect URIs" field. This can be any URL. Just beware that this URL will receive your access token, so use a site that belongs to you. I'll use https://apps.zomers.eu . Make sure "Live SDK support" at the bottom is checked. Click on "Save" at the bottom.
11. Copy the same URI you've used at the previous step to the App.config OneDriveConsumerApiRedirectUri field
12. Run the demo application and click on "Authorize"

### OneDrive for Business via Azure AD / tenant-my.microsoft.com

1. Go to https://manage.windowsazure.com
2. Click on Active Directory in the left navigation bar
3. Click on your Azure Active Directory name
4. At the top, click on Applications
5. At the bottom, click on Add
6. Click on "Add an application my organization is developing"
7. Enter any name you would like, choose "Web application and/or web api" as the type, *regardless whether or not you will be using the OneDrive API in a web application*
8. As the sign-on URL, enter: https://login.live.com/oauth20_desktop.srf . As the App ID URI enter anything you would like. It must be in an URL format. I.e. https://apps.zomers.eu/myonedriveapiapp
9. Once the application is created, click on Configure at the top
10. If you want users outside of your own Azure Active Directory to be able to log in to the OneDrive API, switch "Application is multi-tenant" to YES. If only users from your own Azure Active Directory will be using the OneDrive API, keep it at NO.
11. Click the green Add application button at the bottom
12. Click on Office 365 SharePoint Online and then on the round checkmark buttom at the bottom right
13. At the bottom under "permissions to other applications" ensure that for Windows Azure Active Directory under Delegated Permissions "Sign in and read user profile" is checked. Click on the dropdown for Delegated Permissions in the line that reads "Office 365 SharePoint Online" and select "Read and write user files"
14. Click on Save at the bottom
15. Under keys select 1 or 2 years in the dropdown and click Save again at the bottom
16. After it's done processing your changes, it will show the client secret in the line under keys where you just used the dropdown. Copy this value to notepad. This is the client secret and only will be visible once and never again.
17. Copy the Client ID field at the top.
18. Replace the Client ID and Client Secret in your App.config where you will be using the OneDrive API. Use the DemoApplication its app.config for a sample of this.
19. Run the demo application, select "OneDrive for Business O365" and click on "Authorize"

## Feedback

Feedback is very welcome. If you believe something is not working well or could be improved, feel free to drop me an e-mail.

Koen Zomers
mail@koenzomers.nl
