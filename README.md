# OneDriveAPI
OneDrive API in C#

Microsoft offers their own SDK for communicating with OneDrive. I personally find that one to be too cumbersome and complex to use. I've therefore written my own library which communicates with the OneDrive API. It allows for file uploading (up to 10 GB), file downloading, modifications, retrieving listings and much more. It is still in alpha stage though, so feel free to use this code whereever you see fit, but don't expect a 100% fine working piece of code. I'm using this library for a couple of months now to upload large amounts of files to OneDrive on a steady base and didn't encounter any issues anymore with this version. Feel free to reach out in case you find something that you believe could be improved. This API uses the OneDrive API v1.0.

Notice: As of around July 8, 2015 Microsoft seems to have updated the OneDrive API which made it more strict in a lot of ways. I.e. the access tokens are now really only valid for 60 minutes. If you try to use the access token after 60 minutes, you will get a 401 Access Denied response with a WWW-Authenticate header stating:

_WWW-Authenticate: Bearer realm="OneDriveAPI", error="expired_token", error_description="Auth token expired. Try refreshing."_

In order to get a new access token from the refresh token you already got from authenticating to OneDrive, use the following code:

_var oneDriveApi = await Api.OneDriveApi.GetOneDriveApiFromRefreshToken(ClientId, ClientSecret, RefreshToken);_

This oneDriveApi will have an access token again which is valid for 60 minutes. You can validate how long the access token is valid for by querying:

_oneDriveApi.AccessTokenValidUntil.Value_

Let me know in case you run into other things that no longer work because of this update and I'll be happy to look into it.

## Available via NuGet
You can also pull this API in as a NuGet package by adding the following NuGet repository to Visual Studio:
http://nuget.koenzomers.nl/nuget or running the following line from the NuGet Package Manager Console in Visual Studio:

Install-Package -Id KoenZomers.OneDrive.Api -Source https://nuget.koenzomers.nl/NuGet

## Version History

1.3.1.0 - November 25, 2015

- Deprecated GetOneDriveApiFromRefreshToken which has been replaced with AuthenticateUsingRefreshToken so it can be used with a proxy

1.3.0.0 - November 25, 2015

- Added support for using HTTP/HTTPS proxies

1.2.0.0 - November 25, 2015

- Added functionality for copying, moving and renaming files within OneDrive
- Fixed a bug with Downloading a OneDrive item
- Removed missing assembly reference from demo application
- Added a method to get a stream to a OneDrive item instead of having to save it to disk

1.1.1.0 - July 18, 2015

- Added an overload of GetAuthenticationUri where you can provide a custom security scope to request

1.1.0.0 - July 10, 2015

- Fixed incorrect conflictBehavior tag when using Resumable Upload. It is now set to always overwrite the file if it already exists on OneDrive.

## Feedback

Feedback is very welcome. If you believe something is not working well or could be improved, feel free to drop me an e-mail.

Koen Zomers
mail@koenzomers.nl
