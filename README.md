# project-union
A C# implementation of a rage-mp free-roam game mode

# Install Instructions:

## Pull Notes
Feel free to pull repo files into server-files of RAGEMP directory however I like to keep them separate. Open up Server/ProjectUnion/ProjectUnion.sln and update the pths to all post-build-events to point to your server-files/client_packages folder. Make sure to create the folders that are mentioned otherwise you will get an error that the del cmd is wrong. Build solution and you should be all set. If you get dll issues, copy all .dlls expect ProjectUnion.dll from bridge/resources/projectunion into bridge/runtimes.

## Enable Client Side
1) Make sure you've got a TXT file in RAGEMP install directory called 'enable-clientside-cs.txt' (this is required for client side development)
