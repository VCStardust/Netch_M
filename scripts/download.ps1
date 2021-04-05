param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]
    $OutputPath = "Netch\bin\x64\Release\win-x64\publish",

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]
    $CachePath = "DataCache"
)

$NetchDataURL = "https://github.com/NetchX/NetchData/archive/refs/heads/master.zip"
$NetchModeURL = "https://github.com/NetchX/NetchMode/archive/refs/heads/master.zip"
$NetchI18NURL = "https://github.com/NetchX/NetchI18N/archive/refs/heads/master.zip"

$last = $(Get-Location)
New-Item -ItemType Directory -Name $OutputPath | Out-Null
Set-Location $OutputPath

Invoke-WebRequest -Uri $NetchDataURL -OutFile data.zip
Invoke-WebRequest -Uri $NetchModeURL -OutFile mode.zip
Invoke-WebRequest -Uri $NetchI18NURL -OutFile i18n.zip

Expand-Archive -Force -Path data.zip -DestinationPath .
Expand-Archive -Force -Path mode.zip -DestinationPath .
Expand-Archive -Force -Path i18n.zip -DestinationPath .

New-Item -ItemType Directory -Name bin  | Out-Null
New-Item -ItemType Directory -Name mode | Out-Null
New-Item -ItemType Directory -Name i18n | Out-Null

Copy-Item -Recurse -Force .\NetchData-master\*             .\bin
Copy-Item -Recurse -Force .\NetchMode-master\mode\*        .\mode
Copy-Item -Recurse -Force .\NetchI18N-master\i18n\*        .\i18n

Remove-Item -Recurse -Force NetchData-master
Remove-Item -Recurse -Force NetchMode-master
Remove-Item -Recurse -Force NetchI18N-master
Remove-Item -Force data.zip
Remove-Item -Force mode.zip
Remove-Item -Force i18n.zip

Set-Location $last

.\scripts\dlcloak.ps1  -OutputPath $OutputPath\bin\
.\scripts\dlxray.ps1   -OutputPath $OutputPath\bin\

Set-Location $OutputPath
Get-Item *
Set-Location $last
exit 0
