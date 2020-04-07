$GitBranch = (git rev-parse --abbrev-ref HEAD)
$GitCommit = (git rev-parse HEAD).Substring(0, 10)

$OutFile = "BuildInfo.txt"

Get-Date -Format 'yyyy/MM/dd HH:mm:ss' | Out-File -FilePath $OutFile
Write-Output '' | Out-File -FilePath $OutFile -Append
Write-Output "Branch:  $GitBranch" | Out-File -FilePath $OutFile -Append
Write-Output "Commit:  $GitCommit" | Out-File -FilePath $OutFile -Append