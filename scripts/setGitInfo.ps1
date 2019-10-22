$Hash    = (git rev-parse HEAD).Substring(0, 10)
$Branch  = (git rev-parse --abbrev-ref HEAD)
$GitInfo = "Branch`:$Branch, #$Hash"

Write-Output "Setting Git info: $GitInfo"

$SourceFile = '..\src\Cubes.Core\Environment\CubesEnvironmentInfo.cs'
$Pattern    = '^(?<wb>\s+)GitInfo(?<wa>\s+)=(?<gi>.+);'

$text = (Get-Content $SourceFile) -replace $Pattern,"`${wb}GitInfo`${wa}=`"Branch`:$Branch, #Hash`:$Hash`";"
Set-Content -Path $SourceFile $Text
