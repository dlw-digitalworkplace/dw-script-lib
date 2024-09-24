param(
  [Parameter(Mandatory = $true)]
  [string]$tenantId,
  [Parameter(Mandatory = $true)]
  [string]$managedIdentityName,
  [Parameter(Mandatory = $true)]
  [string[]]$permissions
)

$GraphAppId = "00000003-0000-0000-c000-000000000000" # Don't change this.

Connect-AzureAD

$oMsi = Get-AzureADServicePrincipal -Filter "displayName eq '$managedIdentityName'"
$oGraphSpn = Get-AzureADServicePrincipal -Filter "appId eq '$GraphAppId'"

$oAppRole = $oGraphSpn.AppRoles | Where-Object {($_.Value -in $permissions)}

Connect-MgGraph -TenantId $tenantId -Scopes Application.Read.All, AppRoleAssignment.ReadWrite.All, RoleManagement.ReadWrite.Directory

foreach($AppRole in $oAppRole)
{
  $oAppRoleAssignment = @{
    "PrincipalId" = $oMSI.ObjectId
    "ResourceId" = $oGraphSpn.ObjectId
    "AppRoleId" = $AppRole.Id
  }
  
  New-MgServicePrincipalAppRoleAssignment `
    -ServicePrincipalId $oAppRoleAssignment.PrincipalId `
    -BodyParameter $oAppRoleAssignment `
    -Verbose
}