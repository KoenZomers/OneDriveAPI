# OneDriveAPI

## Version History

2.4.0.0 - May 18, 2022

- Recompiled to support .NET Framework 4.8.0, .NET Core 3.1 and .NET 6.0
- Removed Newtonsoft JSON dependency by switching to System.Text.Json

2.3.2.0 - February 2, 2021

- Added ```public virtual async Task<OneDriveItemCollection> GetNextChildrenByPath(string skipTokenUrl)``` which allows file requests from using i.e. ```public virtual async Task<OneDriveItemCollection> GetChildrenByPath(string path)``` on a large folder containing more than 100 files to retrieve the next batch of files. If your intend is to get all the results, use ```public virtual async Task<OneDriveItem[]> GetAllChildrenByPath(string path)``` instead. [Issue 28](https://github.com/KoenZomers/OneDriveAPI/issues/28)

2.3.1.1 - November 16, 2020

- Fixed bug in using copy [Issue 24](https://github.com/KoenZomers/OneDriveAPI/issues/24). Thanks to [Eirielson Rodrigues](https://github.com/eirielson) for reporting this!

2.3.1.0 - October 27, 2019

- Merged [PR 20](https://github.com/KoenZomers/OneDriveAPI/pull/20) to allow for providing a client secret with the OneDrive Graph API

2.3.0.2 - May 16, 2019

- Fixed a bug when uploading files larger than 5 MB throwing an exception stating that the oneDriveUploadSession is NULL

2.3.0.1 - May 5, 2019

- Recompiled to support multi platform to support .NET Standard 2.0, .NET Framework 4.5.2, .NET Framework 4.7.2 and .NET Core 2.0
- Downgraded Newtonsoft JSON version requirement to be 11.0.1 and higher for .NET Core and .NET Standard and 8.0.1 for .NET Framework to allow for a bit greater compatibility

2.2.1.0 - May 5, 2019

- Added several UpdateFile methods to allow for updating the contents of an existing file. The normal UploadFile throws an exception in some cases if you try to upload a file to the same location, especially when the item is shared with the user and resides on another drive. To aid in that scenario, use the UpdateFile methods
- Added OneDriveSharedItem entity which will expose information on the owner of a shared item
- Several smaller fixes

2.2.0.0 - March 3, 2019

- Converted the API from the .NET Framework to .NET Standard 2.0 so it can also be used on non Windows environments
- Upgraded to use Newtonsoft JSON 12.0.1
- Upgraded the Demo Application to use a proper namespace and be compiled against the .NET Framework 4.6.2 instead of 4.5.2

2.1.2.1 - February 17, 2019

- Fixed bug in OneDrive for Business provider causing an error like "OAuth2 Authorization code was already redeemed" introduced by an API change on the Microsoft side when trying to authenticate

2.1.1.0 - January 4, 2019

- Fixed a typo in NameConflictBeha*v*iorAnnotation. Thanks to Daniel Ethier for reporting this.
- Implemented the ability to specify the NameConflictBehavior when uploading a file. The enumerator provides the option to overwrite the existing file, rename the file that is being uploaded or to fail the upload in the situation where a similarly named file already exists in the target location on OneDrive. Note that the SimpleUpload method does not support providing a NameConflictBehavior so it will always use the ResumableUpload method. It is also only implemented for the Graph API, so ensure to cast your oneDrive instace to OneDriveGraphApi to see the new argument.
- Included the documentation XML file which should bring the inline comments to your application from where you add the NuGet or assembly reference

2.1.0.1 - January 12, 2018

- Bufixes in uploading

2.1.0.0 - January 11, 2018

- Various bugfixes
- Mayor updates to most of the methods to properly support working with shared items from other drives
- NOTICE: This version is not 100% backwards compatible with the previous version. To enable the shared items functionality to work I had to break through the golden rule to try to keep backwards compatibility. The changes required to your code should be minimal though.

2.0.4.3 - January 5, 2018

- Fixed issue with ShareItem methods returning NULL if the item was already shared
- Added option to provide a OneDriveSharingScope to ShareItem when connecting through Graph API or to a OneDrive for Business to choose between an anonymous link or a link that only people in the same organisation can use. The scope is not supported with OneDrive Personal.

2.0.4.2 - January 5, 2018

- Fixed issues with some methods using /sites/ while prepending /me

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