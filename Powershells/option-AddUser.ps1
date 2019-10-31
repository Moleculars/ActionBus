
param(
    [Parameter(Mandatory=$true)][string]$login,
    [Parameter(Mandatory=$true)][string]$password,
    [Parameter(Mandatory=$true)][string]$Pseudo,
    [Parameter(Mandatory=$true)][string]$Mail
)


$RootUrl = [System.Environment]::GetEnvironmentVariable("option_server_url")
if ($RootUrl -eq $null)
{
    throw New-Object Exception "option server not setted. please concidere use option-server.ps1 -uri 'http://optionserver:80'"
}



$url = $RootUrl + "/api/Token/User/add"
$msg = @{login = $login; Password = $password; Pseudo = $Pseudo ; Mail = $Mail}

Write-Host ""
Write-Host "calling configuration server : " $url

$result = Invoke-RestMethod -Uri $url -Method Post -ContentType "application/json;charset=UTF-8"  -Body (ConvertTo-Json $msg) 

if ($result.Valid)
{
    Write-Host "  Id : " $result.Result.id
    Write-Host "  Name : " $result.Result.name
    Write-Host "  Pseudo : " $result.Result.pseudo
    Write-Host "  Mail : " $result.Result.mail
    Write-Host ""
}
else
{
    Write-Error $result.Result
}

