docker pull eassbhhtgu/weatherforecast:latest
docker pull mcr.microsoft.com/dotnet/nightly/aspire-dashboard:latest

docker stack deploy --compose-file .\docker-compose.yml aspire-spike
