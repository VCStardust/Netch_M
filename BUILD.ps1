.\download.ps1 'Netch\bin\x64\Release\win-x64\publish'

Write-Host "Building $Configuration to $OutputPath"

dotnet publish `
	-c "Release" `
	-r "win-x64" `
	-p:Platform="x64" `
	-p:PublishSingleFile=true `
	-p:SelfContained=false `
	-p:PublishTrimmed=false `
	-p:PublishReadyToRun=true `
	Netch\Netch.csproj

if ($lastExitCode) { exit $lastExitCode } 
exit 0