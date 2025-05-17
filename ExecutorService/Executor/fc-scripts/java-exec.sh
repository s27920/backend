#!/bin/bash

EXEC_ID=$1
SIGNING_KEY=$2

ROOTFS="/tmp/$EXEC_ID-rootfs.ext4"
KERNEL_PATH="/app/fc-scripts/vmlinux.bin"
CONFIG_FILE="/tmp/vm_config-$EXEC_ID.json"
SOCK_PATH="/tmp/firecracker-${EXEC_ID}.socket"

STDOUT_PATH="/tmp/$EXEC_ID-OUT-LOG.log"
ANSW_PATH="/tmp/$EXEC_ID-ANSW-LOG.log"

touch "$ANSW_PATH"
touch "$STDOUT_PATH"

cat > "$CONFIG_FILE" << EOF
{
  "boot-source": {
    "kernel_image_path": "$KERNEL_PATH",
    "boot_args": "console=ttyS0 init=/sbin/init quiet loglevel=0 selinux=0 reboot=k panic=-1 pci=off nomodules i8042.noaux i8042.nomux i8042.nopnp i8042.nokbd"
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

process_control(){
  while read -r line; do
    echo "$line" > /dev/ttyS0 2>&1
    if [[ "$line" =~ ctr-"$SIGNING_KEY"- ]]; then
      if [[ "$line" =~ pof ]]; then
        curl --unix-socket "$SOCK_PATH" -X DELETE "http://localhost/"
      elif [[ "$line" =~ ans ]]; then
        echo "$line" >> "$ANSW_PATH"
      fi
    else
        echo "$line" >> "$STDOUT_PATH"
    fi
  done
}

# TODO SUPER IMPORTANT the unix socket comms ARE NOT SHUTTING THE PROCESS DOWN
timeout -s SIGKILL 15s firecracker-v1.2.0-x86_64 --api-sock "$SOCK_PATH" --config-file "$CONFIG_FILE" 2>&1 | process_control &
wait $!

if [ $? -eq 137 ]; then
    echo "timed out" >> "$STDOUT_PATH"
fi

rm -f "$SOCK_PATH" "$CONFIG_FILE" "$ROOTFS" & disown
