# ------------------------------------------------------------------------------
# Script parameters

param(
    [Parameter(Mandatory)]
    [string]$version
)

# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Location variables

$originalPath = Get-Location
$workingPath = ($PSScriptRoot)

# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------

$tokens = @(
    'AssemblyVersion',
    'FileVersion',
    'InformationalVersion',
    'Version'
)

function Process-File {
    param ([string] $file)

    $srcPath = Join-Path -Path $workingPath -ChildPath '../src'
    $srcFile = "$srcPath/$file"
    $temp    = Get-Content $srcFile

    foreach ($token in $tokens) {
        $anchor    = "<$token>"
        $anchorEnd = "</$token>"
        
        $temp = $temp -replace "($anchor)(.*)($anchorEnd)", "`${1}$version`${3}"
    }

    $temp | Out-File $srcFile

}

Process-File Cubes.Core/Cubes.Core.csproj
Process-File Cubes.Host/Cubes.Host.csproj

$versionPath = Join-Path -Path $workingPath -ChildPath '../Version.txt'
$version | Out-File $versionPath

# ------------------------------------------------------------------------------
