# ------------------------------------------------------------------------------
# Script parameters

param(
    [Parameter(Mandatory)]
    [string]$filter
)
# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Get all versions
$request = 'http://baget.gbworks.lan/v3/registration/cubes.core/index.json'
$versions = Invoke-WebRequest $request | 
    ConvertFrom-Json | 
    Select -expand items |
    Select -expand items |
    Select -expand catalogEntry |
    Select version
# ------------------------------------------------------------------------------


# ------------------------------------------------------------------------------
# Loop through versions found
foreach ($v in $versions)
{
    if ($v.version.StartsWith($filter)) {
        $command = "& dotnet nuget delete Cubes.Core {0} --api-key GbWorks@ApiKey! --source http://baget.gbworks.lan --non-interactive" -f $v.version
        Invoke-Expression $command 
    }
}
# ------------------------------------------------------------------------------
