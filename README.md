# project-union
A C# implementation of a rage-mp free-roam game mode

# Install Instructions:

## Pull Notes
Feel free to pull repo files into server-files of RAGEMP directory however I like to keep them separate, in which case just copy your server-files from RAGEMP/ and paste them anywhere then pull repo into that.

## Enable Client Side
1) Make sure you've got a TXT file in RAGEMP install directory called 'enable-clientside-cs.txt' (this is required for client side development)

## Copy DLLS
#### Go into bridge/resources/ProjectUnion/bin/Debug/netcoreapp2.0
#### copy all .dll files from there except ProjectUnion and also the runtimes folder
#### paste into bridge/runtime
