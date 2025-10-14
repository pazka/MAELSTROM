dotnet publish -c Release -r win-x64 --self-contained true ghostNet.csproj

tar.exe -a -c -f publish.zip ./bin/Release/net9.0/win-x64

scp -r publish.zip pazka@hosh.it:/home/pazka/public/maelstrom-viz