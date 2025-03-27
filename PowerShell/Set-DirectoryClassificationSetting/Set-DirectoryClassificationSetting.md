# Set-DirectoryClassificationSetting

## SYNOPSIS
This script allows you to set the allowed classification list and creates a new group setting object if not already present.

Originally this script was also going to include setting the `DefaultClassification` & `ClassificationDescriptions` settings, but at the time of making the script these give HTTP 500 errors (beta endpoints be like 😉), if required, these can be added in the same way as the `ClassificationList` setting.

> [!WARNING]
> This script uses commands that are still in beta!

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Set-DirectoryClassificationSetting

## AUTHOR
 - Name: Ewout Hebbrecht
 - Email: ewout.hebbrecht@delaware.pro
 - Original creator: Nick Sevens
 - Original creator email: nick.sevens@delaware.pro

## Prerequisites
 - An app registration with graph application `Directory.ReadWrite.All` permission, for Workspaces this should be the main app registration.
 - A certificate added to the same app registration for authentication, this certificate is also installed on your machine for use

## Description
This script will follow the following execution path:
1. Install `Microsoft.Graph` & `Microsoft.Graph.Beta` if not already installed
2. Get existing `Group.Unified` setting
3. Create new `Group.Unified` setting if none was found
4. Set `ClassificationList` if value is provided

## Parameters
| Variable name          | Mandatory | Variable Description                                                                | Example                                  |
|------------------------|-----------|-------------------------------------------------------------------------------------|------------------------------------------|
| $ClassificationList    | False     | A comma-delimited list of valid classification values that can be applied to Microsoft 365 groups.     | Internal,Confidential |
| $TenantId              | True      | The tenant ID of the app registration                                               | 00000000-0000-0000-0000-000000000000     |
| $ClientId              | True      | The client ID of the app registration                                               | 00000000-0000-0000-0000-000000000000     |
| $CertificateThumbprint | True      | The thumprint of the certificate installed on the app registration                  | a909502dd82ae41433e6f83886b00d4277a32a7b |
- All parameters are of type string

## EXAMPLES

### EXAMPLE 1
This will set the `ClassificationList` setting.
```powershell
.\Set-DirectoryClassificationSetting -ClassificationList "Internal,Confidential" -TenantId "00000000-0000-0000-0000-000000000000" -ClientId "00000000-0000-0000-0000-000000000000" -CertificateThumbprint "a909502dd82ae41433e6f83886b00d4277a32a7b"
```

## Tags
 * Azure AD
 * Graph
