$usersList = ".\fileMapping.csv"#Containing relative file path of source
Import-Module -Name PnP.Powershell -UseWindowsPowerShell 
# Install-Module PnP.PowerShell
# Install-Module -Name Microsoft.Identity.Client -RequiredVersion 4.50.0.0
$filesToCopy = Import-Csv $usersList -Delimiter ";"

$sourceSiteUrl = "https://imecinternational.sharepoint.com/sites/source"
$sourceRelativeSiteUrl = "/sites/source/Shared%20Documents/"
$destinationRelativeSiteUrl = "/sites/destination/Shared%20Documents/"

Connect-PnPOnline -Url $sourceSiteUrl -Interactive
foreach ($url in $filesToCopy) {
    $currentUrl = $sourceRelativeSiteUrl + $url.Source
    Write-Host $currentUrl
    $destinationFolder = $destinationRelativeSiteUrl + $url.Source.Substring(0, $url.Source.LastIndexOf('/'))
    Write-Host $destinationFolder
    Copy-PnPFile -SourceUrl ($currentUrl) -TargetUrl ($destinationFolder) -Overwrite -Force
}
Disconnect-PnPOnline