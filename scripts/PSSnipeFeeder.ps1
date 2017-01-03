$botAuthconfig = "E:\Poke\PokemonGo-Bot\configs\2_auth.json"

$global:seconds = 15
function StartHttp {
  Start-Job -name httpjob -ScriptBlock {
    $httpport = 5000
    $url = "http://127.0.0.1:$($httpport)/"
    $listener = New-Object System.Net.HttpListener
    $listener.Prefixes.Add($url)
    #$listener.Prefixes.Add("http://+:5555/")
    try {
        $listener.Start()
        write-host "Listening at $($url)"
        $routes = @{
        "/" = { return ( $webcontent) }
        }
        while ($listener.IsListening) {
            if ((test-path -Path "$($env:TMP)\poke.json" ) ) {
                $webcontent = get-content -Path "$($env:TMP)\poke.json"
            } else {
                $webcontent=""
            }
            $context = $listener.GetContext()
            $requestUrl = $context.Request.Url
            $response = $context.Response
            $localPath = $requestUrl.LocalPath
            $route = $routes.Get_Item($requestUrl.LocalPath)

            if ($route -eq $null)
            {
                $response.StatusCode = 404
            }
            else
            {
                $content = & $route
                $buffer = [System.Text.Encoding]::UTF8.GetBytes($content)
                $response.ContentLength64 = $buffer.Length
                $response.OutputStream.Write($buffer, 0, $buffer.Length)
            }
            $response.Close()
            $responseStatus = $response.StatusCode
        }
    } catch {
        Write-Host "Error :  $($_.Exception.Message)"
    }
  }
}
function GetDllPathByName ($dllname) {
    $result = ($libs | Where-Object {$_.FullName -ilike "*\netstandard1.0\$($dllname)"}).FullName
    if ($result -eq $null) {
        $result = ($libs | Where-Object {$_.FullName -ilike "*\net45\$($dllname)"}).FullName
    }
    if ($result -eq $null) {
        $result = ($libs | Where-Object {$_.FullName -ilike "*\net40\$($dllname)"}).FullName
    }

    if ($result -eq $null) {
        Write-Error "Cannot find a file for $($dllname).  Exiting"
        exit
    }
    return $result
}
function GetSession  ($cacheallowed=$false) {
    $cachedir= $env:TEMP
    $cachefileName = "$($cacheDir)-$($loginProvider.UserId)-$($loginProvider.ProviderId).json"
    #write-host $cachefileName
    if ($cacheallowed) {
        if ((test-path -Path $cacheDir) -eq $false) {
            New-Item -ItemType Directory -Path $cachedir -Force
        }
        if (Test-path -Path  $cachefileName ) {
            $accessToken = (get-content -Path $cachefileName | Out-String | ConvertFrom-Json).AccessToken
        }
        if ($accessToken.IsExpired -eq $false) {
            return [POGOLib.Official.Net.Authentication.Login]::GetSession($loginProvider, $accessToken, $initLat, $initLong);
        }
    }
    $token = $loginprovider.GetAccessToken()
    do {}
    while ($token.Status -ne "RanToCompletion")
    $token = $token.Result
    
    #$coordinate = new-object GeoCoordinatePortable.GeoCoordinate ($initLat, $initLong) 

    $sessionl = [POGOLib.Official.Net.Authentication.Login]::GetSession($loginProvider, $initLat, $initLong )
    do {}
    while ($sessionl.Status -ne "RanToCompletion")
    $sessionl = $sessionl.Result

    if ($cacheallowed) {
        $sessionl.accesstoken | ConvertTo-Json | Out-File -FilePath  $cachefileName -Force
    }
    return $sessionl
}
function ProcessCells ($cells_local) {
    #$cells_local | ogv
    #write-host "parsing cells $($cells_local.count)"
    $aa = @()
    foreach ($cell in $cells_local)  {
    if ($cell.catchablepokemons -ne $null) {
        foreach ($row in $cell.catchablepokemons) {
            $aa+=$row
            $global:session.Map.pokecoll+=$row
        }
    }
}
    $aa
    if ($aa.count -gt0 ) {
    }

}
function CheckPokemon ($pokeobj) {
    if ($global:autoconfig.auth_service -ieq "ptc") {
        [POGOLib.Official.LoginProviders.ILoginProvider]$loginprovider = New-Object POGOLib.Official.LoginProviders.PtcLoginProvider($global:autoconfig.username,$global:autoconfig.password)
    } elseif ($global:autoconfig.auth_service -ieq "google") {
        [POGOLib.Official.LoginProviders.ILoginProvider]$loginprovider = New-Object POGOLib.Official.LoginProviders.GoogleLoginProvider($global:autoconfig.username,$global:autoconfig.password)
    } else {
        Write-Error "Bad or missing logging provider. Exiting"
        exit
    }
    $initLat = $pokeobj.latitude
    $initLong= $pokeobj.longitude 
    #$initLat =  ($autoconfig.location -split ",")[0]
    #$initLong = ($autoconfig.location -split ",")[1]
    $global:session = $null
    $global:session = GetSession ($false)
    Register-ObjectEvent -EventName Update -InputObject $global:session.Map -SourceIdentifier MapUpdate -Action {
            #write-host "new cells arrived"
            ProcessCells $global:session.map.cells
        }
    $await = $global:session.StartupAsync()
    do {}
    while ($await.Status -ne "RanToCompletion")
    $nearbycoll = @()
    $global:session.Map | Add-Member -MemberType NoteProperty -Name  "pokecoll" -Value $nearbycoll
    $cells = $global:session.Map.cells
    #$nearby = $false
    #write-host "initial parse"
    ProcessCells $cells

    Start-sleep -Seconds $global:seconds
    $obj = $global:session.Map.pokecoll | Where-Object {$_.PokemonId -ieq $pokeobj.Name}
    if ($obj -ne $null) {
        $pokeobj | Add-Member -type NoteProperty -name encounter -Value ($obj.EncounterId)
        $pokeobj | Add-Member -type NoteProperty -name spawnpoint -Value ($obj.SpawnPointId)
        $pokeobj
    }
    Unregister-Event -SourceIdentifier MapUpdate -ErrorAction SilentlyContinue
    $global:session.Shutdown()
    $global:session = $null
    return $pokeobj
}

