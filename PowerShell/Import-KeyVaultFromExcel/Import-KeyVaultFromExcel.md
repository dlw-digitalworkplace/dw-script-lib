# Import-KeyVaultFromExcel

## SYNOPSIS
This script imports keyVault secrets based on an excel sheet.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Import-KeyVaultFromExcel

## AUTHOR
 - Name: Pieter Vandendriessche
 - Email: pieter.vandendriessche@delaware.pro

## SYNTAX
### Add admin to tenant taxonomy store
```powershell
.\Import-KeyVaultFromExcel.ps1 -subscriptionId <string> -keyVaultName <string> -fileName <string> -sheetName <string>
```

## Prerequisites
The following PowerShell modules need to be installed:
 - AzureAD PowerShell

The following input needs to be prepared:
 -  Excel file with the key in the first column and the value in the second (first row is header). Please see example in folder of script

## Description
The following steps are executed in this script:
 1. A login prompt for the entered subscription will happen, an admin on the subscription needs to login
 2. The excel will be read
 3. Each added secret will be added to the KeyVault

## EXAMPLES

### EXAMPLE 1
This command will add the app@sharepoint account as an admin to the Products term group on the contoso tenant
```powershell
.\Import-KeyVaultFromExcel.ps1 -subscriptionId "67d1f400-991a-4960-9d1d-32d8899515c9" -keyVaultName "sibe-wp-mig-ast-tst-kv" -fileName "KeyVaultSecrets.xlsx" -sheetName "Sheet 1"
```

## Tags
 * Key Vault
 * Azure AD