# Maelström DataViz Project

## Goal 

Data visualization code for the project `Maelström !` by the artist Alessia Sanna. 

It is a mono repo with 3 projects inside, each one for one sculpture.

## Art

### DEAD COMMUNITIES

Visualization for the *[Coral Reef](https://www.noaa.gov/education/resource-collections/marine-life/coral-reef-ecosystems) Sculpture*

### FEED

Visualization for the *Suspended [Fish School](https://www.youtube.com/watch?v=cqDjV6lsJJU) Sculpture*

### GHOST NET

Visualization for the *[Ghost Fishing Net ](https://en.wikipedia.org/wiki/Ghost_net)Sculpture*

## Tech

### Caustics Visualization (Current Implementation)

A water-like caustics visualization system using Silk.NET and OpenGL with procedural generation algorithms.

#### Features

- **Water-like Caustics**: Realistic water caustics with walls around points
- **Property-based Visualization**: 
  - Property A: Wall width around each point
  - Property B: Agitation level of each cell
  - Property C: Color of each cell
- **Dual Window Rendering**: Seamless visualization across two windows without cuts
- **Procedural Generation**: Advanced algorithms for generating caustic patterns
- **Modular Architecture**: Separated display logic from algorithmic logic for easy customization

#### Project Structure

```
src/
├── Data/
│   └── PointData.cs              # Data structures for caustic points
├── Algorithms/
│   └── CausticAlgorithms.cs      # Procedural generation algorithms
├── Rendering/
│   └── CausticRenderer.cs        # Display logic and OpenGL rendering
├── Configuration/
│   └── VisualizationConfig.cs    # Visual parameters and settings
├── caustics/
│   └── CausticExample.cs         # Main entry point
└── CausticApplication.cs         # Application management
```

#### Getting Started

1. Build the project: `dotnet build`
2. Run the executable: `dotnet run`
3. Two windows will open showing the caustics visualization
4. Press any key in the console to start

#### Customization

- **Visual Parameters**: Edit `src/Configuration/VisualizationConfig.cs`
- **Shaders**: Modify `assets/shaders/` for visual effects
- **Algorithms**: Adjust `src/Algorithms/CausticAlgorithms.cs` for pattern generation

#### Dependencies

- Silk.NET.OpenGL (2.22.0)
- Silk.NET.Windowing (2.22.0)
- Silk.NET.Input (2.22.0)
- StbImageSharp (2.30.15)

### Previous Implementation

Originally made with Godot engine