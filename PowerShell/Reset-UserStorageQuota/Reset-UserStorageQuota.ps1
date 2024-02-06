# Validate if module is available
$module = Get-Module -Name Microsoft.Online.SharePoint.PowerShell -ListAvailable | Select Name,Version
if($module -eq $NULL)
{
    Write-Host "Installing module Microsoft.Online.SharePoint.PowerShell because it is not available..." -ForegroundColor Yellow
    Install-Module -Name Microsoft.Online.SharePoint.PowerShell -Force
}


$tenantUrl = Read-Host "Enter the SharePoint admin center URL" 
Connect-SPOService -Url $tenantUrl 

$tenantSettings = Get-SPOTenant
$storageQuota = $tenantSettings.OneDriveStorageQuota

# Get all OneDrive sites of users
$personalOneDrives = Get-SPOSite -IncludePersonalSite $true -Limit all -Filter "Url -like '-my.sharepoint.com/personal/'" | Select -ExpandProperty Url

foreach($oneDriveSite in $personalOneDrives)
{
    $userSite = Get-SPOSite -Identity $oneDriveSite

    if($userSite.StorageQuota -ne $storageQuota)
    {
        $userQuota = $userSite.StorageQuota
        $userStorageUsage = $userSite.StorageUsageCurrent

        Write-Host "Site with url '$oneDriveSite' has different storage quota ($userQuota / $storageQuota (max. allowed))"
        Write-Host "Site with url '$oneDriveSite' the current used space is $userStorageUsage"

        Set-SPOSite -Identity $oneDriveSite -StorageQuotaReset
        Write-Host -ForegroundColor Green "Storage quota has been reset to tenant quota for site '$oneDriveSite'"
    }
}