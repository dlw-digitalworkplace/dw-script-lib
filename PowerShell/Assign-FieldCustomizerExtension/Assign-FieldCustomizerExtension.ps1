param(
  [Parameter(Mandatory = $true)]
  [string]$siteCollectionUrl,
  [Parameter(Mandatory = $true)]
  [string]$fieldInternalName,
  [Parameter(Mandatory = $true)]
  [string]$clientSideComponentId,
  [Parameter(Mandatory = $false)]
  [string]$listName, 
  [Parameter(Mandatory = $false)]
  [switch]$updateExistingLists
)

# Install PnP.PowerShell v1.22 if not already installed
if (-not (Get-Module -Name "PnP.PowerShell" -ListAvailable)) {
  Install-Module -Name "PnP.PowerShell@1.22" -Scope CurrentUser -Force
}

# Connect to the site collection
Connect-PnPOnline -Url $siteCollectionUrl -Interactive

# Find the specified field
if ($listName -eq $null -or $listName -eq "") {
  $field = Get-PnPField -Identity $fieldInternalName
}
else {
  $field = Get-PnPField -Identity $fieldInternalName -List $listName
}

# Associate a field customizer
if ($null -eq $field) {
  Write-Host -ForegroundColor Red "Field '$fieldInternalName' not found"
}
else {
  if ($updateExistingLists) {
    Set-PnPField -Identity $field -Values @{ ClientSideComponentId = [GUID]$clientSideComponentId } -updateExistingLists
  }
  else {
    Set-PnPField -Identity $field -Values @{ ClientSideComponentId = [GUID]$clientSideComponentId }
  }
  Write-Host -ForegroundColor Green "Field '$fieldInternalName' updated"
}

# Disconnect from the site collection
Disconnect-PnPOnline