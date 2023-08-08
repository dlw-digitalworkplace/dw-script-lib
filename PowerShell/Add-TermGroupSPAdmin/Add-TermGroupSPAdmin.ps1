param ($siteUrl, $termGroupName)

Add-Type -Path "C:\Program Files\SharePoint Online Management Shell\Microsoft.Online.SharePoint.PowerShell\Microsoft.SharePoint.Client.dll" 
Add-Type -Path "C:\Program Files\SharePoint Online Management Shell\Microsoft.Online.SharePoint.PowerShell\Microsoft.SharePoint.Client.Runtime.dll" 
Add-Type -Path "C:\Program Files\SharePoint Online Management Shell\Microsoft.Online.SharePoint.PowerShell\Microsoft.Online.SharePoint.Client.Tenant.dll"

# Connect to SharePoint Online
$Cred = Get-Credential
Connect-PnPOnline -Url $siteUrl -Credentials $Cred

$context = Get-PnPContext

# Get the taxonomy session
$taxonomySession = [Microsoft.SharePoint.Client.Taxonomy.TaxonomySession]::GetTaxonomySession($context)
$context.Load($taxonomySession)
$context.ExecuteQuery()

# Define the "app@sharepoint" username
$appAccountName = "i:0i.t|00000003-0000-0ff1-ce00-000000000000|app@sharepoint"

# Get the term store
$termStore = $taxonomySession.GetDefaultSiteCollectionTermStore()
$context.Load($termStore)
$context.ExecuteQuery()

# Get the term group
$termGroup = $termStore.Groups.GetByName($termGroupName)
$context.Load($termGroup)
$context.ExecuteQuery()

# Add the app as administrator to the term group
$termGroup.AddGroupManager($appAccountName)
$context.ExecuteQuery()

Write-Host "The app account has been added as an administrator."

# Disconnect from SharePoint Online
Disconnect-PnPOnline