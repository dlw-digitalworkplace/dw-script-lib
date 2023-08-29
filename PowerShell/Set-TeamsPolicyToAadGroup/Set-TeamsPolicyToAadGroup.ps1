param (
    [Parameter(Mandatory = $true)]
    [string]
    $groupObjectId,

    [Parameter(Mandatory = $false)]
    [string]
    $appPolicyname = $Null,

    [Parameter(Mandatory = $false)]
    [string]
    $setupPolicyname = $Null
)

# Install-module AzureADPreview if needed
if (Get-Module -ListAvailable -Name AzureADPreview) {
    Write-Host "Module AzureADPreview exists"
}
else {
    Write-Host "Installing AzureADPreview"
    Install-Module AzureADPreview -AllowClobber
}

# Install-module MicrosoftTeams if needed
if (Get-Module -ListAvailable -Name MicrosoftTeams) {
    Write-Host "Module MicrosoftTeams exists"
}
else {
    Write-Host "Installing MicrosoftTeams"
    Install-Module MicrosoftTeams -AllowClobber
}

# Connect to AzureAD & MicrosoftTeams
Connect-AzureAD
Connect-MicrosoftTeams

# Get all members in the group, guests are not included
$members = Get-AzureADGroupMember -ObjectId $groupObjectId -All $true | Where-Object { $_.ObjectType -eq "User" -and $_.UserType -ne "Guest" }

# Apply the app permission policy to all the members
Foreach ($member in $members) {
    Write-Host Assigning Teams App Permission Policy `"$appPolicyname`" to $member.DisplayName
    Grant-CsTeamsAppPermissionPolicy -PolicyName $appPolicyname -Identity $member.UserPrincipalName
}

# Apply the app setup policy to all the members
Foreach ($member in $members) {
    Write-Host Assigning Teams App Setup Policy `"$setupPolicyname`" to $member.DisplayName
    Grant-CSTeamsAppSetupPolicy -PolicyName $setupPolicyname -Identity $member.UserPrincipalName
}

# Disconnect from AzureAD & MicrosoftTeams
Disconnect-MicrosoftTeams
Disconnect-AzureAD