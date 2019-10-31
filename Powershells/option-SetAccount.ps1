
param(
    [string]$accountName = "account1"
)

$RootUrl = [System.Environment]::GetEnvironmentVariable("option_server_url")
if ($RootUrl -eq $null)
{
    throw New-Object Exception "option server not setted. please concidere use option-server.ps1 -uri 'http://optionserver:80'"
}

$url = $RootUrl + "/api/Configuration/Account/Add/${accountName}"

Write-Host ""
Write-Host "calling configuration server : " $url

try
{

    $headers = @{ 'authorization' = $r }

    $result = Invoke-RestMethod -Uri $url -Method Post -Headers $headers

    Write-Host "  successfull created account " $accountName

    Write-Host "  current working account switched on " $accountName

    [System.Environment]::SetEnvironmentVariable("option_token", $result)
    [System.Environment]::SetEnvironmentVariable("option_current_working_account", $accountName)

}
catch [System.Exception]
{
    
    Write-Error $Error[0].Exception.Message

}

