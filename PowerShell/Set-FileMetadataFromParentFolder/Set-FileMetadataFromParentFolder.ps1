# Description
## This script updates the metadata of files based on the parent folder in a SharePoint document library. It ensures that files get the same metadata as their parent folder. Additionally, you have the option to define a start and end point and you can choose which metadata fields you want to copy.

# Attention points
## Try to execute the script on your own dev tenant before using somewhere else, script has only been used on "Managed Metadata", "	Single line of text", and "Number" column types
## Be cautious with libraries containing a large number of files(might take too much time to run everything once). consider using the start and end point options to process items in batches.

# Prerequisites
## Install PnP.PowerShell and make sure you have the necessary permissions to make changes to the files in the library.
## Confirm that the library is set up with consistent metadata fields on the folders. !!If there is no metadata on the parent folder, the metadata on its child files will be removed!!

# Note
## Files at the root level of the library will not be updated since they don’t have a parent folder to inherit metadata from.

# Variables: Parameters for running the script
Param(
  [Parameter(Mandatory = $true)]
  [string]$clientId, # Azure AD application client ID
  [Parameter(Mandatory = $true)]
  [string]$siteUrl, # URL of the SharePoint site
  [Parameter(Mandatory = $true)]
  [string]$libraryName, # Name of the document library
  [Parameter(Mandatory = $true)]
  [string[]]$fieldsToCheck, # Metadata fields we want to copy to the files
  [Parameter()]
  [int]$startId = -1, # Optional: Start point for file IDs, default value of -1
  [Parameter()]
  [int]$endId = -1 # Optional: End point for file IDs, default value of -1
)

# Connecting to SharePoint Online
Connect-PnPOnline -Url $siteUrl -Interactive -ClientId $clientId

# Fetch files from the library based on the provided start and end IDs
if (($startId -ne -1) -and ($endId -ne -1)) {
  $queryGetPnpListItem = @"
  <View Scope='RecursiveAll'>
    <Query>
        <Where>
          <And>
            <Geq>
              <FieldRef Name='ID'/>
              <Value Type='Number'>$startId</Value>
            </Geq>
            <Lt>
              <FieldRef Name='ID'/>
              <Value Type='Number'>$endId</Value>
            </Lt>
          </And>
        </Where>
    </Query>
  </View>
"@
  $files = Get-PnPListItem -List $libraryName -Query $queryGetPnpListItem -PageSize 1000
}
else {
  # Fetch all files if no ID range is specified
  $files = Get-PnPListItem -List $libraryName -PageSize 1000
}

$folderCache = @{}

# Process each file and update metadata based on parent folder
foreach ($fileItem in $files) {
  $fileName = $fileItem["FileLeafRef"]

  #skip folders, we only want to update files
  if ($fileItem.FileSystemObjectType -eq "Folder") {
    continue
  }

  $parentFolderUrl = $fileItem.FieldValues["FileDirRef"] # URL of the parent folder

  if (-not $folderCache.ContainsKey($parentFolderUrl)) {

    $query = @"
<View Scope='RecursiveAll'>
  <Query>
    <Where>
      <And>
        <Eq>
          <FieldRef Name='FSObjType'/>
          <Value Type='Integer'>1</Value>
        </Eq>
        <Eq>
          <FieldRef Name='FileRef'/>
          <Value Type='Text'>$parentFolderUrl</Value>
        </Eq>
      </And>
    </Where>
  </Query>
</View>
"@

    try {
      $folderItem = Get-PnPListItem -List $libraryName -Query $query
      $folderCache[$parentFolderUrl] = $folderItem
    }
    catch {
      Write-Host "Failed to retrieve folder item for $parentFolderUrl : $_"
      continue
    }
  }
  else {
    $folderItem = $folderCache[$parentFolderUrl]
  }

  $folderMetadata = $folderItem.FieldValues
  $fileMetadata = $fileItem.FieldValues
  $metadataToUpdate = @{}

  # Compare folder metadata with file metadata
  foreach ($field in $fieldsToCheck) {
    if ($folderMetadata.ContainsKey($field) -and $folderMetadata[$field] -ne $fileMetadata[$field]) {
      # Handle managed metadata fields differently, TermGuid is needed to update metadata
      if ($folderMetadata[$field] -is [Microsoft.SharePoint.Client.Taxonomy.TaxonomyFieldValue]) {
        if ($folderMetadata[$field].Label -ne $fileMetadata[$field].Label) {
          $termGuid = $folderMetadata[$field].TermGuid
          $metadataToUpdate[$field] = $termGuid
        }
      }
      else {
        $metadataToUpdate[$field] = $folderMetadata[$field]
      }
    }
  }

  # Update file metadata if there are changes
  if ($metadataToUpdate.Count -gt 0) {
    try {
      Set-PnPListItem -List $libraryName -Identity $fileItem.Id -Values $metadataToUpdate
    }
    catch {
      Write-Host "Failed to update metadata for file: $fileName - $_"
    }
  }
}
