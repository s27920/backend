#!/bin/bash

CODE=$(cat $2)

rm -rf my-rootfs

mkdir -p my-rootfs

sudo cp --reflink=auto rootfs-base.ext4 rootfs.ext4

sudo mount rootfs.ext4 my-rootfs

cd my-rootfs

CURR_DIR=$(pwd)

echo $CURR_DIR

CLASSNAME=$1

cat > "$CURR_DIR/sandbox/run.sh" << EOF
#!/bin/bash

cd /sandbox

javac -cp "gson-2.13.1.jar" "$CLASSNAME.java"
java -cp ".:gson-2.13.1.jar" $CLASSNAME > /dev/ttyS0 2>&1

sync

echo 1 > /proc/sys/kernel/sysrq
echo c > /proc/sysrq-trigger
#poweroff

EOF

echo $CODE > "$CURR_DIR/sandbox/$CLASSNAME.java"

sudo chmod a-w "$CURR_DIR/sandbox/run.sh"
sudo chmod a+x "$CURR_DIR/sandbox/run.sh"

cd ..

sudo umount my-rootfs

echo "built copy image"
