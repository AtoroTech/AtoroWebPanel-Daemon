# Scripted install

You can download the script with `wget`:
```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
```

Before running this script, you'll need to grant permission for this script to run as an executable:
```bash
chmod +x ./dotnet-install.sh
```

Now let's install our runtime
```bash
./dotnet-install.sh --channel 6.0
```

Set environment variables system-wide
```bash
cd
export DOTNET_ROOT=$HOME/.dotnet
export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
```