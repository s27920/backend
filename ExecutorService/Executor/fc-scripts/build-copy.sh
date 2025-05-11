#!/bin/bash

CLASSNAME="$1"
CODE="$2"
EXEC_ID="$3"
SIGNING_KEY="$4"

ROOTFS_DIR="/tmp/$EXEC_ID-rootfs"
ROOTFS="/tmp/$EXEC_ID-rootfs.ext4"

mkdir -p "$ROOTFS_DIR"

cp --reflink=auto "/app/fc-scripts/rootfs-base.ext4" "$ROOTFS"

mount "$ROOTFS" "$ROOTFS_DIR"

cat > "$ROOTFS_DIR/sandbox/run.sh" << EOF
#!/bin/sh
cd /sandbox

javac -cp "gson-2.13.1.jar" "$CLASSNAME.java"
java -cp ".:gson-2.13.1.jar" $CLASSNAME > /dev/ttyS0 2>&1

sync

echo "ctr-${SIGNING_KEY}-pof" > /dev/ttyS0 2>&1

EOF

echo "$CODE" | base64 -d > "$ROOTFS_DIR/sandbox/$CLASSNAME.java"

chmod a-w "$ROOTFS_DIR/sandbox/run.sh"
chmod a+x "$ROOTFS_DIR/sandbox/run.sh"
umount "$ROOTFS_DIR"

echo "built copy"
