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
    debootstrap \
    curl \
    wget \
    unzip \
    git \
    gcc \
    make \
    build-essential \
    linux-headers-amd64 \
    qemu-utils \
    qemu-kvm \
    libvirt-daemon-system \
    libvirt-clients && \
    wget https://github.com/firecracker-microvm/firecracker/releases/download/v1.2.0/firecracker-v1.2.0-x86_64.tgz && \
    tar -xvf firecracker-v1.2.0-x86_64.tgz && \
    mv release-v1.2.0-x86_64/firecracker-v1.2.0-x86_64 /usr/local/bin/ && \
    apt-get clean && \
    rm -rf firecracker-v1.2.0-x86_64.tgz && \
    rm -rf /var/lib/apt/lists/* && \
    rm -rf release-v1.2.0-x86_64 && \
    echo '/usr/local/bin/firecracker-v1.2.0-x86_64' >> ~/.bashrc

COPY Executor/fc-scripts fc-scripts
COPY Executor/client-src client-src

USER $APP_UID

ENTRYPOINT ["dotnet", "ExecutorService.dll"]