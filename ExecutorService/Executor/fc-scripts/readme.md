To provide some context

build-base.sh:
 
This is the initial scrip that deals with setting up a base
filesystem image. It uses a minimal configuration of jammy configures dns 
fetches apt sources creates necessary directories, scripts and services.
It then cleans all unnecessary stuff out to make a hollow shell of a fs.

It creates a directory 'my-rootfs-base' and file 'rootfs-base.ext4', the latter
being the actual filesystem etched onto a .ext4 the former a dedicated mount
directory used during the build process.

It is generally ill advised to run this script since it takes quite a bit of time
instead when wanting to experiment (providing of course the base configuration is sufficient)
one should rather just copy 'rootfs-base.ext4'. Nevertheless should the need
to run the script arise it should be executed as so.

sudo bash build-base.sh


build-copy.sh:

This script (for now at least is part of the deployment pipeline) it takes
the base image 'rootfs-base.ext4' copies and appends to the copy execution specific
files such as the java file. 

When sandboxing unsafe java code this should be your first step.
To be executed as so:

sudo bash build-copy.sh \<CLASS_NAME\> \<FILE_PATH\>


java-exec.sh

This script is responsible for configuring and deploying the micro-vm. 
It will create temporary files necessary for execution which will be cleaned 
up shortly after the vm terminates
To be executed as so:

sudo bash java-exec.sh
