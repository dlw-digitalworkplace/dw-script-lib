#Progress bar for when a script needs to apply a lot of changes on different items/sites/records/...
$Items = @('Zero','One','Two','Three')
$ItemsCount = $Items.Count
$ItemsDone = 0

foreach($Item in $Items){
    Write-Progress -Activity "Amount of items done" -Status "$ItemsDone Completed" -PercentComplete (($ItemsDone/$ItemsCount)*100)
    #Remove sleep when using progrss bar. Now added so that the bar will be visible when running this part.
    start-sleep -Seconds 1
    $ItemsDone++
}
