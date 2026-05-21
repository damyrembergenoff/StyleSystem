FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY StyleSystem.Api/ ./StyleSystem.Api/
COPY StyleSystem.Shared/ ./StyleSystem.Shared/
RUN dotnet restore StyleSystem.Api/StyleSystem.Api.csproj
RUN dotnet publish StyleSystem.Api/StyleSystem.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StyleSystem.Api.dll"]
