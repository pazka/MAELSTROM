@echo off
echo Building and running Million Points visualization...
dotnet build src.csproj
if %ERRORLEVEL% EQU 0 (
    echo Build successful! Running the program...
    dotnet run --project src.csproj -- MillionPoints
) else (
    echo Build failed!
    pause
)
