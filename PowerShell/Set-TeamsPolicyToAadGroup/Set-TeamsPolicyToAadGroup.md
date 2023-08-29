# Set-TeamsPolicyToAadGroup

## SYNOPSIS
This script is used to apply a Teams app permission and app setup policy to an AAD group.
Teams admin center does not support adding app permission policy by group.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Set-TeamsPolicyToAadGroup

## AUTHOR
 - Name: Ewout Hebbrecht
 - Email: ewout.hebbrecht@delaware.pro
 - Original creator: Pieter Heemeryck
 - Original creator email: pieter.heemeryck@delaware.pro

## SYNTAX
### Apply teams permission & setup policy to group
```powershell
.\Set-TeamsPolicyToAadGroup -groupObjectId <string> -appPolicyname <string> -setupPolicyname <string>
```

### Remove teams permission & setup policy from group
```powershell
.\Set-TeamsPolicyToAadGroup -groupObjectId <string>
```

## Prerequisites
 - requires Powershell 5.1

Modules AzureADPreview & MicrosoftTeams will be installed using the script if not present.

## Description
The script executes the following steps:
 1. Checks if AzureADPreview & MicrosoftTeams are installed and installs them if not present
 2. Connnects to AzureAD & MicrosoftTeams
 3. Collects all the members in the group.
 Note: guests are not included
 4. Applies the app permission policy to all the members
 5. Applies the app setup policy to all the members
 6. The AzureAD & MicrosoftTeams connection is disconnected

## EXAMPLES

### EXAMPLE 1
This will add the members from the group "ba918eaa-4643-43ef-959b-a7870440678e" to the app policy with the name "App Pilot Users" and to the setup policy with the name "Setup Pilot Users".
```powershell
.\Set-TeamsPolicyToAadGroup -groupObjectId "ba918eaa-4643-43ef-959b-a7870440678e" -appPolicyname "App Pilot Users" -setupPolicyname "Setup Pilot Users"
```

### EXAMPLE 2
This will remove any app policy and setup policy from the members in the group "ba918eaa-4643-43ef-959b-a7870440678e".
```powershell
.\Set-TeamsPolicyToAadGroup -groupObjectId "ba918eaa-4643-43ef-959b-a7870440678e"
```

## Tags
 * MS Teams
 * Azure AD