param (
    [Parameter(Mandatory = $false)]
    [string]$ClassificationList,
    [Parameter(Mandatory = $true)]
    [string]$TenantId,
    [Parameter(Mandatory = $true)]
    [string]$ClientId,
    [Parameter(Mandatory = $true)]
    [string]$CertificateThumbprint 
)

# Setting value names
$ClassificationListSettingName = "ClassificationList"

Write-Host "INFO: Installing required modules" -ForegroundColor Green

# Install-module Microsoft.Graph if needed
if (Get-Module -ListAvailable -Name Microsoft.Graph) {
    Write-Host "INFO: Module Microsoft.Graph exists." -ForegroundColor Magenta
}
else {
    Write-Host "INFO: Installing Microsoft.Graph..." -ForegroundColor Magenta
    Install-Module Microsoft.Graph -AllowClobber -Repository PSGallery -Force
}

# Install-module Microsoft.Graph.Beta if needed
if (Get-Module -ListAvailable -Name Microsoft.Graph.Beta) {
    Write-Host "INFO: Module Microsoft.Graph.Beta exists." -ForegroundColor Magenta
}
else {
    Write-Host "INFO: Installing Microsoft.Graph.Beta..." -ForegroundColor Magenta
    Install-Module Microsoft.Graph.Beta -AllowClobber -Repository PSGallery -Force
}

Write-Host "INFO: Installed all required modules" -ForegroundColor Green

Connect-MgGraph -ClientId $ClientId -TenantId $TenantId -CertificateThumbprint $CertificateThumbprint

try {
    # Try to get the existing setting
    $Setting = Get-MgBetaDirectorySetting | Where-Object { $_.DisplayName -eq "Group.Unified"}

    if ($null -eq $Setting) {
        # Create a new setting
        Write-Host "INFO: No existing setting found, creating new setting" -ForegroundColor Yellow

        # Get the template setting ID
        $TemplateId = (Get-MgBetaDirectorySettingTemplate | Where-Object { $_.DisplayName -eq "Group.Unified" }).Id

        # Create the setting using the template
        New-MgBetaDirectorySetting -TemplateId $TemplateId

        # Get the newly created setting
        $Setting = Get-MgBetaDirectorySetting | Where-Object { $_.DisplayName -eq "Group.Unified"}

        Write-Host "INFO: Created new setting with ID '$($Setting.Id)' from template with ID '$TemplateId'" -ForegroundColor Yellow
    }

    # Check if EnableMIPLabels is set to true
    $EnableMIPLabels = ($Setting.Values | Where-Object { $_.Name -eq "EnableMIPLabels" }).Value
    if ($null -ne $EnableMIPLabels -and $EnableMIPLabels -eq "true") {
        Write-Host "WARNING: 'EnableMIPLabels' is set to 'true', classification settings will be ignored and sensitivity labels published in Microsoft Purview compliance portal will be used instead" -ForegroundColor Red
    }

    # Update the existing setting
    Write-Host "INFO: Updating setting with new values" -ForegroundColor Green

    # Set ClassificationList if not empty
    if ($null -ne $ClassificationList) {
        Write-Host "INFO: Setting '$ClassificationListSettingName'" -ForegroundColor Cyan

        # Create the parameters object
        $Params = @{
            Values = @(
                @{
                    Name = $ClassificationListSettingName
                    Value = $ClassificationList
                }
            )
        }

        # Update the setting
        Update-MgBetaDirectorySetting -DirectorySettingId $Setting.Id -BodyParameter $Params

        Write-Host "INFO: Set '$ClassificationListSettingName' with value '$ClassificationList'" -ForegroundColor Cyan
    }

    Write-Host "INFO: successfully updated directory settings. Exiting script" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: Failed to update setting. Exiting script" -ForegroundColor Red
}

# Disconnect
Disconnect-MgGraph
