rm -Rf publishDir bin obj
dotnet publish -c Release -r win-x64 --self-contained true ghostNet.csproj

mkdir publishDir
mkdir publishDir\ghostNet
cp -r ./bin/Release/net9.0/win-x64/* ./publishDir/ghostNet


rm -Rf bin obj
dotnet publish -c Release -r win-x64 --self-contained true feed.csproj

mkdir publishDir\feed
cp -r ./bin/Release/net9.0/win-x64/* ./publishDir/feed


rm -Rf bin obj
dotnet publish -c Release -r win-x64 --self-contained true corals.csproj

mkdir publishDir\corals
cp -r ./bin/Release/net9.0/win-x64/* ./publishDir/corals

tar.exe -z -c -f publish.tar.gz ./publishDir

:: scp -r publish.zip pazka@hosh.it:/home/pazka/public/maelstrom-viz