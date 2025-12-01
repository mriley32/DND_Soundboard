# NOTE: You need to have Windows .NET SDK Installed. I am using v 10.0 downloaded here:

# https://dotnet.microsoft.com/en-us/download/dotnet

# At the top level run:

dotnet build -c Release

# Then double click the .exe in the bin/Release folder to run
# Or you can run it on the command line:

./bin/Release/net10.0-windows/DND_SoundBoard.exe

# Creating a Standalone Executable (for friends)

If you want to share this app with friends who might not have .NET installed, run this command:

dotnet publish -r win-x64 -c Release --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

This will create a standalone executable in:
bin/Release/net10.0-windows/win-x64/publish/

You can zip that 'publish' folder and send it to your friends. They just need to run DND_SoundBoard.exe inside it.
