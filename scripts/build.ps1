# Prepare environment
$Hash = (git rev-parse HEAD)
$Hash = $Hash.Substring(0, 10)
$Directory = ($PSScriptRoot)


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


# User informations
Write-Output "Preparing deploy for Git commit: $HASH"
Write-Output "Working directory: $DIRECTORY"


# Make sure we wrk on the correct path
Set-Location $Directory


# Check for uncommited files
$ChangedFiles = $(git status --porcelain | Measure-Object | Select-Object -expand Count)
if ($ChangedFiles -gt 0)
{
    Write-Output "There are uncommited files on workspace, aborting!"
    exit 1
}


# Rebuild
dotnet clean -c release ../src/Cubes.Core/Cubes.Core.csproj
dotnet build -c release ../src/Cubes.Core/Cubes.Core.csproj -p:AssemblyVersion=$Version -p:FileVersion=$Version -p:InformationalVersion=$Version-$Hash
dotnet clean -c release ../src/Cubes.Web/Cubes.Web.csproj
dotnet build -c release ../src/Cubes.Web/Cubes.Web.csproj   -p:AssemblyVersion=$Version -p:FileVersion=$Version -p:InformationalVersion=$Version-$Hash
dotnet clean -c release ../src/Cubes.Host/Cubes.Host.csproj
dotnet build -c release ../src/Cubes.Host/Cubes.Host.csproj -p:AssemblyVersion=$Version -p:FileVersion=$Version -p:InformationalVersion=$Version-$Hash


# Publish
New-Item -ItemType directory -Path ../tmp
Set-Location ../tmp
Remove-Item * -Recurse -Force
dotnet publish --no-build ../src/Cubes.Host/Cubes.Host.csproj -o $Directory/../tmp/Cubes-v$Version -c release


# Compress
$DeployPath = "../deploy"
If(!(test-path $DeployPath)) { New-Item -ItemType Directory -Force -Path $DeployPath }
Compress-Archive -Path Cubes-v$Version/* -CompressionLevel Optimal -DestinationPath ../deploy/Cubes-v$Version.zip -Force


# Increase build number
Set-Location $Directory
$line = Get-Content .\buildInfo.txt | Select-String "BUILD=" | Select-Object -ExpandProperty Line
$content = Get-Content .\buildInfo.txt
$content | ForEach-Object {$_ -replace $line,"BUILD=$Build"} | Set-Content .\buildInfo.txt
git commit -a -m "Bump build number"


# Finally
Remove-Item ../tmp -Recurse
exit 0
