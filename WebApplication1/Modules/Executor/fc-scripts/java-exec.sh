#!/bin/bash

EXEC_ID=$(uuidgen | cut -d'-' -f1)

KERNEL_PATH="/home/janek/firecracker/vmlinux.bin"

CURR_DIR=$(pwd)

CONFIG_FILE="$CURR_DIR/vm_config-$EXEC_ID.json"
SOCK_PATH="$CURR_DIR/firecracker-${EXEC_ID}.socket"

ROOTFS="$CURR_DIR/rootfs.ext4"

cat > "$CONFIG_FILE" << EOF
{
  "boot-source": {
    "kernel_image_path": "$KERNEL_PATH",
    "boot_args": "console=ttyS0 reboot=k panic=-1 pci=off nomodules i8042.noaux i8042.nomux i8042.nopnp i8042.nokbd systemd.unit=executor.service"
  },
  "drives": [
    {
      "drive_id": "rootfs",
      "path_on_host": "$ROOTFS",
      "is_root_device": true,
      "is_read_only": false
    }
  ],
  "machine-config": {
    "vcpu_count": 1,
    "mem_size_mib": 256,
    "smt": false
  }
}
EOF

firecracker --api-sock "$SOCK_PATH" --config-file "$CONFIG_FILE"

if [ $? -eq 137 ]; then
    echo "timed out"
fi

cat firecracker.log


echo "firecracker exit"

rm -f "$SOCK_PATH" "$CONFIG_FILE"
