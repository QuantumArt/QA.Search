FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /src
COPY nuget.config ./
COPY QA.Search.Api/QA.Search.Api.csproj                 QA.Search.Api/
COPY QA.Search.Common/QA.Search.Common.csproj           QA.Search.Common/
COPY QA.Search.Data/QA.Search.Data.csproj               QA.Search.Data/
RUN dotnet restore QA.Search.Api/QA.Search.Api.csproj

COPY QA.Search.Api/              QA.Search.Api/
COPY QA.Search.Common/           QA.Search.Common/
COPY QA.Search.Data/             QA.Search.Data/
RUN dotnet publish QA.Search.Api/QA.Search.Api.csproj -c Release -o QA.Search.Api/out 

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app
COPY --from=build-env /src/QA.Search.Api/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "QA.Search.Api.dll"]
