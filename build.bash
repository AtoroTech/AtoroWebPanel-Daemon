clear
rm -r bin/Debug
dotnet build
cd bin/Debug/net6.0
./AtoroWebPanel-Daemon -reset
./AtoroWebPanel-Daemon
