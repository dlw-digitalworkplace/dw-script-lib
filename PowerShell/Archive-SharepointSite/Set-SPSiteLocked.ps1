Param(
	[Parameter(Mandatory = $true)]
	[string] $TenantName # e.g. mytenant
)

function Set-SPSiteLocked {
	param($siteUrl)

	# https://sympmarc.com/2022/03/10/sharepoint-site-lock-and-remove-from-search-results/

	Write-Host "Processing site $siteUrl."

	# Connect to the SharePoint site
	$siteConnection = Connect-PnPOnline -Url $siteUrl -Interactive -ReturnConnection

	
	$site = Get-PnPSite -Connection $siteConnection -Includes WriteLocked | Select-Object WriteLocked
	if ($site.WriteLocked) {
		Write-Host -ForegroundColor DarkGray "Site is already locked. Skipping."
		return
	}

	# Allow custom scripts
	Set-PnPSite -Connection $siteConnection -DenyAddAndCustomizePages $false

	# Exclude site from search index
	Write-Host -ForegroundColor Gray "Excluding site from Search..." -NoNewLine

	$web = Get-PnPWeb -Connection $siteConnection
	$web.NoCrawl = $true
	$web.Update()
	Invoke-PnPQuery

	Write-Host -ForegroundColor Green " Done."

	# Lock the site collection
	Write-Host -ForegroundColor Gray "Locking site..." -NoNewLine

	Set-PnPSite -Connection $siteConnection -LockState ReadOnly -WarningAction SilentlyContinue

	Write-Host -ForegroundColor Green " Done."
}

# Ask for confirmation
Write-Host -ForegroundColor Yellow "You are about to lock all sites in extra file)'."
$confirmation = Read-Host "Are you sure you want to proceed? (Y/N)"
if ($confirmation -ne "Y" -and $confirmation -ne "y") {
	Write-Host "Script execution aborted."
	exit
}

$spRoot = "https://$tenantName.sharepoint.com"

$adminSiteUrl = "https://$tenantName-admin.sharepoint.com"
$adminConnection = Connect-PnPOnline -Url $adminSiteUrl -Interactive -ReturnConnection

$allSitesFile = ".\AllSites.csv"
$allSites = Import-Csv $allSitesFile -Delimiter "@"
Write-Host ($allSites | Format-Table | Out-String)
foreach ($site in $allSites) {
	Set-SPSiteLocked -siteUrl ($spRoot + $site.TeamSiteUrl)
}