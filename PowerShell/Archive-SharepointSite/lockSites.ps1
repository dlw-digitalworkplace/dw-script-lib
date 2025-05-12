$siteList = Get-Content -Path ".\AllSubsites.txt"


foreach ($siteUrl in $siteList) {
    try {
        # Connect to site
        Connect-PnPOnline -Url $siteUrl -Interactive

        # Make admin account a site owner
        # Add-PnPSiteCollectionAdmin -Owners "sp-temp-sa4@imecinternational.onmicrosoft.com"

        # Get Read-Role object
        $readRoleDefinitionBinding = Get-PnPRoleDefinition -Identity Read
            
        $web = Get-PnPWeb
        Write-Host Locking $web.Title... -BackgroundColor Red
            
        # If no unique role assignments (so permissions inherited), break inheritance
        Enable-PnPFeature -Identity 41e1d4bf-b1a2-47f7-ab80-d5d6cbba3092
        Get-PnPProperty -ClientObject $web -Property "HasUniqueRoleAssignments", "RoleAssignments"
        if ($false -eq $web.HasUniqueRoleAssignments) {
            $web.BreakRoleInheritance($True, $True)
            Get-PnPProperty -ClientObject $web -Property "HasUniqueRoleAssignments", "RoleAssignments"
        }
            
        foreach ($roleAssignmentWeb in $web.RoleAssignments) {
            Get-PnPProperty -ClientObject $roleAssignmentWeb -Property "RoleDefinitionBindings"
                
            $roleAssignmentWeb.RoleDefinitionBindings.RemoveAll()
            $roleAssignmentWeb.RoleDefinitionBindings.Add($readRoleDefinitionBinding)
            $roleAssignmentWeb.Update()
            Invoke-PnPQuery
        }
            
        $lists = Get-PnPList | Where-Object { $_.Hidden -eq $false }
        $listAmount = $lists.Count
        $listIndex = 1
        Write-Host Amount of items: $listAmount
        foreach ($list in $lists) {
            Get-PnPProperty -ClientObject $list -Property "HasUniqueRoleAssignments", "RoleAssignments"
                
            if (!$list.HasUniqueRoleAssignments) {
                $list.BreakRoleInheritance($True, $True)
                Get-PnPProperty -ClientObject $list -Property "HasUniqueRoleAssignments", "RoleAssignments"
            }
                
            if ($list.HasUniqueRoleAssignments) {
                foreach ($roleAssignmentList in $list.RoleAssignments) {
                    Get-PnPProperty -ClientObject $roleAssignmentList -Property "RoleDefinitionBindings"
                        
                    $roleAssignmentList.RoleDefinitionBindings.RemoveAll()
                    $roleAssignmentList.RoleDefinitionBindings.Add($readRoleDefinitionBinding)
                    $roleAssignmentList.Update()
                    Invoke-PnPQuery
                }
            }
                
            $items = @()
            $items = Get-PnPListItem -List $list -PageSize 2000
                
            foreach ($item in $items) {
                Get-PnPProperty -ClientObject $item -Property "HasUniqueRoleAssignments", "RoleAssignments"
                    
                if ($item.HasUniqueRoleAssignments) {
                    foreach ($roleAssignmentItem in $item.RoleAssignments) {
                        Get-PnPProperty -ClientObject $roleAssignmentItem -Property "RoleDefinitionBindings"
                            
                        $roleAssignmentItem.RoleDefinitionBindings.RemoveAll()
                        $roleAssignmentItem.RoleDefinitionBindings.Add($readRoleDefinitionBinding)
                        $roleAssignmentItem.Update()
                        Invoke-PnPQuery
                    }
                }
            }
            Write-Host Finished item $listIndex/$listAmount
            $listIndex++
        }
    }
    catch {
        $siteUrl | Out-File -FilePath "./logs.txt" -Append
    }
}