get-job -Name httpjob -ErrorAction SilentlyContinue | Stop-Job | Out-Null
get-job -Name httpjob -ErrorAction SilentlyContinue | remove-job | Out-Null

#StartHttp

$nugetlibs =   (get-item -Path "$($PSScriptRoot)\..\NugetLibs").FullName
if ($libs -eq $null) {
    $libs = Get-ChildItem -Recurse -Path $nugetlibs -Filter *.dll -ErrorAction SilentlyContinue
}
add-type -Path  (GetDllPathByName "Google.Protobuf.dll") 
add-type -Path  (GetDllPathByName "S2Geometry.dll") 
add-type -path  (GetDllPathByName "Newtonsoft.Json.dll")
add-type -path  (GetDllPathByName "GeoCoordinate.NetStandard1.dll")
add-type -path  (GetDllPathByName "POGOProtos.NetStandard1.dll")
add-type -path  (GetDllPathByName "POGOLib.Official.dll")
add-type -path  (GetDllPathByName "POGOLib.Official.Google.dll")


$global:autoconfig = Get-Content -Path $botAuthconfig  | Out-String | ConvertFrom-Json

[POGOLib.Official.Configuration]::Hasher = New-Object POGOLib.Official.Util.Hash.PokeHashHasher($autoconfig.hashkey)


$pokemonspool=@()
    





    Unregister-Event -SourceIdentifier MapUpdate -ErrorAction SilentlyContinue
    $global:session = $null
do {
    
   #if ( (get-job -Name httpjob).State -ne "Running" ) {
   #     Write-Host "Listener doesn't work. Please investigate. Exiting. First kill all powershell.exe processes and try again" 
   #     write-host "error message from listener: " 
   #     Receive-job -Name httpjob
   #     break;
   #}
   Receive-Job -Name httpjob -ErrorAction SilentlyContinue | Out-Null
   $data = Get-Clipboard -Format Text -TextFormatType Text -ErrorAction SilentlyContinue
   if ( $data -ne ""  )    
{    
    $clip = ($data -split "\r\n")[0]
    $clip = $clip -ireplace "\r\n",""
    $clip = $clip.trim()
}
   #$clip =  Get-Clipboard 
   if(  ($clip -ne $lastclip) -and ($clip.Length -ge 10) ) {
   if (($clip.Substring(0,10) -ieq 'msniper://') -or ($clip.Substring(0,10) -ieq 'pokesniper') ) {
        $coll = $clip -isplit "\/"
        write-host "Checking : $($clip) ...."
        if ($clip.Substring(0,14) -ieq 'pokesniper2://') {
            $pokename =  $coll[2]
            $pokecoords = $coll[3]
        }
        if ($clip.Substring(0,10) -ieq 'msniper://') {
            $pokename =  $coll[2]
            $pokecoords = $coll[5]
        }
        if ( ($pokemonspool | Where-Object { ( ($_.name -eq "$pokename.Trim()") -and ($_.latitude -eq (($pokecoords -split ",")[0].Trim()) ) -and ($_.longitude -eq (($pokecoords -split ",")[1].Trim()))) } ) -eq $null ) {
            $tmp = New-Object System.Object
            $tmp | Add-Member -type NoteProperty -name name -Value ($pokename.Trim())
            #$tmp | Add-Member -type NoteProperty -name iv -Value ([int]$([math]::Round($coll[6])))
            $tmp | Add-Member -type NoteProperty -name latitude -Value (($pokecoords -split ",")[0].Trim())
            $tmp | Add-Member -type NoteProperty -name longitude -Value (($pokecoords -split ",")[1].Trim())
            $tmp | Add-Member -type NoteProperty -name expiration -Value ([int64](((Get-Date).AddMinutes(3)-(get-date "1/1/1970")).TotalMilliseconds))
            $tmp =  CheckPokemon $tmp
            if ($tmp.spawnpoint -ne $null) {
                    $pokemonspool+=$tmp
                    write-host "Added $($clip) to list"
            } else {
                    write-host "Rejected $($clip) - unable to verify presense"
            }
            $newpokemonspool=@()
            foreach ($record in  $pokemonspool) {
                if ($record.expiration -gt (([int64](((Get-Date)-(get-date "1/1/1970")).TotalMilliseconds))) ) {
                    $newpokemonspool+=$record
                }
            }
            $pokemonspool = $newpokemonspool
            $content = $pokemonspool | ConvertTo-Json -Depth 3  
            write-host ""
            write-host "Pokemons serving this moment: "
            Write-Host $content
            write-host ""
            $content |Out-File -FilePath "$($env:TMP)\poke.json" 
        }
    }
   }
   $lastclip = $clip
   start-sleep -Seconds 1
   
} while (1 -eq 1)

