Get-ChildItem ..\src -Directory -Recurse -Include bin,obj | ForEach-Object {
    $tgt = $_.FullName + '\*'
    Remove-Item $tgt -Recurse
}