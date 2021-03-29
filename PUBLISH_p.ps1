Write-Host 'Building'

dotnet publish `
	-c "Release" `
	-r "win-x64" `
	-p:Platform="x64" `
	-p:PublishSingleFile=true `
	-p:SelfContained=true `
	-p:PublishTrimmed=true `
	-p:PublishReadyToRun=true `
	-p:IncludeAllContentInSingleFile=true `
	-o Netch\bin\Publish\ `
	Netch\Netch.csproj

if ($LASTEXITCODE) { exit $LASTEXITCODE } 

Write-Host 'Build done'