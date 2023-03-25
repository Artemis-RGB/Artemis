param (
    [Parameter(Mandatory = $true)][string]$sourceDirectory,
    [Parameter(Mandatory = $true)][string]$destinationDirectory,
    [Parameter(Mandatory = $false)][string]$artemisArgs
)

Write-Host "Artemis update script v1"

# Wait up to 10 seconds for the process to shut down
for ($i = 1; $i -le 10; $i++) {
    $process = Get-Process -Name Artemis.UI.Windows -ErrorAction SilentlyContinue
    if (!$process)
    {
        break
    }
    Write-Host "Waiting for Artemis to shut down ($i / 10)"
    Start-Sleep -Seconds 1
}

# If the process is still running, kill it
$process = Get-Process -Name Artemis.UI.Windows -ErrorAction SilentlyContinue
if ($process)
{
    Stop-Process -Id $process.Id -Force
    Start-Sleep -Seconds 1
}

# Check if the destination directory exists
if (!(Test-Path $destinationDirectory))
{
    Write-Error "The destination directory at $destinationDirectory does not exist"
    Exit 1
}

# Clear the destination directory but don't remove it, leaving ACL entries in tact
Write-Host "Cleaning up old version where needed"
Get-ChildItem $destinationDirectory | Remove-Item -Recurse -Force

# Move the contents of the source directory to the destination directory
Write-Host "Installing new files"
Get-ChildItem $sourceDirectory | Move-Item -Destination $destinationDirectory
# Remove the now empty source directory
Remove-Item $sourceDirectory

Write-Host "Finished! Restarting Artemis"
Start-Sleep -Seconds 1

# When finished, run the updated version
if ($artemisArgs)
{
    Start-Process -FilePath "$destinationDirectory\Artemis.UI.Windows.exe" -WorkingDirectory $destinationDirectory -ArgumentList $artemisArgs
}
else
{
    Start-Process -FilePath "$destinationDirectory\Artemis.UI.Windows.exe" -WorkingDirectory $destinationDirectory
}