FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /src
COPY nuget.config ./
COPY QA.Search.Common/QA.Search.Common.csproj															QA.Search.Common/
COPY QA.Search.Data/QA.Search.Data.csproj																QA.Search.Data/
COPY QA.Search.Generic.DAL/QA.Search.Generic.DAL.csproj													QA.Search.Generic.DAL/
COPY QA.Search.Generic.Integration.API/QA.Search.Generic.Integration.API.csproj							QA.Search.Generic.Integration.API/
COPY QA.Search.Generic.Integration.Core/QA.Search.Generic.Integration.Core.csproj						QA.Search.Generic.Integration.Core/
COPY QA.Search.Generic.Integration.QP/QA.Search.Generic.Integration.QP.csproj							QA.Search.Generic.Integration.QP/
COPY QA.Search.Generic.Integration.QP.Permissions/QA.Search.Generic.Integration.QP.Permissions.csproj   QA.Search.Generic.Integration.QP.Permissions/
RUN dotnet restore QA.Search.Generic.Integration.API/QA.Search.Generic.Integration.API.csproj

COPY QA.Search.Common								QA.Search.Common/
COPY QA.Search.Data									QA.Search.Data/
COPY QA.Search.Generic.DAL							QA.Search.Generic.DAL/
COPY QA.Search.Generic.Integration.API				QA.Search.Generic.Integration.API/
COPY QA.Search.Generic.Integration.Core				QA.Search.Generic.Integration.Core/
COPY QA.Search.Generic.Integration.QP				QA.Search.Generic.Integration.QP/
COPY QA.Search.Generic.Integration.QP.Permissions   QA.Search.Generic.Integration.QP.Permissions/

RUN dotnet publish QA.Search.Generic.Integration.API/QA.Search.Generic.Integration.API.csproj -c Release -o QA.Search.Integration.API/out 

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
COPY --from=build-env /src/QA.Search.Integration.API/out .
ENTRYPOINT ["dotnet", "QA.Search.Integration.API.dll"]
