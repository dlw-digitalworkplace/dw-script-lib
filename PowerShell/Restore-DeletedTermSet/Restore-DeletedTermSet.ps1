param ($TaxonomyHiddenListId, $RemovedTermSetId, $RemovedTermSetName, $RemovedTermSetGroupName, $SiteUrl)

Connect-PnPOnline -Url $SiteUrl -Interactive
$Lcid = 1033
$TaxonomyHiddenList = Get-PnPList -Identity $TaxonomyHiddenListId
$TaxonomyHiddenListitems = (Get-PnPListItem -List $TaxonomyHiddenListId -PageSize 50).FieldValues | where{$_.IdForTermSet -eq $RemovedTermSetId}

$TermSet = Get-PnPTermSet -Identity $RemovedTermSetId -TermGroup $RemovedTermSetGroupName -ErrorAction SilentlyContinue

if($TermSet -eq $null){
    New-PnPTermSet -Name $RemovedTermSetName -TermGroup $RemovedTermSetGroupName -Id $RemovedTermSetId -Lcid $lcid
}

$TermsCount = $TaxonomyHiddenListitems.Count
$TermsDone = 0

#Add all root terms that have been referenced directly
[array]$RootTermsUsedDirectly = $TaxonomyHiddenListitems | Where{$_.Path.split(":").count -eq 1}
foreach($RootTermUsedDirectly in $RootTermsUsedDirectly){
    try{
        $TempRootTerm = Get-PnPTerm -Identity $RootTermUsedDirectly.IdForTerm -TermSet $RemovedTermSetId -TermGroup $RemovedTermSetGroupName -Recursive -ErrorAction SilentlyContinue
        if($TempRootTerm -eq $null){
            New-PnPTerm -TermGroup $RemovedTermSetGroupName -TermSet $RemovedTermSetId -Name $RootTermUsedDirectly.Term -Id $RootTermUsedDirectly.IdForTerm -Lcid $Lcid
        }
        $TermsDone++
    }
    catch{
        Write-Error "Something went wrong while creating term with path: $Term.Path"
        Write-Error $_
        $TermsDone++
    }
}

#Add all root terms that have childs who are used
[array]$RootTerms = $RootTermsUsedDirectly | ForEach-Object { $_.Path.Split(':')[0] } | Select-Object -Unique
[array]$RootTermsUsedAsParent = $TaxonomyHiddenListitems | where{$_.Path.Split(':')[0] -notin $RootTerms} | ForEach-Object { $_.Path.Split(':')[0] } | Select-Object -Unique
foreach($RootTermUsedAsParent in $RootTermsUsedAsParent){
    try{
        $TempRootTermByLabel = Get-PnPTerm -Identity $RootTermUsedAsParent -TermSet $RemovedTermSetId -TermGroup $RemovedTermSetGroupName -Recursive -ErrorAction SilentlyContinue
        if($TempRootTermByLabel -eq $null){
            New-PnPTerm -TermGroup $RemovedTermSetGroupName -TermSet $RemovedTermSetId -Name $RootTermUsedAsParent -Lcid $Lcid
        }
        $TermsDone++
    }
    catch{
        Write-Error "Something went wrong while creating term with path: $Term.Path"
        Write-Error $_
        $TermsDone++
    }
}

#Add all child terms
$Increment = 1
Do{
    foreach($Term in $TaxonomyHiddenListitems){
        $PathWithoutTitle = $Term.Path.Replace($term.Term,"")
        $CharCount = ($PathWithoutTitle.ToCharArray() | Where-Object {$_ -eq ':'} | Measure-Object).Count

        if($CharCount -eq $Increment){
            try{
                $TempChilTerm = Get-PnPTerm -Identity $Term.IdForTerm -TermSet $RemovedTermSetId -TermGroup $RemovedTermSetGroupName -Recursive -ErrorAction SilentlyContinue
                if($TempChilTerm.Id -eq $null){
                    $ParentTermPath = $PathWithoutTitle.Substring(0,$PathWithoutTitle.Length-1)
                    $ParentTerm = $TaxonomyHiddenListitems | where{$_.Path -eq $ParentTermPath}
                    if($ParentTerm.Count -eq 0){
                        $ParentTermPathSplit = $ParentTermPath.Split(":")

                        foreach($TermSplit in $ParentTermPathSplit){
                            $Temp = Get-PnPTerm -Identity $TermSplit -TermSet $RemovedTermSetId -TermGroup $RemovedTermSetGroupName -Recursive -ErrorAction SilentlyContinue

                            if($Temp -ne $null){
                                $ExistingTerm = $Temp
                            }

                            else{
                                $ExistingTerm = Add-PnPTermToTerm -ParentTermId $ExistingTerm.Id -Name $TermSplit -Id (New-Guid).Guid -Lcid $Lcid
                            }
                        }
                        Add-PnPTermToTerm -ParentTermId $ExistingTerm.Id -Name $Term.Term -Id $Term.IdForTerm -Lcid $Lcid
                    }
                    else{
                        Add-PnPTermToTerm -ParentTermId $ParentTerm.IdForTerm -Name $Term.Term -Id $Term.IdForTerm -Lcid $Lcid
                    }
                }
                $TermsDone++
            }
            catch{
                Write-Error "Something went wrong while creating term with path: $Term.Path"
                Write-Error $_
                $TermsDone++
            }
        }
    }
    $Increment++
}
while($TermsDone -lt $TermsCount)