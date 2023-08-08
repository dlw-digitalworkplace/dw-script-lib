# Add-TermGroupSPAdmin

## SYNOPSIS
This script can be used to add the sp add-in account (app@sharepoint) to a term group. In the modern taxonomy store this cannot be done manually anymore.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Add-TermGroupSPAdmin

## AUTHOR
 - Name: Jens Van Dale
 - Email: jens.vandale@delaware.pro

## SYNTAX
### Add admin to tenant taxonomy store
```powershell
.\Add-TermGroupSPAdmin.ps1 -siteUrl <string> -termGroupName <string>
```

## Prerequisites
The following PowerShell modules need to be installed:
 - PnP.PowerShell
 - SharePoint Online Management Shell

## Description
The following steps are executed in this script:
 1. Some SharePoint dll's are loaded from the SharePoint Online Management Shell (Taxonomy)
 2. A connection to SharePoint is made using PnP.PowerShell
 3. A taxonomy session is build
 4. The term gropu is loaded based on the provided term group name
 5. The app@sharepoint account is added as a admin to this term group
 6. The SharePoint connection is disconnected

## EXAMPLES

### EXAMPLE 1
This command will add the app@sharepoint account as an admin to the Products term group on the contoso tenant
```powershell
.\Add-TermGroupSPAdmin.ps1 -siteUrl "https://contoso-admin.sharepoint.com" -termGroupName "Products"
```

## Tags
 * SharePoint online
 * Taxonomy