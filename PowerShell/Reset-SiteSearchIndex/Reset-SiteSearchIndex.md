# Reset-SiteSearchIndex

## SYNOPSIS
This script allows you to reset the search index for a specific site collection or a set of site collections.
> [!WARNING]  
> Be careful when using this script. Resetting the search index of site might cause heavy loads on the search index process of your tenant.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Reset-SiteSearchIndex

## AUTHOR
 - Name: Robin Agten
 - Email: robin.agten@delaware.pro

## Prerequisites
 - PnP.PowerShell v1.22 (will be installed automatically if not available)

## Description
This script will follow the following execution path:
1. Install PnP.PowerShell if not available
2. Check if the `allSiteCollections` flag is enables
3. If enabled:
    1. Validate if the sp admin url is provided
    2. Validate and warn if no url filter is provided
    3. Fetch all site collections
    4. Loop all site collections and reindex
4. Else:
    1. Validate if the site collection url is provided
    2. Reindex the site collection

## EXAMPLES

### EXAMPLE 1 - Single site collection
```powershell
.\Reset-SiteSearchIndex.ps1 -siteCollectionUrl "https://site.sharepoint.com/sites/sitecollection1"
```

### EXAMPLE - Multiple site collections
All site collections containing '/contoso-' in the url will be reset
```powershell
.\Reset-SiteSearchIndex.ps1 -multipleSiteCollections -spAdminUrl "https://agtenrdev-admin.sharepoint.com" -siteCollectionUrlFilter '/contoso-'
```

### EXAMPLE - All site collections
All site collections containing '/contoso-' in the url will be reset
```powershell
.\Reset-SiteSearchIndex.ps1 -multipleSiteCollections -spAdminUrl "https://agtenrdev-admin.sharepoint.com"
```

## Tags
 * SharePoint
 * Search