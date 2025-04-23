# Set-FileMetadataFromParentFolder

## SYNOPSIS

This script updates the metadata of files based on the parent folder in a SharePoint document library. It ensures that files get the same metadata as their parent folder, one level above.

## SOURCE

https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Set-FileMetadataFromParentFolder

## AUTHOR

- Name: Arne Meersman
- Email: arne.meersman@delaware.pro

## SYNTAX

### Update file metadata based on parent folder

```powershell
Set-FileMetadataFromParentFolder -ClientId <String> -SiteUrl <String> -LibraryName <String> -FieldsToCheck <String[]> [-StartId <Int>] [-EndId <Int>]
```

## Prerequisites

The following PowerShell modules need to be installed:

- PnP PowerShell

To successfully execute this script, the following prerequisites must be met:

- You have the necessary permissions to make changes to the files in the library.
- Ensure metadata fields are configured in the SharePoint library. !!If there is no metadata on the parent folder, the metadata on its child files will be removed!!

## Description

This script updates the metadata of files based on the parent folder in a SharePoint document library. It ensures that files get the same metadata as their parent folder, one level above. Additionally, you have the option to define a start and end point to run the script for a specific section of the library or to execute the script in smaller parts. You also have the ability to choose which metadata fields you want to copy to the files.

## EXAMPLES

### EXAMPLE 1: Don't use startId & endId

Set metadata based on parent folders for the following metadata fields: Project, Project Name, Field3, Field4. In this example we go over the whole library.

```powershell
.\Set-MetadataBasedOnParent.ps1 -clientId "your-client-id" -siteUrl "https://contoso.sharepoint.com/sites/example" -libraryName "Documents" -fieldsToCheck @("Project", "Project_x0020_Name", "Field3", "Field4")
```

### EXAMPLE 2: Use startId & endId

Set metadata based on parent folders for the following metadata fields: Project, Project Name, Field3, Field4. In this example we use the startId and endId to edit files with Id between 100000 and 200000.

```powershell
.\Set-MetadataBasedOnParent.ps1 -clientId "your-client-id" -siteUrl "https://contoso.sharepoint.com/sites/example" -libraryName "Documents" -fieldsToCheck @("Project", "Project_x0020_Name", "Field3", "Field4") -startId 100000 -endId 200000
```

## Tags

- SharePoint
- PnP.PowerShell
- Metadata Management