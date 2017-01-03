$nugeturl = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe'

$nugetlibs =  "$($PSScriptRoot)\..\NugetLibs"
if ((test-path -Path "$($PSScriptRoot)\nuget.exe" ) -eq $false) {
    Invoke-WebRequest -Uri $nugeturl  -OutFile "$($PSScriptRoot)\nuget.exe" -Method Get -TimeoutSec 60
}
#nuget.exe install POGOLib.Official -OutputDirectory NugetLibs
start-process -Wait -FilePath "$($PSScriptRoot)\nuget.exe" -ArgumentList @("install","POGOLib.Official","-Prerelease","-OutputDirectory",$nugetlibs)
start-process -Wait -FilePath "$($PSScriptRoot)\nuget.exe" -ArgumentList @("install","POGOLib.Official.Google","-Prerelease","-OutputDirectory",$nugetlibs)
remove-item -Path "$($nugetlibs)\POGOLib.Official.1.1.0" -Force -Recurse -ErrorAction SilentlyContinue

