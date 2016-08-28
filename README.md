# OneDriveAPI
OneDrive API in C#

Microsoft offers their own SDK for communicating with OneDrive. I personally find that one to be too cumbersome and complex to use. I've therefore written my own library which communicates with the OneDrive API. It allows for file uploading (up to 10 GB), file downloading, modifications, retrieving listings and much more. It is still in alpha stage though, so feel free to use this code whereever you see fit, but don't expect a 100% fine working piece of code. I'm using this library for a couple of months now to upload large amounts of files to OneDrive on a steady base and didn't encounter any issues anymore with this version. Feel free to reach out in case you find something that you believe could be improved. This API uses the OneDrive API v2.0 and supports storing files on the Consumer OneDrive as well as on OneDrive for Business on Office 365. At present it does NOT support OneDrive for Business on On Premises SharePoint farms.

Notice: As of around July 8, 2015 Microsoft seems to have updated the OneDrive API which made it more strict in a lot of ways. I.e. the access tokens are now really only valid for 60 minutes. If you try to use the access token after 60 minutes, you will get a 401 Access Denied response with a WWW-Authenticate header stating:

_WWW-Authenticate: Bearer realm="OneDriveAPI", error="expired_token", error_description="Auth token expired. Try refreshing."_

The code contains a fully working demo Windows Forms application which shows you exactly how to use all of the functionality exposed in the OneDrive API.

In order to get a new access token from the refresh token you already got from authenticating to OneDrive, use the following code:

_var oneDriveApi = new OneDriveApi(ClientId, ClientSecret);_
_await oneDriveApi.AuthenticateUsingRefreshToken(RefreshToken);_

This oneDriveApi will have an access token again which is valid for 60 minutes. You can validate how long the access token is valid for by querying:

_oneDriveApi.AccessTokenValidUntil.Value_

Let me know in case you run into other things that no longer work because of this update and I'll be happy to look into it.

## Available via NuGet
You can also pull this API in as a NuGet package by adding the following NuGet repository to Visual Studio:
http://nuget.koenzomers.nl/nuget or running the following line from the NuGet Package Manager Console in Visual Studio:

Install-Package -Id KoenZomers.OneDrive.Api -Source https://nuget.koenzomers.nl/NuGet

## Version History

1.6.5.0 - August 28, 2016

- Added GetSharedWithMe method to get all items that have been shared with the current user. Only valid when used with a OneDrive for Business site. Still need to figure out how to get the actual content through the OneDrive API as I don't want to use SharePoint CSOM or REST as it seems to require a different access token.

1.6.4.1 - August 13, 2016

- Changed implementation of GetItemInFolder so it scans through all items in the folder instead of just the results of the first batch

1.6.4.0 - August 13, 2016

- Added GetItemInFolder to retrieve an item based on the folder it resides in and its filename
- Added GetAllChildrenByXXX methods for each of the existing methods to get all items inside a certain folder which obeys the paging NextLink in the results to ensure all child items are returned and not just the first batch

1.6.3.0 - August 11, 2016

- Fixed a bug with the Delete method throwing an exception

1.6.2.0 - August 11, 2016

- Added additional API functions for getting content from linked folders on other OneDrives (GetItemById and OneDriveRemoteItem entity)

1.6.1.0 - August 11, 2016

- Added support for getting folder contents by the parent folder ID instead of a path (GetChildrenByFolderId). This allows you to get the items from a folder on someone else their OneDrive which you have linked to yours.

1.6.0.0 - August 9, 2016

- Added support for the new OneDrive Consumer API registration process. Follow the steps below to register your own application.
  ** BEWARE: If upgrading from a previous version, ensure you add the OneDriveConsumerApiRedirectUri field to your App.Config. Leave its value an empty string in this case.

1.5.3.0 - July 10, 2016

- Added extra error handling in case retreiving an access token fails. It will now throw a TokenRetrievalFailedException with detailed information why it failed instead of just returning NULL to aid in troubleshooting.

1.5.2.0 - June 4, 2016

- Fixed an issue where OneDrive could respond with a HTTP 500 error and the retry functionality would fail

[Version History](./VersionHistory.md)

## Register your own Client ID / Client Secret

If you wish to use the OneDrive API, you need to register your own Client ID / Client Secret. Depending on whether you want to target OneDrive for Consumers or OneDrive for Business, follow the steps below to do so.

### OneDrive for Consumers

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

### OneDrive for Business

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
