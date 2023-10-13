FROM node:16-bullseye as front-builder
WORKDIR /src
COPY QA.Search.Admin/ClientApp/package.json QA.Search.Admin/ClientApp/package-lock.json QA.Search.Admin/ClientApp/
WORKDIR QA.Search.Admin/ClientApp/
RUN npm ci

WORKDIR /src
COPY QA.Search.Admin/ClientApp QA.Search.Admin/ClientApp/
WORKDIR QA.Search.Admin/ClientApp/
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /src
COPY nuget.config ./
COPY QA.Search.Admin/QA.Search.Admin.csproj                                         QA.Search.Admin/
COPY QA.Search.Common/QA.Search.Common.csproj                                       QA.Search.Common/
COPY QA.Search.Data/QA.Search.Data.csproj                                           QA.Search.Data/
COPY QA.Search.Generic.DAL/QA.Search.Generic.DAL.csproj                             QA.Search.Generic.DAL/
COPY QA.Search.Generic.Integration.Core/QA.Search.Generic.Integration.Core.csproj   QA.Search.Generic.Integration.Core/
COPY QA.Search.Generic.Integration.QP/QA.Search.Generic.Integration.QP.csproj	    QA.Search.Generic.Integration.QP/
RUN dotnet restore QA.Search.Admin/QA.Search.Admin.csproj

COPY QA.Search.Admin                      QA.Search.Admin/
COPY QA.Search.Common                     QA.Search.Common/
COPY QA.Search.Data                       QA.Search.Data/
COPY QA.Search.Generic.DAL                QA.Search.Generic.DAL/
COPY QA.Search.Generic.Integration.Core   QA.Search.Generic.Integration.Core/
COPY QA.Search.Generic.Integration.QP	  QA.Search.Generic.Integration.QP/
COPY --from=front-builder /src/QA.Search.Admin/ClientApp/build ./QA.Search.Admin/ClientApp/build/
RUN dotnet publish QA.Search.Admin/QA.Search.Admin.csproj -c Release -o QA.Search.Admin/out

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as base
# Build runtime image
WORKDIR /app
COPY --from=build-env /src/QA.Search.Admin/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "QA.Search.Admin.dll"]
