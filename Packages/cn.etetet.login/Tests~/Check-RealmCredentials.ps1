$ErrorActionPreference = 'Stop'
$project = Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent
Push-Location $project
try {
    'Test --Name=Login.*' | dotnet ./Bin/ET.App.dll --SceneName=Test
    if ($LASTEXITCODE -ne 0) { throw 'Login tests failed.' }
}
finally {
    Pop-Location
}
