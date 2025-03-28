# Set-DefaultValuesFolder

## SYNOPSIS

This script connects to a SharePoint site and sets default column values for folders in a document library based on data from CSV files.

## SOURCE

https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Set-DefaultValuesFolder

## AUTHOR

- Name: Arne Meersman
- Email: arne.meersman@delaware.pro

## SYNTAX

### Set

```powershell
.\Set-DefaultValuesFolder.ps1 -clientId <string> -siteUrl <string> -libraryName <string> -pathToDataCsv <string> -pathToConfigCsv <string> -folderKey <string>
```

## Prerequisites

The following PowerShell modules need to be installed:

- PnP PowerShell

The following input needs to be prepared:

- Data CSV file: Contains folder name and values to be set (first row is header)
- Config CSV file: Maps columns in the data file to SharePoint fields (first row is header)

## Description

The following steps are executed in this script:

1.  The script connects to the specified SharePoint site.
2.  The data from the CSV file is read, including folder paths and values for columns.
3.  Each folder will have its default column values set based on the data and configuration provided.

## EXAMPLES

### EXAMPLE 1

This command will set default column values for folders in the "Documents" library of the "https://contoso.sharepoint.com/sites/example" SharePoint site:

```powershell
.\Set-DefaultValuesFolder.ps1 -clientId "your-client-id" -siteUrl "https://contoso.sharepoint.com/sites/example" -libraryName "Documents" -pathToDataCsv "defaultColumnValues.csv" -pathToConfigCsv "config.csv" -folderKey "folder"
```

## Tags

- SharePoint
- PnP PowerShell
- Document Library
- Default Column Values
