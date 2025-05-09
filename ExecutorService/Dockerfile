FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 1337

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./ExecutorService.csproj", "ExecutorService/"]
RUN dotnet restore "ExecutorService/ExecutorService.csproj"
COPY . "ExecutorService/"
WORKDIR "/src/ExecutorService"
RUN dotnet build "ExecutorService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ExecutorService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    wget \
    unzip \
    git \
    gcc \
    make \
    build-essential \
    linux-headers-$(uname -r) \
    qemu-utils \
    qemu-kvm \
    libvirt-daemon-system \
    libvirt-clients && \
    FC_VERSION="v1.2.0" && \
    curl -LO "https://github.com/firecracker-microvm/firecracker/releases/download/${FC_VERSION}/firecracker-${FC_VERSION}-$(uname -m).tgz" && \
    tar -xzf "firecracker-${FC_VERSION}-$(uname -m).tgz" && \
    mv "firecracker-${FC_VERSION}-$(uname -m)/firecracker" /usr/local/bin/ && \
    mv "firecracker-${FC_VERSION}-$(uname -m)/jailer" /usr/local/bin/ && \
    apt-get clean && \
    rm -rf "firecracker-${FC_VERSION}-$(uname -m)" \
    rm -rf /var/lib/apt/lists/*

USER $APP_UID
ENTRYPOINT ["dotnet", "ExecutorService.dll"]
    
ENTRYPOINT ["dotnet", "ExecutorService.dll"]