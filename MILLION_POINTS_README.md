# Million Points Visualization

This program displays one million white points on a dark background, each following their own unique pseudo-random path.

## Features

- **1,000,000 animated points** - Each point follows its own pseudo-random path using multiple sine wave functions
- **Smooth performance** - Optimized rendering with efficient VBO updates
- **Interactive controls** - Adjust speed, pause/resume, and reset the simulation
- **Real-time statistics** - FPS counter and parameter display in the window title

## Controls

- **ESC** - Exit the program
- **SPACE** - Pause/Resume the animation
- **UP Arrow** - Increase animation speed (up to 5x)
- **DOWN Arrow** - Decrease animation speed (down to 0.1x)
- **R** - Reset all points to random starting positions
- **1** - Set to 100,000 points (better performance)
- **2** - Set to 500,000 points (balanced performance)
- **3** - Set to 1,000,000 points (full visualization)
- **P** - Toggle between SMOOTH mode (all points update every frame) and PERF mode (batched updates)

## How to Run

### Option 1: Using the batch file
```bash
run_million_points.bat
```

### Option 2: Using dotnet directly
```bash
dotnet build src.csproj
dotnet run --project src.csproj -- MillionPoints
```

## Technical Details

### Pseudo-Random Path Generation
Each point uses four unique seeds to generate smooth, organic-looking movement:
- Multiple sine waves with different frequencies create complex, non-repetitive paths
- Velocity-based movement with damping prevents infinite acceleration
- Points wrap around screen edges for continuous movement

### Performance Optimizations
- Pre-computed seeds for each point to avoid random number generation during updates
- VBO updates only when necessary (at ~60fps)
- Efficient OpenGL point rendering
- Soft circular point rendering with alpha blending for better visual quality

### Visual Effects
- Soft circular points with distance-based alpha falloff
- Subtle color variation based on time and position
- Smooth blending for overlapping points

## Requirements

- .NET 9.0
- Silk.NET libraries (automatically restored via NuGet)
- OpenGL-compatible graphics card

## Performance Notes

The program is optimized to run smoothly with one million points. Performance may vary depending on your graphics card and system specifications. If you experience performance issues, you can modify the `_pointCount` variable in the source code to reduce the number of points.
