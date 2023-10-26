# Assign-FieldCustomizerExtension

## SYNOPSIS
This small PowerShell script can be used to assign an SPFx field customizer extension to a SharePoint column. 
The extension can be assigned to a field on site level or list level based on the client side component id of the SPFx solution

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Assign-FieldCustomizerExtension

## AUTHOR
 - Name: Robin Agten
 - Email: robin.agten@delaware.pro

## SYNTAX
```powershell
.\Assign-FieldCustomizerExtension.ps1 -siteCollectionUrl <string> -fieldInternalName <string> -clientSideComponentId <string> -listName <string> [-updateExistingLists]
```

## Prerequisites
 - PnP.PowerShell v1.22 (will be installed automatically if not available)

## Description
This script will execute the following:
 - Install PnP.PowerShell if needed
 - Get the SP field based on the internal name and list name
 - Update the SP field porperty `clientSideComponentId` with the provided id

## EXAMPLES

### EXAMPLE 1: Assign to site level column
```powershell
.\Assign-FieldCustomizerExtension.ps1 -siteCollectionUrl "https://contoso.sharepoint.com/sites/form-customizer" -fieldInternalName "Title" -clientSideComponentId "f5b9454b-ba41-487b-9978-8d526f7f838d"
```

### EXAMPLE 2: Assign to site level column and update all lists that contain the column
```powershell
.\Assign-FieldCustomizerExtension.ps1 -siteCollectionUrl "https://contoso.sharepoint.com/sites/form-customizer" -fieldInternalName "Title" -clientSideComponentId -updateExistingLists
```
### EXAMPLE 3: Assing to list level column
```powershell
.\Assign-FieldCustomizerExtension.ps1 -siteCollectionUrl "https://contoso.sharepoint.com/sites/form-customizer" -fieldInternalName "Title" -clientSideComponentId -listName "FieldCustomizer"
```

## Tags
 * SharePoint
 * SPFx field customizer