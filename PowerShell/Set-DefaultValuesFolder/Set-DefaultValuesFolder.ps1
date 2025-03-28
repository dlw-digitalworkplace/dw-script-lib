# This script connects to a SharePoint site and sets default column values
# for folders in a document library based on data from CSV files.

Param(
    [Parameter(Mandatory = $true)]
    [string]$clientId,
    [Parameter(Mandatory = $true)]
    [string]$siteUrl,
    [Parameter(Mandatory = $true)]
    [string]$libraryName,
    [Parameter(Mandatory = $true)]
    [string]$pathToDataCsv, # Path to the data CSV file containing folder values (see example defaultColumnValues.csv)
    [Parameter(Mandatory = $true)]
    [string]$pathToConfigCsv, # Path to the config CSV file mapping columns to SharePoint fields (see example config.csv)
    [Parameter(Mandatory = $true)]
    [string]$folderKey #The key representing the folder name in the data CSV
)

Connect-PnpOnline -Url $siteUrl -Interactive -ClientId $clientId

$config = Import-Csv -Path $pathToConfigCsv -Delimiter ";"

# Function to set a default column value for a folder in the document library
function Set-DefaultFieldValue {
    param (
        [string]$folderPath,
        [string]$folderRelativeURL,
        [string]$field,
        [string]$value
    )

    try {
        Set-PnPDefaultColumnValues -List $libraryName -Field $field -Value $value -Folder $folderPath
        Set-PnPDefaultColumnValues -List $libraryName -Field $field -Value $value -Folder $folderRelativeURL
    }
    catch {
        Write-Error "Failed to set value for '$field' in folder '$folderPath'. Error: $_"
    }
}

$data = Import-Csv -Path $pathToDataCsv -Delimiter ";"

# Loop over each row in the data CSV which contains folder information
foreach ($row in $data) {
    try {
        $folderPath = $row.$folderKey
        $folderRelativeURL = "$siteUrl/$libraryName/$folderPath"

        # Loop over each config item to set the corresponding field value for the folder
        foreach ($configItem in $config) {
            $csvColumn = $configItem.csvColumn # Column name from the CSV
            $spField = $configItem.sharePointField # SharePoint field to update
            $isManagedMetadata = [boolean]::Parse($configItem.isManagedMetadata) # Check if the field is managed metadata

            # Ensure that the column exists in the current row
            if ($row.PSObject.Properties.Name -contains $csvColumn) {
                $value = $row.$csvColumn

                # If the field is managed metadata, split the value by '|' to extract the desired part
                if ($isManagedMetadata) {
                    $value = ($value -split '\|')[1]
                }

                if ($spField) {
                    Set-DefaultFieldValue -folderPath $folderPath `
                        -folderRelativeURL $folderRelativeURL `
                        -field $spField `
                        -value $value
                }
            }
        }

        Write-Output ("Default values set for folder: $folderPath")
    }
    catch {
        Write-Error "An error occurred while setting values for folder: $($row.folder). Error: $_"
    }
}