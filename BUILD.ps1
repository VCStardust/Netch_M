Write-Host 'Building'

dotnet build -p:Configuration="Release" `
	-p:SolutionDir="$pwd\" `
	-restore `
    -p:Platform="x64" `
	Netch\Netch.csproj

if ($LASTEXITCODE) { exit $LASTEXITCODE } 

Write-Host 'Build done'
