# Script to start Cubes specifying root folder. Useful to keep binaries folder clear
# from other Cubes folders.
#
# We can create a link to the most recent cubes binaries folder using the following
#     New-Item -ItemType SymbolicLink -Path Current -Target .\LatestCubesBuildFolder\

# Assuming bootstrap script is on Cubes root folder
$workingPath  = ($PSScriptRoot)
$env:CUBES_ROOTFOLDER="$workingPath"
& "$workingPath\Versions\Current\Cubes.Host.exe"

