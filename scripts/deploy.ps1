# Prepare
$Directory = ($PSScriptRoot)
Set-Location $Directory


# Check for uncommited files
$ChangedFiles = $(git status --porcelain | Measure-Object | Select-Object -expand Count)
if ($ChangedFiles -gt 0)
{
    Write-Output "There are uncommited files on workspace, aborting!"
    exit 1
}


# User informations
Write-Output "Working directory: $DIRECTORY"


# Initial values
$Version = 0.0.0
$Build = 0


# Get Version details
foreach($line in Get-Content .\buildInfo.txt) {
    if ($line.StartsWith("VERSION=")) { $Version = $line.Substring("VERSION=".Length) }
    if ($line.StartsWith("BUILD=")) { $Build = $line.Substring("BUILD=".Length) }
}


# Increase build number
$Build = [int]$Build +1
$Version = "$Version.$Build"
Write-Output "Version is $Version"


# Update build number file
$line = Get-Content .\buildInfo.txt | Select-String "BUILD=" | Select-Object -ExpandProperty Line
$content = Get-Content .\buildInfo.txt
$content | ForEach-Object {$_ -replace $line,"BUILD=$Build"} | Set-Content .\buildInfo.txt


# Commit changes, create tag
git commit -a -m "Bump build number"
git tag -a $Version -m "Version $Version"


# Run build
Invoke-Expression .\build.ps1

# Publish nuget