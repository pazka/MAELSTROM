@echo off
echo Building and running Shader Million Points visualization...
dotnet build src.csproj
if %ERRORLEVEL% EQU 0 (
    echo Build successful! Running the shader-based program...
    dotnet run --project src.csproj -- ShaderMillionPoints
) else (
    echo Build failed!
    pause
)
