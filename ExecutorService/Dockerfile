FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 1337

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Executor/Executor.csproj", "Executor/"]
RUN dotnet restore "Executor/Executor.csproj"
COPY . .
WORKDIR "/src/Executor"
RUN dotnet build "Executor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Executor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN sudo apt-get update \
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
    libvirt-clients &&\
    wget https://github.com/firecracker-microvm/firecracker/releases/download/v1.2.0/firecracker-v1.2.0-x86_64.tgz &&\
    tar -xvf firecracker-v1.2.0-x86_64.tgz &&\
    sudo mv firecracker-v1.2.0-x86_64 /usr/local/bin/firecracker &&\
    wget https://s3.amazonaws.com/spec.ccfc.min/img/quickstart_guide/x86_64/kernels/vmlinux.bin -O ~/firecracker/vmlinux.bin && \
    curl -L https://github.com/firecracker-microvm/firecracker/releases/download/${latest}/firecracker-$(basename $(curl -fsSLI -o /dev/null -w  %{url_effective} ${release_url}/latest))-$(uname -m).tgz | tar -xz

ENTRYPOINT ["dotnet", "Executor.dll"]
