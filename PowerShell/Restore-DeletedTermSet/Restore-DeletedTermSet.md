# Restore-DeletedTermSet

## SYNOPSIS
This script can be used to restore a deleted term set by using the TaxonomyHiddenList of a site to restore the used terms.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Restore-DeletedTermSet

## AUTHOR
 - Name: Ruben Grillaert
 - Email: ruben.grillaert@delaware.pro

## SYNTAX
### Restore deleted term set
```powershell
.\Restore-DeletedTermSet.ps1 -TaxonomyHiddenListId <string> -RemovedTermSetId <string> -RemovedTermSetName <string> -RemovedTermSetGroupName <string> -SiteUrl <string>
```

## Prerequisites
To run the script, the PnP.PowerShell module needs to be installed.
Further, the following parameters are needed:
 1. TaxonomyHiddenListId: can be found by adding "/Lists/TaxonomyHiddenList" to the site url
 2. RemovedTermSetId: the TaxonomyHiddenList contains a field with an ID reference to the term set. By looking for a term in the deleted term set, you can find the ID.
 3. RemovedTermSetName: name of the term set that has been removed.
 4. RemovedTermSetGroupName: term group name where the term set was stored.
 5. SiteUrl: site where the terms are used. This script is not ready to be used for multiple sites because if a child term doesn't have a parent yet in the term set, it will create that term with the correct label but not the correct ID. However, the first part can be modified to group all the data of different sites.

## Description
When running the script, the following steps are executed:
 1. Creating a connection with SharePoint. This shows a pop-up the first time.
 2. Get all the terms from the TaxonomyHiddenList.
 3. Create the missing term set if needed. If the term set exists, the script continues.
 4. Adding all root terms that are used directly or that have a child term that is used.
 5. Adding all child terms by depth incremental. This step also adds parent terms (with a random ID) that were not used in order to restore the tree structure.

## EXAMPLES

### EXAMPLE 1
```powershell
.\Restore-DeletedTermSet.ps1 -TaxonomyHiddenListId "21d0b4a0-8f88-4409-8597-dd2c83f3e98e" -RemovedTermSetId "0fe908e1-1feb-45cd-9152-bfa892412b62" -RemovedTermSetName "ContosoTermSet" -RemovedTermSetGroupName "ContosoTermGroup" -SiteUrl "https://contoso.sharepoint.com/sites/example"
```

## Tags
 * SharePoint online
 * Taxonomy