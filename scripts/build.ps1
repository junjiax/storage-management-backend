Write-Host "Building solution..."
cd (Split-Path -Parent $MyInvocation.MyCommand.Definition)
# Run dotnet build on the solution root
dotnet build ..\dotnet-backend.sln
