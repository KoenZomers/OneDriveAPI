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

1.5.0.3 - May 26, 2016

- Bugfix where GetAccessToken would return NULL instead of a new Access Token when the Access Token would have expired, but a valid Refresh Token was still available

1.5.0.3 - May 12, 2016

- Added UploadFileViaSimpleUpload method which accepts in a string the path to the file to upload

1.5.0.2 - May 10, 2016

- Updated Newtonsoft.Json reference to version 8.0.3

1.5.0.0 - April 29, 2016

- Refactored code heavily to be more optimized and have less duplicate code
- Replaced all WebRequest instances with HttpClient. This effectively means that HTTP proxies are now supported for every function in this OneDrive API and that TLS 1.2 is now supported.

[Version History](./VersionHistory.md)

## Feedback

Feedback is very welcome. If you believe something is not working well or could be improved, feel free to drop me an e-mail.

Koen Zomers
mail@koenzomers.nl
