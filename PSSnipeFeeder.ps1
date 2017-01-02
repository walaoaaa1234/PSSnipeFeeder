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
        "/" = { return ( $content) }
        }
        while ($listener.IsListening) {
            if ((test-path -Path "$($env:TMP)\poke.json" ) ) {
                $content = get-content -Path "$($env:TMP)\poke.json"
            } else {
                $content=""
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


get-job -Name httpjob -ErrorAction SilentlyContinue | Stop-Job | Out-Null
get-job -Name httpjob -ErrorAction SilentlyContinue | remove-job | Out-Null

StartHttp

$pokemonspool=@()

    
do {
   if ( (get-job -Name httpjob).State -ne "Running" ) {
        Write-Host "Listener doesn't work. Please investigate. Exiting. First kill all powershell.exe processes and try again" 
        write-host "error message from listener: " 
        Receive-job -Name httpjob
        break;
   }
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
        write-host "Added $($clip) to list"
        if ($clip.Substring(0,14) -ieq 'pokesniper2://') {
            $pokename =  $coll[2]
            $pokecoords = $coll[3]
        }
        if ($clip.Substring(0,10) -ieq 'msniper://') {
            $pokename =  $coll[2]
            $pokecoords = $coll[5]
        }
        $tmp = New-Object System.Object
        $tmp | Add-Member -type NoteProperty -name name -Value ($pokename.Trim())
        #$tmp | Add-Member -type NoteProperty -name iv -Value ([int]$([math]::Round($coll[6])))
        $tmp | Add-Member -type NoteProperty -name latitude -Value (($pokecoords -split ",")[0].Trim())
        $tmp | Add-Member -type NoteProperty -name longitude -Value (($pokecoords -split ",")[1].Trim())
        $tmp | Add-Member -type NoteProperty -name expiration -Value ([int64](((Get-Date).AddMinutes(3)-(get-date "1/1/1970")).TotalMilliseconds))
        $pokemonspool+=$tmp
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
   $lastclip = $clip
   start-sleep -Seconds 1
   
} while (1 -eq 1)

