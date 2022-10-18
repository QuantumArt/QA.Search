FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

RUN apt-get install -y \
    && curl -sL https://deb.nodesource.com/setup_12.x | bash \
    && apt-get install -y nodejs

WORKDIR /src
COPY QA.Search.Api/ClientApp/package.json QA.Search.Api/ClientApp/package-lock.json QA.Search.Api/ClientApp/
WORKDIR QA.Search.Api/ClientApp/
RUN npm ci

WORKDIR /src
COPY QA.Search.Api/ClientApp QA.Search.Api/ClientApp/
WORKDIR QA.Search.Api/ClientApp/
RUN npm run build

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
