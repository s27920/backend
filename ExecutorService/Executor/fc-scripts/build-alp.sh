#!/bin/bash

sudo rm -rf /tmp/rootfs-alp
sudo rm -rf alpine-rootfs.ext4

#note that seek 192 didn't work so I feel that 256 may be optimal

dd if=/dev/zero of=alpine-rootfs.ext4 bs=1M count=0 seek=256
mkfs.ext4 alpine-rootfs.ext4

mkdir -p /tmp/rootfs-alp

sudo mount alpine-rootfs.ext4 /tmp/rootfs-alp

cd /tmp/rootfs-alp || exit 1

sudo curl -O https://dl-cdn.alpinelinux.org/alpine/v3.21/releases/x86_64/alpine-minirootfs-3.21.3-x86_64.tar.gz
sudo tar -xpf alpine-minirootfs-3.21.3-x86_64.tar.gz
sudo rm -rf alpine-minirootfs-3.21.3-x86_64.tar.gz

mkdir -p sandbox

sudo curl -o "/tmp/rootfs-alp/sandbox/gson-2.13.1.jar" https://repo1.maven.org/maven2/com/google/code/gson/gson/2.13.1/gson-2.13.1.jar

sudo chmod a-w "/tmp/rootfs-alp/sandbox/gson-2.13.1.jar"
sudo chmod a+r "/tmp/rootfs-alp/sandbox/gson-2.13.1.jar"

sudo cp /etc/resolv.conf /tmp/rootfs-alp/etc/resolv.conf

cat > "/tmp/rootfs-alp/etc/apk/repositories" << EOF
http://dl-cdn.alpinelinux.org/alpine/v3.21/main
http://dl-cdn.alpinelinux.org/alpine/v3.21/community
EOF

sudo mount -t proc proc proc/
sudo mount -t sysfs sys sys/
sudo mount -o bind /dev dev/
sudo mount -o bind /dev/pts dev/pts/

sudo chroot /tmp/rootfs-alp /bin/sh << 'EOF'
apk update
apk add openjdk17-jre-headless coreutils openrc mdevd

echo 'ttyS0 root:root 660' > /etc/mdevd.conf

cat > "/etc/init.d/executor" << 'INNER_EOF'
#!/sbin/openrc-run
description="java executor script"
command="/sandbox/run.sh"
command_background=false
pidfile="/run/executor.pid"
start_stop_daemon_args="--make-pidfile"

depend(){
    need localmount
    need mdevd
}

INNER_EOF

chmod +x /etc/init.d/executor
rc-update add executor default
EOF

echo "" >  /tmp/rootfs-alp/etc/resolv.conf

cd ~ || exit 1

sudo umount /tmp/rootfs-alp/dev/pts
sudo umount /tmp/rootfs-alp/dev
sudo umount /tmp/rootfs-alp/proc
sudo umount /tmp/rootfs-alp/sys
sudo umount /tmp/rootfs-alp