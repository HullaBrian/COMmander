Stop-Service COMmander -erroraction 'silentlycontinue'
Write-Host "Stopped COMmander service"

Remove-Item "C:\Program Files\COMmander" -Force -Recurse -erroraction 'silentlycontinue'
Write-Host "Removed service files"

C:\Windows\System32\sc.exe delete "COMmander"
# Remove-Service -Name "COMmander"
Write-Host "Removed service"