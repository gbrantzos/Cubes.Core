# Output file
$OutFile = "BuildInfo.txt"

# Build number
$BuildNumber = 0
Try
{
    $NumberLine = (Get-Content $OutFile | Select-String -Pattern 'Build').Line
    $separatorIndex = $NumberLine.IndexOf(':') + 1
    $BuildNumber = [int]($NumberLine.Substring($separatorIndex).Trim())
    $BuildNumber = $BuildNumber + 1
}
Catch
{
    Write-Output "No build information found!"
}

# Git details
$GitBranch = (git rev-parse --abbrev-ref HEAD)
$GitCommit = (git rev-parse HEAD).Substring(0, 10)

# Output info to file
Get-Date -Format 'yyyy/MM/dd HH:mm:ss' | Out-File -FilePath $OutFile
Write-Output '' | Out-File -FilePath $OutFile -Append
Write-Output "Branch: $GitBranch"   | Out-File -FilePath $OutFile -Append
Write-Output "Commit: $GitCommit"   | Out-File -FilePath $OutFile -Append
Write-Output "Build : $BuildNumber" | Out-File -FilePath $OutFile -Append
