#!/bin/bash

rm -rf my-rootfs-base
rm -rf rootfs-base.ext4

dd if=/dev/zero of=rootfs-base.ext4 bs=1M count=1024
mkfs.ext4 rootfs-base.ext4

mkdir -p my-rootfs-base

sudo mount rootfs-base.ext4 my-rootfs-base

sudo debootstrap --variant=minbase jammy my-rootfs-base

sudo mount -t proc proc my-rootfs-base/proc
sudo mount -t sysfs sys my-rootfs-base/sys
sudo mount -o bind /dev my-rootfs-base/dev
sudo mount -o bind /dev/pts my-rootfs-base/dev/pts

cd my-rootfs-base

CURR_DIR=$(pwd)

cat /etc/resolv.conf > "$CURR_DIR/etc/resolv.conf"

cat > "$CURR_DIR/etc/apt/sources.list" << EOF
deb http://archive.ubuntu.com/ubuntu jammy main universe
deb http://archive.ubuntu.com/ubuntu jammy-updates main universe
deb http://security.ubuntu.com/ubuntu jammy-security main universe
EOF

sudo mkdir -p sandbox

sudo curl -o "$CURR_DIR/sandbox/gson-2.13.1.jar" https://repo1.maven.org/maven2/com/google/code/gson/gson/2.13.1/gson-2.13.1.jar

sudo chmod a-w "$CURR_DIR/sandbox/gson-2.13.1.jar"
sudo chmod a+r "$CURR_DIR/sandbox/gson-2.13.1.jar"

sudo chroot "$CURR_DIR" /bin/bash <<EOF

apt-get update
apt install -y --no-install-recommends \
  systemd systemd-sysv \
  bash \
  openjdk-17-jdk-headless \
  ca-certificates \
  coreutils

cat > /etc/systemd/system/executor.service << 'INNER_EOF'
[Unit]
Description=java executor service
Before=shutdown.target
After=local-fs.target
DefaultDependencies=no

[Service]
Type=simple
ExecStart=/sandbox/run.sh
TimeoutSec=90
StandardOutput=journal+console

[Install]
WantedBy=multi-user.target

INNER_EOF

apt clean
rm -rf /var/lib/apt/lists/*
echo "" > "$CURR_DIR/etc/apt/sources.list"

systemctl enable executor.service
systemctl mask getty.target systemd-logind.service
EOF

cd ..

sudo umount my-rootfs-base/dev/pts
sudo umount my-rootfs-base/proc
sudo umount my-rootfs-base/sys
sudo umount my-rootfs-base/dev
sudo umount my-rootfs-base
