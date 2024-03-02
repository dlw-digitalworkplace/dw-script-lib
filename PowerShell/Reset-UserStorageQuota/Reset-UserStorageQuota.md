# Reset-UserStorageQuota

## SYNOPSIS
This script resets the storage quota for an end-user to the quota that has been set in the tenant settings.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Reset-UserStorageQuota

## AUTHOR
 - Name: Pieter Vandendriessche
 - Email: pieter.vandendriessche@delaware.pro

## SYNTAX
```powershell
.\Reset-UserStorageQuota.ps1
```

## Description
The following steps are executed in this script:
 1. When the module is not installed, it will install automatically
 2. Input for the tenant admin site url will be asked
 3. A login prompt will be shown, log in with the tenant administrator credentials
 4. The script fetches all the sites and reset the quota if different from tenant quota

## Tags
 * Storage
 * OneDrive
 * SharePoint
 * Administration
