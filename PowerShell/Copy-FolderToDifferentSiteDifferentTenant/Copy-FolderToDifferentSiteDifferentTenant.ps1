Param(
    [Parameter(Mandatory=$true)]
    [string]$inputCsv
)



#region Global settings
(New-Object -TypeName System.Net.WebClient).Proxy.Credentials = [System.Net.CredentialCache]::DefaultNetworkCredentials
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Import-Module Sharegate
#endregion

#region Variables
$srcSiteUrl
$dstSiteUrl
$srcFolder
$dstFolder
$processedRowCount = 0
#endregion

#region Functions
# Function to process each row and return an array of objects
function ProcessRows($inputTable) {
        $processedRows = New-Object System.Collections.Generic.List[PSObject]

    foreach ($row in $inputTable) {
        if([string]::IsNullOrWhiteSpace($row.srcSiteURL)) {
            continue
        }


        $rowData = [PSCustomObject]@{
            SrcSiteUrl = $row.srcSiteURL
            DstSiteUrl = $row.dstSiteUrl
            SrcFolder = $row.srcFolder
            DstFolder = $row.dstFolder
        }

        $processedRows.Add($rowData)
    }

    return $processedRows
}

# Function to copy content and optionally delete source folder
function CopyContent($srcSiteUrl, $dstSiteUrl, $srcFolder, $dstFolder, $dstConnection, $srcConnection) {
        $dstList = Get-List -Site $dstConnection -name "Documents"

        $srcList = Get-List -Site $srcConnection -name "Documents"

    try {
        $copysettings = New-CopySettings -OnContentItemExists IncrementalUpdate

        Copy-Content -SourceList $srcList -DestinationList $dstList -SourceFolder $srcFolder -DestinationFolder $dstFolder -TaskName "($srcFolder) to ($dstFolder)" -CopySettings $copysettings
    } catch {
        $errorMessage = "ERROR: Something went wrong while copying (index $processedRowCount):`n$_"
        Write-Host $errorMessage -ForegroundColor Red
        $errorMessage | Out-File -Append -FilePath "$PWD\log.txt"
    }
}

#endregion

try {
    # Load input csv
    $inputTable = Import-Csv $inputCsv -Delimiter ";"
}
catch {
    Write-Host "Error: Unable to import CSV. Script will now exit."
    exit
}

# Process input csv to rows
$rows = ProcessRows $inputTable

# Sort rows based on source and destination URLs
$sortedRows = $rows | Sort-Object SrcSiteUrl, DstSiteUrl


# Iterate through each row and process it
foreach ($row in $sortedRows) {
    Write-Host "INFO: Start with processing of row with index $processedRowCount" -ForegroundColor Green
    Write-Host "Processing Row:"
    Write-Host "SrcSiteUrl: $($row.SrcSiteUrl)"
    Write-Host "DstSiteUrl: $($row.DstSiteUrl)"
    Write-Host "SrcFolder: $($row.SrcFolder)"
    Write-Host "DstFolder: $($row.DstFolder)"

    if($processedRowCount -eq 0)
    {
        $srcConnection = Connect-site -Url $row.SrcSiteUrl -Browser -DisableSSO
        $dstConnection = Connect-Site -Url $row.DstSiteUrl -Browser -DisableSSO
    }
    else{
        $srcConnection = Connect-site -Url $row.SrcSiteUrl -UseCredentialsFrom $srcConnection
        $dstConnection = Connect-Site -Url $row.DstSiteUrl -UseCredentialsFrom $dstConnection
    }

    
    CopyContent -srcSiteUrl $row.SrcSiteUrl -dstSiteUrl $row.DstSiteUrl -srcFolder $row.SrcFolder -dstFolder $row.DstFolder -dstConnection $dstConnection -srcConnection $srcConnection
    $processedRowCount++

    Write-Host "INFO: End of processing row, total processed: $processedRowCount" -ForegroundColor Green
}