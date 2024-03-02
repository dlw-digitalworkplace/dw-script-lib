
Param(
    [Parameter(Mandatory=$false)]
    [string]$spAdminUrl,
    [Parameter(Mandatory=$false)]
    [string]$siteCollectionUrl,
    [Parameter(Mandatory=$false)]
    [string]$siteCollectionUrlFilter,
    [switch]$multipleSiteCollections
)

# Install PnP.PowerShell v1.22 if not already installed
if (-not (Get-Module -Name "PnP.PowerShell" -ListAvailable)) {
  Install-Module -Name "PnP.PowerShell@1.22" -Scope CurrentUser -Force
}

if ($multipleSiteCollections) {
  # Sp admin url is required in case of all site collections
  if ($spAdminUrl -eq $null -or $spAdminUrl -eq "") {
    Write-Host -f Red "The SharePoint admin URL is required!"
    Exit
  }

  # Check if the site collection url filter is provided, if not show a warning asking to continue
  if ($siteCollectionUrlFilter -eq $null -or $siteCollectionUrlFilter -eq "") {
    Write-Host -f Magenta "You are about to reset the search index for all site collections. Do you want to continue? (Y/N)"
    $continue = Read-Host
    if ($continue -ne "Y") {
      Exit
    }
  }

  # Connect to the SharePoint admin center
  Write-Host -f Yellow "Connecting to SharePoint admin center: $spAdminUrl ..."
  Connect-PnPOnline -Url $spAdminUrl -Interactive

  # Get all site collections
  Write-Host -f Yellow "Getting all site collections ..."
  if ($siteCollectionUrlFilter -eq $null -or $siteCollectionUrlFilter -eq "") {
    $siteCollections = Get-PnPTenantSite
  } else {
    $siteCollections = Get-PnPTenantSite | Where-Object { $_.Url -like "*$($siteCollectionUrlFilter)*" }
  }
  Write-Host -f Green "Found $($siteCollections.Count) site collections"

  ForEach ($site in $siteCollections) {
    # Reset search index for the specified site collection
    Write-Host -f Yellow "Resetting search index for site: $($site.Url) ..."
    Connect-PnPOnline -Url $site.Url -Interactive
    Request-PnPReIndexWeb
    Write-Host -f Green "Resetted search index for site: $($site.Url)"
  }
} Else {
  # Site collection url is required in case of single site collection
  if ($siteCollectionUrl -eq $null -or $siteCollectionUrl -eq "") {
    Write-Host -f Red "The site collection URL is required!"
    Exit
  } 

  # Reset search index for the specified site collection
  Write-Host -f Yellow "Resetting search index for site: $siteCollectionUrl ..."
  Connect-PnPOnline -Url $siteCollectionUrl -Interactive
  Request-PnPReIndexWeb
  Write-Host -f Green "Resetted search index for site: $siteCollectionUrl"
}