# Add-GraphPermissionsToManagedIdentity

## SYNOPSIS
This script can be used to add Graph Permissions to the Managed Identity of your App Service. Keep in mind that you need Global Admin permissions to run this script.

## SOURCE
https://github.com/dlw-digitalworkplace/dw-script-lib/tree/main/PowerShell/Add-GraphPermissionsToManagedIdentity

## AUTHORS
 - Name: Wout Torremans
 - Email: wout.torremans@delaware.pro
 - Name: Dimitri Bosteels
 - Email: dimitri.bosteels@delaware.pro

## SYNTAX
### {Example} (Default)
```powershell
./Add-GraphPermissionsToManagedIdentity-AzureADModule.ps1 -tenantId <string> -managedIdentityName <string> -permissions <string[]>
./Add-GraphPermissionsToManagedIdentity-AzModule.ps1 -tenantId <string> -managedIdentityObjectId <string> -permissions <string[]>
```

## Prerequisites AzureADModule
> The following PowerShell modules need to be installed
>  - AzureAD
>  - Microsoft.Graph

## Prerequisites AzModule
> The following PowerShell modules need to be installed
>  - Az

## Description
The following steps are executed:
1. A login pompt will be shown, you will need to login with a Global Admin of the tenant
2. The Microsoft Graph service principal and the service principal of the managed identity you provided will be retrieved
3. The correct App roles that are linked with the Microsoft Graph Service Principal are retrieved, if you provide a service principal that is not available it will not be added
4. A connection with Graph will be made with the following Scopes: Application.Read.All, AppRoleAssignment.ReadWrite.All, RoleManagement.ReadWrite.Directory
5. The retrieved App roles are added to the service principal of the managed identity you provided.

## EXAMPLES

### EXAMPLE 1
This example will add the Sites.ReadWrite.All and Directory.ReadWrite.All app roles to the managed identity with the name test on the tenant with id xxxx-xxxx-xxxx
```powershell
./Add-GraphPermissionsToManagedIdentity-AzureADModule.ps1 -tenantId xxxx-xxxx-xxxx -managedIdentityName test -permissions @("Directory.ReadWrite.All","Sites.ReadWrite.All")
./Add-GraphPermissionsToManagedIdentity-AzModule.ps1 -tenantId xxxx-xxxx-xxxx -managedIdentityObjectId xxxx-xxxx-xxxx -permissions @("Directory.ReadWrite.All","Sites.ReadWrite.All")
```

## Tags
 * MS Graph
 * Azure AD
 * Powershell