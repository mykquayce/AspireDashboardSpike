FROM mcr.microsoft.com/dotnet/sdk:8.0 as build-env
WORKDIR /app
COPY . .
RUN dotnet restore ./AspireDashboardSpike.Api/AspireDashboardSpike.Api.csproj --source https://api.nuget.org/v3/index.json
RUN dotnet publish ./AspireDashboardSpike.Api/AspireDashboardSpike.Api.csproj --configuration Release --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 443/tcp
ENV ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "./AspireDashboardSpike.Api.dll"]
