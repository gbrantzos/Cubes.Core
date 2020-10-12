# ------------------------------------------------------------------------------
# Script parameters
param(
    [string] $version,
    [switch] $deploy,
    [switch] $allowDirty,
    [string] $configuration
)
# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Exit codes

#   0   OK
#   1   No Git TAG on repository
#   2   Invalid version format
#   3   Repository has pending changes

# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Variables

# Location 
$originalPath = Get-Location
$workingPath  = ($PSScriptRoot)
$rootPath     = (Get-Item $workingPath).Parent

# Project
$project      = "Cubes.Core"

# Build info
$gitHash      = (git rev-parse HEAD).Substring(0, 10)
$gitBranch    = (git rev-parse --abbrev-ref HEAD)
$timeStamp    = Get-Date -Format 'yyyyMMddHHmm'
$versionInfo  = "1.0.0-beta1"
$banner       = ""

# Folders
$outputFolder  = Join-Path -Path $rootPath -ChildPath "tmp/$project"
$packageFolder = Join-Path -Path $rootPath -ChildPath "deploy"

# Nuget server
$nugetServer = "http://baget.gbworks.lan/v3/index.json"
$nugetServerKey = "GbWorks@ApiKey!"

# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Methods

function Prepare-Version {
    $tmp    = ""
    $source = ""
    if (!$version) {
        $tag = (git tag --points-at HEAD) | sort length | select -first 1
        if (!$tag) {
            Write-Host "No Git TAG defined!"
            exit 1
        }         
        $tmp=$tag
        $source="Git"
    } else {
        $tmp=$version
        $source="parameters"
    }
    
    # Sanitize version, remove extra v
    if ($tmp.StartsWith("v")) { $tmp=$tmp.SubString(1) }
    
    # Check version format
    $re=[regex]"([0-9]\.[0-9]\.[0-9])(\-){0,1}((.)*)"
    $m=$re.Match($tmp)
    if (!$m.Success) {
        Write-Host "Invalid version format! $tmp"
        exit 2
    }
    
    $script:version = $m.Groups[1].Value
    $info = $m.Groups[3].Value
    if ($info -eq "") {
        $script:versionInfo="$script:version"
    } else {
        
        $script:versionInfo="$script:version-$info"
    }
    
    Write-Host "Version $script:version, full version $script:versionInfo (from $source)..."
    Write-Host "Git commit $gitHash, branch $gitBranch"
}    

# Check for GIT pending changes
function Pending-Changes {
    if ($allowDirty) { return }
    
    $st = (git status -su)
    if (![string]::ISNullOrEmpty($st)) {
        Write-Host "`n`nGit repository has pending changes!`nAborting..."
        exit 3
    }
}

# Initialize folder if missing
function Initialize-Folder {
    param ([string] $folderName)
    if (!(Test-Path $folderName)) {
        New-Item -ItemType Directory -Force -Path $folderName | Out-Null
    }
    Write-Host "Output folder '$outputFolder'`n`n"
}

# Clear temp folder
function Clear-Folder {
    param ([string] $path)
    if (Test-Path $path) { Remove-Item $path -Recurse }
}

# Initialize banner variable
function Initialize-Banner {
    $script:banner = "

   ______      __                 ______              
  / ____/_  __/ /_  ___  _____   / ____/___  ________ 
 / /   / / / / __ \/ _ \/ ___/  / /   / __ \/ ___/ _ \
/ /___/ /_/ / /_/ /  __(__  )  / /___/ /_/ / /  /  __/
\____/\__,_/_.___/\___/____/   \____/\____/_/   \___/ 
                                                      

"
}

# Display banner
function Display-Banner { Write-Host $banner }

# Build
function Build-Solution {
    $config = $(if ([string]::IsNullOrEmpty($configuration)) { "Release" } else { $configuration })
    
    $srcPath = Join-Path -Path $rootPath -ChildPath 'src'
    dotnet clean -c $config "$srcPath/Cubes.Core/Cubes.Core.csproj"
    dotnet build -c $config "$srcPath/Cubes.Core/Cubes.Core.csproj" `
        -p:AssemblyVersion=$version `
        -p:FileVersion=$version `
        -p:InformationalVersion=$versionInfo
    dotnet clean -c $config "$srcPath/Cubes.Host/Cubes.Host.csproj"
    dotnet build -c $config "$srcPath/Cubes.Host/Cubes.Host.csproj" `
        -p:AssemblyVersion=$version `
        -p:FileVersion=$version `
        -p:InformationalVersion=$versionInfo
        
    dotnet publish "$srcPath/Cubes.Host/Cubes.Host.csproj" `
        --no-build `
        -o $outputFolder `
        -c $config
}

# Build information file
function Write-BuildInformation {
    param ([string] $path)
    $buildInfo = "$banner`nGit commit $gitHash, branch $gitBranch`nBuild at $(Get-Date)"
    $filePath = Join-Path $outputFolder -ChildPath "BuildInformation.txt"
    $buildInfo | Out-File -Path $filePath
}

# Create ZIP file
function Create-Package {
    if (!(Test-Path $packageFolder)) {
        New-Item -ItemType Directory -Force -Path $packageFolder | Out-Null
    }
    
    Remove-Item "$outputFolder/appsettings.Development.json"
    Compress-Archive `
        -Path "$outputFolder\*" `
        -Destination "$packageFolder\$project-v$versionInfo.zip" `
        -Force
}

# Deploy package
function Deploy-Package {
    if ($deploy) {
        $config = $(if ([string]::IsNullOrEmpty($configuration)) { "Release" } else { $configuration })
        $srcPath = Join-Path -Path $rootPath -ChildPath 'src'
        
        dotnet pack "$srcPath/Cubes.Core/Cubes.Core.csproj" `
            --no-build `
            -c $config `
            -p:PackageVersion=$versionInfo `
            -o $outputFolder
        Copy-Item -Path "$outputFolder/$project.$versionInfo.nupkg" -Destination $packageFolder
        
        Write-Host 'Pushing nuget package...'
        dotnet nuget push `
            -s $nugetServer `
            -k $nugetServerKey `
            "$outputFolder/$project.$versionInfo.nupkg"
    }
}
# ------------------------------------------------------------------------------



# Banner, info
Initialize-Banner
Display-Banner

# If version not defined, get from Git
Prepare-Version

# Check for pending changes
Pending-Changes

# Prepare new output folder
Initialize-Folder $outputFolder

# Build using version as parameter
Build-Solution

# Add BuildInfo on output folder
Write-BuildInformation

# Create archive
Create-Package

# If asked, Deploy somewhere
Deploy-Package

# Cleanup
Clear-Folder $outputFolder
