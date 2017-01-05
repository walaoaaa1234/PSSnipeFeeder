
$configfile="$($PSScriptRoot)\..\feeder\config.json"
$wrapperDir= (get-item -Path "$($PSScriptRoot)\..\wrapper").FullName
$config = Get-Content -Path $configfile | Out-String | ConvertFrom-Json
$url = "http://$(($config.webserveraddress)):$(($config.webserverport))"

$regvalue = """C:\Program Files\dotnet\dotnet.exe"" run -p ""$($wrapperDir)"" $($url)/addpokemon/%1"

#msniper
new-item -Path HKCU:\SOFTWARE\Classes\msniper -Force
New-ItemProperty -Path HKCU:\SOFTWARE\Classes\msniper -Name "URL Protocol" -PropertyType String -Value "" -Force
New-ItemProperty -Path HKCU:\SOFTWARE\Classes\msniper -Name "(Default)" -PropertyType String -Value "URL:msniper" -Force
new-item -Path HKCU:\SOFTWARE\Classes\msniper\shell\open\command -Value $regvalue  -Force

#pokesniper2
new-item -Path HKCU:\SOFTWARE\Classes\pokesniper2 -Force
New-ItemProperty -Path HKCU:\SOFTWARE\Classes\pokesniper2 -Name "URL Protocol" -PropertyType String -Value "" -Force
New-ItemProperty -Path HKCU:\SOFTWARE\Classes\pokesniper2 -Name "(Default)" -PropertyType String -Value "URL:pokesniper2" -Force
new-item -Path HKCU:\SOFTWARE\Classes\pokesniper2\shell\open\command -Value $regvalue -Force