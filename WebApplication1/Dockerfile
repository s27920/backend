FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApplication1.csproj", "WebApplication1/"]
RUN dotnet restore "WebApplication1/WebApplication1.csproj"
COPY . WebApplication1/
WORKDIR "/src/WebApplication1"
RUN dotnet build "WebApplication1.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WebApplication1.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM docker:dind
USER root

RUN apk update && apk add --no-cache \
    icu-libs \
    krb5-libs \
    libgcc \
    libintl \
    libssl3 \
    libstdc++ \
    zlib \
    bash \
    wget \
    docker-cli \
    procps \
    curl

RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh -O dotnet-install.sh && \
    chmod +x ./dotnet-install.sh && \
    ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet && \
    ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet && \
    rm dotnet-install.sh
    
WORKDIR /app
COPY --from=publish /app/publish .
COPY entrypoint.sh .
COPY ./Modules/Executor/client-src ./client-src
COPY ./Modules/Executor/exec-scripts ./exec-scripts
COPY ./Modules/Executor/executor-images ./executor-images
COPY ./Modules/Executor/scripts ./scripts

EXPOSE 80

ENTRYPOINT ["/app/entrypoint.sh"]