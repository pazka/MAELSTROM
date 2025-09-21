@echo off
echo Building and running Texture-Based Million Points visualization...
dotnet build src.csproj
if %ERRORLEVEL% EQU 0 (
    echo Build successful! Running the texture-based program...
    dotnet run --project src.csproj -- TexturePoints
) else (
    echo Build failed!
    pause
)
