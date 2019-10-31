

$RootUrl = [System.Environment]::GetEnvironmentVariable("option_server_url")
if ($RootUrl -eq $null)
{
    throw New-Object Exception "option server not setted. please concidere use option-server.ps1 -uri 'http://optionserver:80'"
}

$r = [System.Environment]::GetEnvironmentVariable("option_token")
if ($r -eq $null)
{
    throw New-Object Exception "your are not authenticated. please concidere use option-connect.ps1 -login 'yourlogin' -password 'yourpassword'"
}

$url = $RootUrl + "/api/Configuration/Account"

Write-Host ""
Write-Host "calling configuration server : " $url

try
{

    $headers = @{
        'authorization' = $r
    }

    $result = Invoke-RestMethod -Uri $url -Method Get -Headers $headers

    Write-Output $result
    

}
catch [System.Exception]
{
    
    Write-Error $Error[0].Exception.Message

}

