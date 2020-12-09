# ------------------------------------------------------------------------------
# Script parameters

param(
    [Parameter(Mandatory)]
    [string]$Version
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

function Check-TagExistence {
    $status =  (git tag -l "v$Version")
      
    if (![string]::IsNullOrWhitespace($status)) 
    { 
        Write-Host  "Tag 'v$Version' already exsists in repository!`nAborting..."
        exit 1
    }
}

function Pending-Changes {
    # Check for GIT pending changes
    $st = (git status -su)
    if (![string]::ISNullOrEmpty($st)) {
        Write-Host "Git repository has pending changes!`nAborting..."
        exit 3
    }
}

function Validate-Version {
    # Check version format
    $re=[regex]"([0-9]\.[0-9]\.[0-9])(\-){0,1}((.)*)"
    $m=$re.Match($Version)
    if (!$m.Success) {
        Write-Host "Invalid version format! $Version!`nAborting..."
        exit 2
    }
}

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

function Bump-VersionAndTag {
    # Add and commit
    git add --all
    git commit -m "Bump version $version"
    git tag -a "v$version" -m "Version $version"
}

Validate-Version
Check-TagExistence
Pending-Changes

Process-File Cubes.Core/Cubes.Core.csproj
Process-File Cubes.Host/Cubes.Host.csproj

Bump-VersionAndTag

# ------------------------------------------------------------------------------
