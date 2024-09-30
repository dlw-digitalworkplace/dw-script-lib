param(
  [Parameter(Mandatory = $true)]
  [string]$tenantId,
  [Parameter(Mandatory = $true)]
  [string]$managedIdentityObjectId,
  [Parameter(Mandatory = $true)]
  [string[]]$permissions
)


$GraphAppId = "00000003-0000-0000-c000-000000000000"


Connect-AzAccount -Tenant $tenantId

$servicePrincipal = Get-AzADServicePrincipal -ObjectId $managedIdentityObjectId

$graphServicePrincipal = Get-AzADServicePrincipal -ApplicationId $GraphAppId

$graphServiceAppRoles = $graphServicePrincipal.AppRole | Where-Object {$permissions -contains $_.Value }

foreach($appRoleToAssign in $graphServiceAppRoles)
{
    Write-Host "Assigning $($appRoleToAssign.Value) to $($servicePrincipal.DisplayName)..."
    try
    {
        New-AzADServicePrincipalAppRoleAssignment -ServicePrincipalId $servicePrincipal.Id -ResourceId $graphServicePrincipal.Id -AppRoleId $appRoleToAssign.Id
        Write-Host "Role assigned" -ForegroundColor Green
    }catch
    {
        Write-Error "Assignment failed"
    }
}