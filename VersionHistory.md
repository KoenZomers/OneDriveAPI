# OneDriveAPI

## Version History

1.6.7.0 - August 18, 2017

- Compiled both the API and demo application against the .NET Framework 4.5.2 since 4.5 has gone out of support by Microsoft
- Added event UploadProgressChanged which reports back on the upload progress. This is implemented in the demo application under the Upload button. Be sure to upload a file larger than 5 MB to see it working as it's only used with the UploadFileViaResumableUpload method. Thanks to [sza110](https://github.com/sza110) for writing [this functionality](https://github.com/sza110/OneDriveAPI/commit/5ae44e089ef1e61b6672bee16d66b6a89917d241).

1.6.6.2 & 1.6.6.3 - June 29, 2017

- Wrapped the upload response with a new InvalidResponseException type which contains the message received from OneDrive when it is unable to parse it back to the expected type.

1.6.6.1 - October 17, 2016

- The last update introduced a bug when using the system default proxy with the system default credentials. Fixed it in this release.

1.6.6.0 - October 14, 2016

- Fixed a bug where if you would be using a proxy and first have it set to use the proxy and then use the same instance to tell it to no longer use the proxy, it would still use the proxy anyway.

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