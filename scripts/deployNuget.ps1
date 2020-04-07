# Variables
$OriginalPath = Get-Location
$WorkingPath  = ($PSScriptRoot)
$BaseVersion  = "5.0.1"

# Output file
$BuildInfoFile = "$WorkingPath\..\src\Cubes.Core\BuildInfo.txt"

# Build number
$BuildNumber = 0
Try
{
    $NumberLine = (Get-Content $BuildInfoFile | Select-String -Pattern 'Build').Line
    $separatorIndex = $NumberLine.IndexOf(':') + 1
    $BuildNumber = [int]($NumberLine.Substring($separatorIndex).Trim())
    $BuildNumber = $BuildNumber + 1
}
Catch
{
    throw "Could not get build number information!"
}

Set-Location -Path $WorkingPath\..\src\Cubes.Core
dotnet pack -p:PackageVersion=$BaseVersion-Build$BuildNumber
dotnet nuget push .\bin\Debug\Cubes.Core.$BaseVersion-Build$BuildNumber.nupkg -s S:\Nuget

Set-Location $OriginalPath
