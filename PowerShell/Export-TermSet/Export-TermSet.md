# Export-TermSet

## SYNOPSIS
Exports all terms from a specified Term Set in SharePoint Online, including their labels and translations, to a CSV file.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Export-TermSet

## AUTHOR
 - Name: Robin Agten
 - Email: robin.agten@delaware.pro

## SYNTAX
### Export-TermSet (Default)
```powershell
Export-TermSet -ClientId <String> -TenantName <String> -TermGroupName <String> -TermSetName <String> [-OutputPath <String>] [-LocaleIds <Int[]>]
```

## Prerequisites
- PowerShell version 7
- PnP PowerShell module installed (version 2.12 or higher)
- SharePoint Online tenant admin credentials
- Permissions to access the specified Term Set in SharePoint Online

## Description
This script connects to SharePoint Online using the PnP PowerShell module and exports all terms from a specified Term Set to a CSV file. It includes term details such as term ID, name, path, and labels (default and non-default) for specified locale IDs. The output file is saved to the specified path or the script's root directory.

## EXAMPLES

### EXAMPLE 1
Export terms from a Term Set without specifying locale IDs.
```powershell
Export-TermSet -ClientId "your-client-id" -TenantName "your-tenant-name" -TermGroupName "GroupName" -TermSetName "SetName"
```

### EXAMPLE 2
Export terms from a Term Set with locale-specific labels.
```powershell
Export-TermSet -ClientId "your-client-id" -TenantName "your-tenant-name" -TermGroupName "GroupName" -TermSetName "SetName" -LocaleIds 1033,1036
```

### EXAMPLE 3
Export terms to a custom output path.
```powershell
Export-TermSet -ClientId "your-client-id" -TenantName "your-tenant-name" -TermGroupName "GroupName" -TermSetName "SetName" -OutputPath "C:\Exports"
```

## Tags
 * SharePoint
 * Taxonomy