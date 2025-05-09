#!/bin/bash

CLASSNAME=$1
CODE=$(cat "$2")
EXEC_ID=$3

ROOTFS_DIR="/tmp/$EXEC_ID-rootfs"
ROOTFS="/tmp/$EXEC_ID-rootfs.ext4"

mkdir -p "$ROOTFS_DIR"

sudo cp --reflink=auto "/fc-scripts/rootfs-base.ext4" "$ROOTFS"

sudo mount "$ROOTFS" "$ROOTFS_DIR"

cat > "$ROOTFS_DIR/sandbox/run.sh" << EOF
#!/bin/bash

cd /sandbox

javac -cp "gson-2.13.1.jar" "$CLASSNAME.java"
java -cp ".:gson-2.13.1.jar" $CLASSNAME > /dev/ttyS0 2>&1

sync

echo 1 > /proc/sys/kernel/sysrq
echo c > /proc/sysrq-trigger

EOF

echo "$CODE" > "$ROOTFS_DIR/sandbox/$CLASSNAME.java"

sudo chmod a-w "$ROOTFS_DIR/sandbox/run.sh"
sudo chmod a+x "$ROOTFS_DIR/sandbox/run.sh"
sudo umount "$ROOTFS_DIR"