
param(
    [Parameter(Mandatory=$true)][string]$login,
    [Parameter(Mandatory=$true)][string]$password,
    [Parameter(Mandatory=$false)][string]$account
)

$RootUrl = [System.Environment]::GetEnvironmentVariable("option_server_url")
if ($RootUrl -eq $null)
{
    throw New-Object Exception "option server not setted. please concidere use option-server.ps1 -uri 'http://optionserver:80'"
}

$url = $RootUrl + "/api/Token/User/connect"
$msg = @{Login = $login; Password = $password }

Write-Host ""

Write-Host "calling configuration server : " $url
try
{

    $result = Invoke-RestMethod -Uri $url -Method Post -ContentType "application/json;charset=UTF-8"  -Body (ConvertTo-Json $msg) 

    Write-Host "  successfull authentication "

    [System.Environment]::SetEnvironmentVariable("option_token", $result)

    if (-Not($account -eq $null))
    {

        [System.Environment]::SetEnvironmentVariable("option_current_working_account", $account)
    
    
    }

}
catch [System.Exception]
{
    
    Write-Error $Error[0].Exception.Message

}

