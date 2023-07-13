rm -r bin/Debug
dotnet build
cd bin/Debug/net6.0
./McControllerX-Daemon -reset
./McControllerX-Daemon
