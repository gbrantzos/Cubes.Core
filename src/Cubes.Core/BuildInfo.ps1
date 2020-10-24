# Output file
$OutFile = "BuildInfo.txt"

# Git details
$GitBranch = (git rev-parse --abbrev-ref HEAD)
$GitCommit = (git rev-parse HEAD).Substring(0, 10)

# Output info to file
Get-Date -Format 'yyyy/MM/dd HH:mm:ss' | Out-File -FilePath $OutFile
Write-Output '' | Out-File -FilePath $OutFile -Append
Write-Output "Branch: $GitBranch"   | Out-File -FilePath $OutFile -Append
Write-Output "Commit: $GitCommit"   | Out-File -FilePath $OutFile -Append
