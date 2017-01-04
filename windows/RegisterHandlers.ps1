
$configfile="$($PSScriptRoot)\..\feeder\config.json"
$wrapperDir= (get-item -Path "$($PSScriptRoot)\..\wrapper").FullName
$config = Get-Content -Path $configfile | Out-String | ConvertFrom-Json
$url = "http://$(($config.webserveraddress)):$(($config.webserverport))"

$regvalue = """C:\Program Files\dotnet\dotnet.exe"" run -p ""$($wrapperDir)"" $($url)/addpokemon/%1"

#msniper
new-item -Path HKCU:\SOFTWARE\Classes\msniper\shell\open\command -Value $regvalue  -Force

#pokesniper2
new-item -Path HKCU:\SOFTWARE\Classes\pokesniper2\shell\open\command -Value $regvalue -Force