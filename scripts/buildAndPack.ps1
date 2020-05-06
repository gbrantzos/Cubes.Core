# ------------------------------------------------------------------------------
# Location variables
$originalPath = Get-Location
$workingPath  = ($PSScriptRoot)
# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Methods
function Tag {
    # To be defined by branch....
    # master should return nothing
    if ($gitBranch.StartsWith("master")) {
        return ""
    }

    # release should return -rc<count> of commits, i.e. -rc3
    if ($gitBranch.StartsWith("release")) {
        $commits = (git rev-list --count HEAD ^develop)
        return "-rc$commits"
    }

    # all others should return -dev-<timestamp>
    return "-dev-$timeStamp"
}

function BuildConfig {
    # To be defined by branch....
    # master and release/* should return Release
    if ($gitBranch.StartsWith("master") -or $gitBranch.StartsWith("release")) {
        return "Release"
    }

    # others should return Debug
    return "Debug"
}

# Create folder if missing
function CreateFolder {
    param ([string] $folderName)
    if(!(test-path $folderName)) {
        New-Item -ItemType Directory -Force -Path $folderName
    }

}

# Clear temp folder
function ClearTemp {
    $tgtPath = Join-Path -Path $workingPath -ChildPath '../tmp'
    if(Test-Path $tgtPath) {
        Remove-Item $tgtPath -Recurse
    }
}

# Clean exit
function Finish {
    param ([int] $code = 0)
    Set-Location $originalPath

    if ($code -eq 0) { Write-Output 'Finished!' }
    exit $code
}
# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Setup

# TODO Shall we store them globally????
$major = 5
$minor = 1
$patch = 2
$gitHash   = (git rev-parse HEAD).Substring(0, 10)
$gitBranch = (git rev-parse --abbrev-ref HEAD)
$timeStamp = Get-Date -Format 'yyyyMMddHHmm'

$buildConfig    = BuildConfig
$tag            = Tag
$nugetServer    = "http://baget.gbworks.lan/v3/index.json"
$nugetServerKey = "GbWorks@ApiKey!"
# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Prepare version numbers

$version="$major.$minor.$patch"
$assemblyVersion=$version
$fileVersion="$major.$minor.$patch"
$informationalVersion="$major.$minor.$patch$tag.$gitHash"

Write-Output 'Versions'
Write-Output ''
Write-Output "Assembly Version      : $assemblyVersion"
Write-Output "File Version          : $fileVersion"
Write-Output "Informational Version : $informationalVersion"
# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Build

Write-Output 'Building project...'

$srcPath = Join-Path -Path $workingPath -ChildPath '../src'
dotnet clean -c $buildConfig "$srcPath/Cubes.Core/Cubes.Core.csproj"
dotnet build -c $buildConfig "$srcPath/Cubes.Core/Cubes.Core.csproj" `
  -p:AssemblyVersion=$assemblyVersion `
  -p:FileVersion=$fileVersion `
  -p:InformationalVersion=$informationalVersion
dotnet clean -c $buildConfig "$srcPath/Cubes.Host/Cubes.Host.csproj"
dotnet build -c $buildConfig "$srcPath/Cubes.Host/Cubes.Host.csproj" `
  -p:AssemblyVersion=$assemblyVersion `
  -p:FileVersion=$fileVersion `
  -p:InformationalVersion=$informationalVersion
# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Create packages

Write-Output 'Creating package ...'

$srcPath = Join-Path -Path $workingPath -ChildPath '../src'
$tgtPath = Join-Path -Path $workingPath -ChildPath '../tmp'
ClearTemp
CreateFolder $tgtPath
dotnet publish "$srcPath/Cubes.Host/Cubes.Host.csproj" `
  --no-build `
  -o $tgtPath/Cubes-v$version `
  -c $buildConfig

# Cubes host package
$srcPath = Join-Path -Path $workingPath -ChildPath "../tmp/Cubes-v$Version"
$tgtPath = Join-Path -Path $workingPath -ChildPath '../deploy'
Compress-Archive -Path $srcPath/* `
  -CompressionLevel Optimal `
  -DestinationPath $tgtPath/Cubes-v$Version$tag.zip `
  -Force

# Cubes nuget
$srcPath = Join-Path -Path $workingPath -ChildPath '../src'
$tgtPath = Join-Path -Path $workingPath -ChildPath '../deploy'
dotnet pack "$srcPath/Cubes.Core/Cubes.Core.csproj" `
  --no-build `
  -c $buildConfig `
  -p:PackageVersion=$version$tag `
  -o $tgtPath
dotnet nuget push `
  -s $nugetServer `
  -k $nugetServerKey `
  "$tgtPath/Cubes.Core.$version$tag.nupkg"
# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Clean Exit

ClearTemp
Finish
# ------------------------------------------------------------------------------
