param (
    [Parameter(Mandatory=$true)][string]$sourceDirectory,
    [Parameter(Mandatory=$true)][string]$destinationDirectory,
    [Parameter(Mandatory=$false)][string]$artemisArgs
)

# Wait up to 10 seconds for the process to shut down
for ($i=1; $i -le 10; $i++) {
    $process = Get-Process -Name Artemis.UI.Windows -ErrorAction SilentlyContinue
    if (!$process) {
        break
    }
    Write-Host "Waiting for Artemis to shut down ($i / 10)"
    Start-Sleep -Seconds 1
}

# If the process is still running, kill it
$process = Get-Process -Name Artemis.UI.Windows -ErrorAction SilentlyContinue
if ($process) {
    Stop-Process -Id $process.Id -Force
    Start-Sleep -Seconds 1
}

# Check if the destination directory exists
if (!(Test-Path $destinationDirectory)) {
    Write-Error "The destination directory does not exist"
}

# If the destination directory exists, clear it
Get-ChildItem $destinationDirectory | Remove-Item -Recurse -Force

# Move the contents of the source directory to the destination directory
Get-ChildItem $sourceDirectory | Move-Item -Destination $destinationDirectory

Start-Sleep -Seconds 1

# When finished, run the updated version
if ($artemisArgs) {
    Start-Process -FilePath "$destinationDirectory\Artemis.UI.Windows.exe" -WorkingDirectory $destinationDirectory -ArgumentList $artemisArgs
} else {
    Start-Process -FilePath "$destinationDirectory\Artemis.UI.Windows.exe" -WorkingDirectory $destinationDirectory
}