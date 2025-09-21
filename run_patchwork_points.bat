@echo off
echo Building and running Patchwork Million Points visualization...
dotnet build src.csproj
if %ERRORLEVEL% EQU 0 (
    echo Build successful! Running the patchwork program...
    dotnet run --project src.csproj -- PatchworkPoints
) else (
    echo Build failed!
    pause
)
