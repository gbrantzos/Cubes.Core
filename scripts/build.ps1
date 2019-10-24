# Variables
$originalPath = Get-Location
$workingPath  = ($PSScriptRoot)
$hash         = (git rev-parse HEAD).Substring(0, 10)
$baseVersion  = "5.0.1"
$version      = "0.0.1"
$buildConfig  = "release"

# Output to user
function Log {
    param ([string] $info)

    Write-Output --------------------------------------------------------------------------------
    Write-Output $info
    Write-Output --------------------------------------------------------------------------------
    Write-Output ''
}

# Check for changes on given path
function UnsavedChanges {
    param([string] $path)

    $currentPath = Get-Location
    Set-Location $path

    $changedFiles = $(git status --porcelain | Measure-Object | Select-Object -expand Count)
    Set-Location $currentPath

    $ChangedFiles -gt 0
}

# Clean exit
function Finish {
    param ([int] $code = 0)
    Set-Location $originalPath

    if ($code -eq 0) { Log 'Finished!' }
    exit $code
}

# Confirm working on dirty repo
function CheckForDirtyRepo {
    Log "Working on $workingPath"

    $tgtPath = Join-Path -Path $workingPath -ChildPath '..'
    if (UnsavedChanges $tgtPath) {
        $message  = 'Uncommited Changes'
        $question = 'There are uncommited files on workspace, continue?'
        $choices  = '&Yes', '&No'

        $decision = $Host.UI.PromptForChoice($message, $question, $choices, 1)
        if ($decision -eq 1) {
            Log "There are uncommited files on workspace, aborting!"
            Exit 1
        }
    }
}

# Prepare version number
function PrepareVersion {
    $ver = "$baseVersion.$(Get-Date -Format 'yyyyMMdd')"
    $script:version = $ver

    Log "Version is: $script:version"
}

# Create folder if missing
function CreateFolder {
    param ([string] $folderName)
    if(!(test-path $folderName)) { New-Item -ItemType Directory -Force -Path $folderName }

}
# Build all
function Build {
    Log "Building version $version ..."

    $srcPath = Join-Path -Path $workingPath -ChildPath '../src'
    dotnet clean -c $buildConfig "$srcPath/Cubes.Core/Cubes.Core.csproj"
    dotnet build -c $buildConfig "$srcPath/Cubes.Core/Cubes.Core.csproj" -p:AssemblyVersion=$baseVersion -p:FileVersion=$baseVersion -p:InformationalVersion=$version-$hash
    dotnet clean -c $buildConfig "$srcPath/Cubes.Web/Cubes.Web.csproj"
    dotnet build -c $buildConfig "$srcPath/Cubes.Web/Cubes.Web.csproj"   -p:AssemblyVersion=$baseVersion -p:FileVersion=$baseVersion -p:InformationalVersion=$version-$hash
    dotnet clean -c $buildConfig "$srcPath/Cubes.Host/Cubes.Host.csproj"
    dotnet build -c $buildConfig "$srcPath/Cubes.Host/Cubes.Host.csproj" -p:AssemblyVersion=$baseVersion -p:FileVersion=$baseVersion -p:InformationalVersion=$version-$hash
}

# Publish
function Publish {
    Log "Publishing version $version ..."

    $srcPath = Join-Path -Path $workingPath -ChildPath '../src'
    $tgtPath = Join-Path -Path $workingPath -ChildPath '../tmp'
    CreateFolder $tgtPath

    dotnet publish --no-build "$srcPath/Cubes.Host/Cubes.Host.csproj" -o $tgtPath/Cubes-v$version -c release
}

# Create archive
function Pack {
    Log "Creating package for version $version ..."

    $srcPath = Join-Path -Path $workingPath -ChildPath "../tmp/Cubes-v$Version"
    $tgtPath = Join-Path -Path $workingPath -ChildPath '../deploy'
    CreateFolder $tgtPath

    Compress-Archive -Path $srcPath/* -CompressionLevel Optimal -DestinationPath $tgtPath/Cubes-v$Version.zip -Force
}

# Clear temp folder
function ClearTemp {
    $tgtPath = Join-Path -Path $workingPath -ChildPath '../tmp'
    if(Test-Path $tgtPath) { Remove-Item $tgtPath -Recurse }
}

# ------------------------------------------------------------------------------
# Main body

CheckForDirtyRepo
PrepareVersion
Build
ClearTemp
Publish
Pack

# Clean Exit
ClearTemp
Finish

# ------------------------------------------------------------------------------