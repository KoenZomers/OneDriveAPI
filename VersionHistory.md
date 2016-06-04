# OneDriveAPI

## Version History

1.5.1.0 - May 26, 2016

- Compiled against .NET Framework v4.6.1 as v4.5 has run out of support (https://blogs.msdn.microsoft.com/dotnet/2015/12/09/support-ending-for-the-net-framework-4-4-5-and-4-5-1/)

1.5.0.4 - May 26, 2016

- Bugfix where GetAccessToken would return NULL instead of a new Access Token when the Access Token would have expired, but a valid Refresh Token was still available

1.5.0.3 - May 12, 2016

- Added UploadFileViaSimpleUpload method which accepts in a string the path to the file to upload

1.5.0.2 - May 10, 2016

- Updated Newtonsoft.Json reference to version 8.0.3

1.5.0.0 - April 29, 2016

- Refactored code heavily to be more optimized and have less duplicate code
- Replaced all WebRequest instances with HttpClient. This effectively means that HTTP proxies are now supported for every function in this OneDrive API and that TLS 1.2 is now supported.

1.4.1.0 - April 28, 2016

- Added UploadFileAs methods which accept a Stream [as requested](https://github.com/KoenZomers/OneDriveAPI/issues/5)

1.4.0.0 - December 1, 2015

- Added support for OneDrive for Business on Office 365 through the OneDrive API v2.0. It does NOT work against an On Premises SharePoint farm.
  NOTE: The bug in the OneDrive API v2.0 is fixed and will now also work against OneDrive for Business.

1.3.1.1 - November 30, 2015

- Upgraded NewtonSoft.JSON NuGet package to v7.0.1

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