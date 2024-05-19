docker pull mcr.microsoft.com/dotnet/sdk:8.0
docker pull mcr.microsoft.com/dotnet/aspnet:8.0

docker build `
	--file .\AspireDashboardSpike.Api\Dockerfile `
	--tag eassbhhtgu/weatherforecast:latest `
	.

docker push eassbhhtgu/weatherforecast:latest
