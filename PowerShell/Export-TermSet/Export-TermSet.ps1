param ( 
      [Parameter(Mandatory=$true)][string]$ClientId,
      [Parameter(Mandatory=$true)][string]$TenantName,
      [Parameter(Mandatory=$true)][string]$TermGroupName,
      [Parameter(Mandatory=$true)][string]$TermSetName,
      [Parameter(Mandatory=$false)][string]$OutputPath,
      [Parameter(Mandatory=$false)][int[]]$LocaleIds
)

# Connect to SharePoint Online
$tenantUrl = "https://$TenantName-admin.sharepoint.com"
Write-Host -f yellow "Connecting to SharePoint online: '$tenantUrl'"
Connect-PnPOnline -Url $tenantUrl -ClientId $ClientId -Interactive -Verbose

# Initialize an empty array to store term objects
$allTermObjects = New-Object System.Collections.ArrayList

# Function to get term details recursively
function Get-Term {
    param ( [string]$termId, [int[]]$LocaleIds )

    # Process main term
    $mainTerm = Get-PnPTerm -Identity $termId -TermSet $TermSetName -TermGroup $TermGroupName -Includes Labels,PathOfTerm -IncludeChildTerms -Recursive
    $pathOfTerm = $mainTerm.PathOfTerm -replace ";", "|"
    Write-Host -f magenta "Processing term with: '$($mainTerm.Id)' - $pathOfTerm"

    # Set base term properties
    $termData = [ordered] @{
        "TermId" = $mainTerm.Id
        "TermName" = $mainTerm.Name
        "PathOfTerm" = $pathOfTerm
    }

    # Process labels/translations
    if ($PSBoundParameters.ContainsKey("LocaleIds")) {
        foreach ($localeId in $LocaleIds) {
            Write-Host -f yellow "  Adding labels for locale: '$localeId'"
        
            # Get the default label for the locale
            $defaultLabel = $mainTerm.Labels | Where-Object { $_.Language -eq $localeId -and $_.IsDefaultForLanguage -eq $True }
            $defaultLabelSafe = (!$defaultLabel.Value ? $null : $defaultLabel.Value) ?? "-"

            # Get the non default labels for the locale
            $nonDefaultLabels = $mainTerm.Labels | Where-Object { $_.Language -eq $localeId -and $_.IsDefaultForLanguage -eq $False } 
            $nonDefaultLabelsMerged = ($nonDefaultLabels | ForEach-Object { $_.Value }) -join '|'
            $nonDefaultLabelsSafe = (!$nonDefaultLabelsMerged ? $null : $nonDefaultLabelsMerged) ?? "-"
            
            # Add the labels to the data
            $termData.Add("$($localeId)_default", $defaultLabelSafe)
            $termData.Add("$($localeId)_other", $nonDefaultLabelsSafe)
        }

    }
    
    # Add the data to the global list of items
    $allTermObjects.Add($termData) | Out-Null

    # Process children if any
    if ($mainTerm.TermsCount -gt 0) {
      $childTerms = $mainTerm.Terms
      foreach ($t in $childTerms) {
          Get-Term -termId $t.Id -LocaleIds $LocaleIds
      }
    }    
}

# Get all terms in the term set
$allTerms = Get-PnPTerm -TermSet $TermSetName -TermGroup $TermGroupName
foreach ($term in $allTerms) {
    Get-Term -termId $term.Id -LocaleIds $LocaleIds
}

# Disconnect from the service
Disconnect-PnPOnline

# Build the export path
$output = $PSScriptRoot
if ($PSBoundParameters.ContainsKey("OutputPath")) { $output = $OutputPath }
$outputFilePath = "$output/$TermGroupName-$TermSetName.csv"

# Export the data to CSV
$allTermObjects | Export-Csv -Path "$output/$TermGroupName-$TermSetName.csv" -NoTypeInformation -Encoding Unicode -Delimiter ";"

# Print the full path to the console
Write-Output -f green "The CSV file has been saved to: $outputFilePath"