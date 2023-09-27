Param(
    [Parameter(Mandatory=$true)]
    [string]$subscriptionId,
    [Parameter(Mandatory=$true)]
    [string]$keyVaultName,
    [Parameter(Mandatory=$true)]
    [string]$fileName,
    [Parameter(Mandatory=$true)]
    [string]$sheetName,
    [string]$location
)

Login-AzureRmAccount -SubscriptionId $subscriptionId

# Open Excel file
$objExcel = New-Object -ComObject Excel.Application
$location = Get-Location
$filePath = "$($location)/$($fileName)"
Write-Host $filePath
$workbook = $objExcel.Workbooks.Open($filePath)
$sheet = $workbook.Worksheets.Item($sheetName)
$objExcel.Visible=$false

$rowMax = ($sheet.UsedRange.Rows).count
Write-Host -f Yellow "Checked excel and found $($rowMax) rows!!"

# Start index
$rowName,$colName = 2,1

for ($i=0; $i -le $rowMax; $i++)
{
	# Get the web url
	$key = $sheet.Cells.Item($rowName+$i,$colName).text
    $value = $sheet.Cells.Item($rowName+$i,$colName+1).text	        
     
    if ($key -eq "" -or $value -eq "") {
        Write-Host -f Magenta "Skipping empty row"
    } else {
        Write-Host -f Yellow "Start Adding: '$($key) - $($value)' ..."
        $secret = ConvertTo-SecureString -String $value -AsPlainText -Force
        $kvSecret = Set-AzureKeyVaultSecret -VaultName $keyVaultName -Name $key -SecretValue $secret        
        Write-Host -f Green "Secret with id '$($kvSecret.Id)' successfully added!"
    }
    
}
     
$objExcel.quit()
