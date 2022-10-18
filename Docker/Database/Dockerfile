FROM mcr.microsoft.com/mssql/server:2017-latest

ENV ACCEPT_EULA Y
ENV SA_PASSWORD StrongPass1234

WORKDIR /src

COPY . /src

RUN chmod +x /src/create-db.sh

CMD /bin/bash ./entrypoint.sh