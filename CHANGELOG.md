# Changelog

All notable changes to this project are documented in this file.

## [3.1.0.0]

### Added

- Added support for resolving OneDrive sharing URLs, including regular "Copy link" URLs and shortened `1drv.ms` links, into `OneDriveItem` instances through the Microsoft Graph `shares` API.
- Added `GetItemBySharingUrl`, `ResolveSharingUrl`, `GetItemBySharingToken` overloads and `CreateSharingUrlToken` so callers can work directly with sharing links or precomputed Microsoft Graph share tokens.
- Added `SharingUrl` and `SharingToken` metadata to `OneDriveItem` so items retrieved through sharing links retain the context needed for later content operations without serializing those values back to Graph.

### Changed

- Download and upload operations now recognize items retrieved through sharing links and call the appropriate `shares/{token}/driveItem` Microsoft Graph endpoints for content downloads, simple upload updates and resumable upload session creation.
- Requests that redeem sharing links now send the required `Prefer` header values (`redeemSharingLinkIfNecessary` with a fallback to `redeemSharingLink`) through new internal request helper overloads.
- URL construction now treats `shares/` requests as Microsoft Graph API calls, matching the existing handling for `drives/` requests.
- Updated the package, assembly and file versions to 3.1.0.0 and refreshed the generated XML documentation for the new public API surface.

### Fixed

- `DownloadItemAndSaveAs` now returns `false` when a download stream cannot be opened instead of attempting to copy from a null stream.

## [3.0.2.0]

### Fixed

- Fixed a potential issue with larger file uploads [#49](https://github.com/KoenZomers/OneDriveAPI/pull/49). Thanks YasarF!

## [3.0.1.0]

### Fixed

- Fixed an issue with file upload size handling

## [3.0.0.0]

### ⚠️ Breaking Changes

This is a **major version release** with breaking changes to the authentication model. It is **not** backwards compatible with 2.x.

- **Migrated authentication to MSAL (Microsoft Authentication Library)**
  - Replaced the hand-rolled OAuth2 token retrieval (manual `HttpClient` calls to token endpoints, custom `QueryStringBuilder`-based request construction) with [Microsoft.Identity.Client](https://www.nuget.org/packages/Microsoft.Identity.Client) (MSAL.NET).
  - The library now exposes the underlying MSAL `IPublicClientApplication` / `IConfidentialClientApplication` so consumers can use **any** MSAL-supported authentication flow (interactive with system browser, embedded webview, device code, authorization code, silent/cached token acquisition, refresh token migration, client credentials for app-only scenarios) rather than being restricted to a single hard-coded flow.
  - Token caching, silent renewal and expiry handling are now delegated to MSAL's token cache instead of custom logic in `OneDriveApi`.

- **Removed `OneDriveConsumerApi`**
  - This class authenticated against the legacy Microsoft **Live Connect** protocol (`login.live.com/oauth20_*`), which is not part of Microsoft Entra ID / Azure AD and is **not supported by MSAL**.
  - Consumers who need OneDrive Personal access should use `OneDriveGraphApi` against the Microsoft Graph API (`https://graph.microsoft.com`), which supports both personal Microsoft accounts and organizational accounts through Microsoft Entra ID v2.0.
  - If you were using `OneDriveConsumerApi`, you will need to register a new app registration in the [Microsoft Entra admin center](https://entra.microsoft.com) (or [Azure Portal](https://portal.azure.com)) rather than the legacy [Live Connect developer center](https://account.live.com/developers/applications/index), and switch to `OneDriveGraphApi`.

- **Removed `OneDriveForBusinessO365Api`**
  - This class called the legacy SharePoint REST v2.0 endpoint (`https://{tenant}.sharepoint.com/_api/v2.0/...`) using the ADAL v1 "resource" parameter model together with the (now discontinued) Office 365 Discovery Service.
  - Per Microsoft's official guidance, all innovation for SharePoint/OneDrive REST access is driven through the Microsoft Graph REST API; the SharePoint REST v2.0 endpoint is a legacy passthrough that receives no new functionality.
  - Use `OneDriveGraphApi` instead, which covers OneDrive Personal, OneDrive for Business and SharePoint sites through the single, actively developed Microsoft Graph API (`https://graph.microsoft.com`).

- **`OneDriveAccessToken` no longer represents a raw parsed OAuth token response**
  - Token data is now sourced from MSAL's `AuthenticationResult`. Code that directly constructed or inspected `OneDriveAccessToken` from manual token endpoint responses will need to be updated.

- **`TokenRetrievalFailedException` now wraps `Microsoft.Identity.Client.MsalException`**
  - Previously wrapped a custom `OneDriveError` parsed from the token endpoint's JSON error response.

- **Merged the abstract `OneDriveApi` base class into `OneDriveGraphApi`**
  - Now that `OneDriveGraphApi` is the only remaining API class, the separate abstract base class no longer served a purpose and has been folded into a single, concrete `OneDriveGraphApi` class.
  - Code that referenced the `OneDriveApi` type directly (e.g. `OneDriveApi oneDrive = new OneDriveGraphApi(...)`) should reference `OneDriveGraphApi` instead.
  - All public methods and properties remain available on `OneDriveGraphApi` with the same names and signatures - only the type name used for variables/fields declared as `OneDriveApi` needs to change.

### Migration Guidance

| Before (2.x) | After (3.x) |
|---|---|
| `new OneDriveConsumerApi(clientId, clientSecret)` | Not supported — use `new OneDriveGraphApi(clientId, clientSecret)` with a Microsoft Entra ID app registration |
| `new OneDriveForBusinessO365Api(clientId, clientSecret)` + resource/service discovery | Not supported — use `new OneDriveGraphApi(clientId, clientSecret)`, which covers the same OneDrive for Business / SharePoint functionality through the Microsoft Graph API |
| `oneDriveApi.GetAuthenticationUri()` + manual browser + `GetAuthorizationTokenFromUrl` | Still supported for the authorization-code pattern, now backed by MSAL under the hood — or use the newly exposed MSAL client application instance directly for `AcquireTokenInteractive`, `AcquireTokenByDeviceCode`, etc. |
| `oneDriveApi.AuthenticateUsingRefreshToken(refreshToken)` | Still supported, now performs the refresh through MSAL |
| `OneDriveApi oneDrive = new OneDriveGraphApi(...)` | `OneDriveGraphApi oneDrive = new OneDriveGraphApi(...)` - the `OneDriveApi` base class has been merged into `OneDriveGraphApi` |

See `README.md` for updated authentication examples.

---

## Earlier releases

See [VersionHistory.md](VersionHistory.md) for the full history of releases prior to this changelog.
