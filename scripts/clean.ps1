# Variables
$originalPath = Get-Location
$workingPath  = ($PSScriptRoot)

$tgtPath = Join-Path -Path $workingPath -ChildPath '../src'
Get-ChildItem $tgtPath -Directory -Recurse -Include bin,obj | ForEach-Object {
    $tgt = $_.FullName + '\*'
    Remove-Item $tgt -Recurse
}

Set-Location $originalPath