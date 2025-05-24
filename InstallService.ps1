Write-Host "Installing COMmander service..."

New-Item -Path "C:\Program Files\COMmander" -ItemType Directory
Move-Item -Path ".\COMmanderService\*" -Destination "C:\Program Files\COMmander\"
Write-Host "Moved service files!"

New-Service -Name "COMmander" -BinaryPathName "`"C:\Program Files\COMmander\COMmanderService.exe`"" -DisplayName "COMmander" -StartupType Automatic -Credential "LocalSystem" -DependsOn "RpcSs"
Write-Host "Registered service!"

Write-Host "`nDone!"