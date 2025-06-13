Write-Host "Installing COMmander service..."

New-Item -Path "C:\Program Files\COMmander" -ItemType Directory
Copy-Item -Path ".\COMmanderService\*" -Destination "C:\Program Files\COMmander\"
Write-Host "Moved service files!"

New-Service -Name "COMmander" -BinaryPathName "`"C:\Program Files\COMmander\COMmanderService.exe`"" -DisplayName "COMmander" -StartupType Automatic -Credential "LocalSystem" -DependsOn "RpcSs"
Write-Host "Registered service!"

Start-Service COMmander
Sleep 2
Restart-Service COMmander -Force

Write-Host "`nDone!"