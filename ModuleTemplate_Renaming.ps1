$base_dir = Read-Host -Prompt 'Root Directory'
$search = Read-Host -Prompt 'Replace'
$replace_with = Read-Host -Prompt 'With'

Get-ChildItem $base_dir -Recurse -Include "*.*" |
    ForEach-Object { (Get-Content $_.FullName) | 
    ForEach-Object {$_ -replace $search, $replace_with} | 
    Set-Content $_.FullName }

Get-ChildItem $base_dir -Recurse |
    Rename-Item -NewName {$_.name -replace $search, $replace_with}
