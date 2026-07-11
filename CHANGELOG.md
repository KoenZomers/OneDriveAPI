# Changelog

All notable changes to this project are documented in this file.

## [3.0.0.0] - Unreleased

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

- **`OneDriveForBusinessO365Api` modernized to Microsoft Entra ID v2.0**
  - Previously used the legacy Azure AD v1 "resource" parameter model together with a service discovery call to `https://api.office.com/discovery/v2.0/me/services/` to locate the OneDrive for Business / SharePoint endpoint (ADAL-style).
  - Now uses MSAL v2.0 scope-based authentication (e.g. `https://{tenant}-my.sharepoint.com/.default`) consistent with `OneDriveGraphApi`, removing the dependency on the legacy discovery service.

- **`OneDriveAccessToken` no longer represents a raw parsed OAuth token response**
  - Token data is now sourced from MSAL's `AuthenticationResult`. Code that directly constructed or inspected `OneDriveAccessToken` from manual token endpoint responses will need to be updated.

- **`TokenRetrievalFailedException` now wraps `Microsoft.Identity.Client.MsalException`**
  - Previously wrapped a custom `OneDriveError` parsed from the token endpoint's JSON error response.

### Migration Guidance

| Before (2.x) | After (3.x) |
|---|---|
| `new OneDriveConsumerApi(clientId, clientSecret)` | Not supported — use `new OneDriveGraphApi(clientId, clientSecret)` with a Microsoft Entra ID app registration |
| `new OneDriveForBusinessO365Api(clientId, clientSecret)` + resource/service discovery | `new OneDriveForBusinessO365Api(clientId, clientSecret)` using MSAL v2 scopes — no code change needed for basic usage, but app registration must support the Microsoft Graph / SharePoint v2 scopes |
| `oneDriveApi.GetAuthenticationUri()` + manual browser + `GetAuthorizationTokenFromUrl` | Still supported for the authorization-code pattern, now backed by MSAL under the hood — or use the newly exposed MSAL client application instance directly for `AcquireTokenInteractive`, `AcquireTokenByDeviceCode`, etc. |
| `oneDriveApi.AuthenticateUsingRefreshToken(refreshToken)` | Still supported, now performs the refresh through MSAL |

See `README.md` for updated authentication examples.

---

## [2.4.0.0] and earlier

See [VersionHistory.md](VersionHistory.md) for the full history of releases prior to this changelog.
