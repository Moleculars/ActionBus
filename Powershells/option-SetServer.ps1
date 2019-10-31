
param(
    [string]$uri = "http://localhost:5000"
)


    [System.Environment]::SetEnvironmentVariable("option_server_url", $uri)
    Write-Host "option server setted on : " $uri
    Write-Host ""