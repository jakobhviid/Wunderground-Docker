# ---- dotnet build stage ----
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build

ARG BUILDCONFIG=RELEASE
ARG VERSION=1.0.0

WORKDIR /build/

COPY ./Server/Server.csproj ./Server.csproj
RUN dotnet restore ./Server.csproj

COPY ./Server ./

RUN dotnet build -c ${BUILDCONFIG} -o out && dotnet publish ./Server.csproj -c ${BUILDCONFIG} -o out /p:Version=${VERSION}

# ---- final stage ----

FROM omvk97/dotnet-kerberos-auth

LABEL Maintainer="Oliver Marco van Komen"

RUN apt-get update && apt-get install -y sqlite3

RUN mkdir /database/

ENV DOTNET_PROGRAM_HOME=/opt/WundergroundServer

COPY --from=build /build/out ${DOTNET_PROGRAM_HOME}

# Copy necessary scripts + configuration
COPY scripts /tmp/
RUN chmod +x /tmp/*.sh && \
    mv /tmp/* /usr/bin && \
    rm -rf /tmp/*

CMD [ "docker-entrypoint.sh" ]