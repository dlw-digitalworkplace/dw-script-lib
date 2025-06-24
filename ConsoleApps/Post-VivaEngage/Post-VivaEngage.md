# Post-VivaEngage

## SYNOPSIS

This script posts messages in a single Viva Engage (VE) Community. The input is a storage account mix of table items containing post message & metadata and media uploaded to a blob container. The novelty this script brings is the way of authenticating towards the Yammer API. The old way using a verified Yammer app is retired, so you must use authentication via an Entra Id app. The rate limits (10 requests per 30 seconds) seem no longer to apply when posting messages / uploading files (using the small file upload endpoint) as an upload of 14.4k posts with lots of images and videos gave no throttling issues whatsoever at full speed.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/ConsoleApps/Post-VivaEngage.

## Relevant docs

Blog posts:
- https://techcommunity.microsoft.com/blog/viva_engage_blog/calling-legacy-viva-engage-yammer-apis-via-an-entra-application/4412337
- https://techcommunity.microsoft.com/blog/viva_engage_blog/yammer-api-with-aad-tokens-postman-collection/857923

API reference
- https://learn.microsoft.com/en-us/rest/api/yammer/messages-json-post
- https://learn.microsoft.com/en-us/rest/api/yammer/upload-files-into-yammer-groups

Useful SO post
- https://stackoverflow.com/questions/66062985/how-to-upload-files-on-yammer-api-via-the-azure-upload-small-file-api/66062986#66062986
- https://stackoverflow.com/questions/76383947/how-to-use-yammer-api-using-aad-authentication-method


## AUTHOR
 - Name: Pieter Heemeryck
 - Email: pieter.heemeryck@delaware.pro

## Prerequisites
 - The code uses .NET 8 so this must be installed
 - Entra ID app (see /readMe folder and authentication section in this file)

## Running the console app

### Running in visual studio
1. Open Visual Studio Solution
2. Update the user secrets of the project (see sample in Program.cs)
3. Run the console application

## Screenshots
See /readMe folder

## Tags
 * M365
 * Viva Engage
 * Yammer

 ## Authentication

The new way of authentication is chosen towards the Yammer v1 API. It requires an Entra Id app with certain Yammer permissions (access_as_user / user_impersonation / EngagementConversation.ReadWrite.All). See screenshots in /readme folder. The application permission is not strictly needed, but might be interesting when wanting to evolve to a scenario where you can automate the fetching of the access token.

Generating a new access token can be done using Postman. See screenshots in /readme folder.

- Token name: anything
- Grant type: Authorization Code
- Callback url: https://www.getpostman.com/oauth2/callback
- Auth url: https://login.microsoftonline.com/organizations/oauth2/v2.0/authorize
- Access Token url: https://login.microsoftonline.com/organizations/oauth2/v2.0/token
- Client Id: \<of Entra id app>
- Client secret: \<of Entra id app>
- Scope: https://api.yammer.com/user_impersonation
- State: \<empty>
- Client Authentication: Send as Basic Auth header