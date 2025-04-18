# List-ItemPermissions

## SYNOPSIS
This console application gets you started using the Microsoft Graph API C# SDK v5.77, written in .NET 8.
It connects to the Graph API using a client secret.

Some sample methods are provided, such as fetching a site, its lists, list items. Mostly the focus is on fetching any custom permissions on driveItems, detecting sharing links.

Sharing links are detected by checking the driveItem/<id>/permissions endpoint. When the 'link' property is not null, it is a sharing link. The following output can be found:

- Shared with who?
- Role (read | write)
- Type (view | edit)
- Scope (anonymous | users | organization)
- Prevents download (boolean)
- Has password (boolean)
- Expiration date (optional DateTime)

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/ConsoleApps/List-ItemPermissions.

## Relevant MS docs

For getting driveItems, detecting sharing links:

 - [A permission on a drive item](https://learn.microsoft.com/en-us/graph/api/resources/permission?view=graph-rest-1.0): Permissions have many different forms. The permission resource represents these different forms through facets on the resource.
 - [A sharing link (specific type of permission)](https://learn.microsoft.com/en-us/graph/api/resources/sharinglink?view=graph-rest-1.0): if a permission resource has a non-null sharingLink facet, the permission represents a sharing link (as opposed to permissions granted to a person or group).
 - [Create sharing link](https://learn.microsoft.com/en-us/graph/api/driveitem-createlink?view=graph-rest-1.0&tabs=http)
 - [Listing permissions](https://learn.microsoft.com/en-us/graph/api/driveitem-list-permissions?view=graph-rest-1.0&tabs=http): you can differentiate if the permission is inherited or not by checking the inheritedFrom property
 - [Grant access to an existing sharing link](https://learn.microsoft.com/en-us/graph/api/permission-grant?view=graph-rest-1.0&tabs=csharp)
 - [Delete a sharing permission from a file](https://learn.microsoft.com/en-us/graph/api/permission-delete?view=graph-rest-1.0&tabs=http): this correctly removes the sharing link as perceived in the SPO UI
 - [Accessing shared drive items](https://learn.microsoft.com/en-us/graph/api/shares-get?view=graph-rest-1.0&tabs=http): did not contain any extra useful info (although 'owner' should be in there)...
 - [Shared drive item](https://learn.microsoft.com/en-us/graph/api/resources/shareddriveitem?view=graph-rest-1.0): did not contain any extra useful info (although 'owner' should be in there)...

## AUTHOR
 - Name: Pieter Heemeryck
 - Email: pieter.heemeryck@delaware.pro

## Prerequisites
 - The code uses .NET 8 so this must be installed
 - A valid app registration (client id, client secret) with the following application permissions:
   - Sites.ReadWrite.All

## Running the console app

### Running in visual studio
1. Open Visual Studio Solution
2. Update the user secrets of the project (see the `appsettings.json` file)
3. Run the console application

## Screenshots
None

## Tags
 * Microsoft Graph