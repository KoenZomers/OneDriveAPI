# OneDriveAPI

## Version History

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