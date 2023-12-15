# Microsoft Graph

## SYNOPSIS
This script guides you through the basics of how to get started with the Microsoft Graph PowerShell SDK. This replaces the soon to be deprecated / retired AzureAD PowerShell module.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/MicrosoftGraph

## AUTHOR
 - Name: Pieter Heemeryck
 - Email: pieter.heemeryck@delaware.pro

## Prerequisites
 - PowerShell module 'Microsoft.Graph'
 - PowerShell version >5.1
 - .NET Framework 4.7.2 or later

## Description
This script guides you through:
- Installing the MS Graph PS SDK
- Connecting to the Graph in several ways
- Discovery of required scope permissions to execute certain commandlets
- Discovery of commandlets
- Help with commandlets (parameters & samples)
- Executing some basic commands (get user, get teams, get channel)
- Disconnect from the Graph

```PowerShell
#region Prepare

# Basics: https://learn.microsoft.com/en-us/powershell/microsoftgraph/get-started?view=graph-powershell-1.0

# Install-Module Microsoft.Graph -Scope CurrentUser
# Get-InstalledModule Microsoft.Graph

# Make sure you connect with sufficient permissions. To do this, you need to know what commandlets require which permissions
Find-MgGraphCommand -command Get-MgUser | Select -First 1 -ExpandProperty Permissions

# Discover available commands:
Get-Command -Module Microsoft.Graph* *team*

# Get detailed parameter info and samples on how to use
Get-Help Get-MgUser -Detailed

#endregion


#region Connect to Graph

# https://learn.microsoft.com/en-us/powershell/microsoftgraph/authentication-commands?view=graph-powershell-1.0

Connect-MgGraph -Scopes "User.Read.All","Group.ReadWrite.All" # interactive & delegated

Connect-MgGraph -AccessToken $myAccessToken # access token already contains scope

$clientId = ""
$tenantId = ""
$certThumbprint = ""
Connect-MgGraph -ClientId $clientId -TenantId $tenantId -CertificateThumbprint $certThumbprint # have the certificate installed correctly on local machine + uploaded to AAD app
# Create self-signed certificate using PowerShell: https://learn.microsoft.com/en-us/entra/identity-platform/howto-create-self-signed-certificate

$clientSecretCredential = Get-Credential -Credential $clientId # Enter client_secret in the password prompt.
Connect-MgGraph -TenantId $tenantId -ClientSecretCredential $clientSecretCredential

Connect-MgGraph -Identity # system-assigned managed identity
Connect-MgGraph -Identity -ClientId "User_Assigned_Managed_identity_Client_Id" # user-assigned managed identity

Get-MgContext # check your connection (e.g. AuthType)

#endregion


#region Execute commands

Get-MgUser # get all users
$user = Get-MgUser -Filter "displayName eq 'Test user'"

Get-MgUserJoinedTeam -UserId $user.Id

$teamId = ""
$team = Get-MgTeam -TeamId $teamId
Get-MgTeamChannel -TeamId $team.Id
$channel = Get-MgTeamChannel -TeamId $teamId -Filter "displayName eq 'General'"
$primaryChannel = Get-MgTeamPrimaryChannel -TeamId $teamId

# reference: https://learn.microsoft.com/en-us/powershell/module/microsoft.graph.teams/?view=graph-powershell-1.0

#endregion


#region Close up

Disconnect-MgGraph

#endregion
```

## Tags
 * Azure AD
 * Graph
 * MS Teams
 * SharePoint