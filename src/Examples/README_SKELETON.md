# Caustics Visualization Skeleton

This is a customizable skeleton application for creating water-like caustics visualizations with moving points. The architecture is designed to be easily extensible while keeping useful features like world position tracking and neighbor detection.

## Architecture Overview

### Core Components

1. **PointController** - Controls individual point movement and behavior
2. **NeighborDetector** - Detects nearby points and calculates wall information
3. **ShaderProperties** - Manages customizable shader properties per point
4. **PointManager** - Manages all points and their controllers
5. **SkeletonApp** - Main application class (customize this!)

## Quick Start

### 1. Basic Usage

```csharp
// Create a new skeleton app
var app = new SkeletonApp();
app.Run();
```

### 2. Customizing Point Movement

#### Option A: Use Built-in Controllers

```csharp
// Circular movement
pointManager.AddCircularPoint(point, radius: 1.0f, speed: 0.5f, phase: 0.0f);

// Wave movement
pointManager.AddWavePoint(point, amplitude: 0.8f, frequency: 1.0f, direction: Vector2.UnitX);

// Random walk
pointManager.AddRandomWalkPoint(point, maxSpeed: 0.3f);

// Static point
pointManager.AddPoint(point);
```

#### Option B: Create Custom Controllers

```csharp
public class MyCustomController : PointController
{
    public override CausticPoint UpdatePoint(float time)
    {
        // Your custom movement logic here
        Vector2 newPosition = CalculateNewPosition(time);
        
        return new CausticPoint(
            newPosition,
            _originalPoint.WallWidth,
            _originalPoint.Agitation,
            _originalPoint.Color
        );
    }
}

// Use it
var controller = new MyCustomController(point, index, worldBounds);
pointManager.AddPoint(point, controller);
```

### 3. Customizing Shader Properties

```csharp
public class MyShaderProperties : ShaderProperties
{
    public override ShaderProperties Animate(float time)
    {
        var animated = base.Animate(time);
        
        // Custom property animations
        animated.WallWidth = WallWidth * (1.0f + 0.5f * MathF.Sin(time * 2.0f));
        animated.Color = CalculateCustomColor(time);
        
        return animated;
    }
}
```

### 4. Accessing Neighbor Information

```csharp
// Get neighbors for a specific point
var neighbors = pointManager.GetNeighbors(pointIndex);

// Get walls for a specific point
var walls = pointManager.GetWalls(pointIndex);

// Get all walls in the system
var allWalls = pointManager.GetAllWalls();
```

## Advanced Customization

### 1. Override SkeletonApp

```csharp
public class MyCustomApp : SkeletonApp
{
    protected override void SetupPoints()
    {
        // Your custom point setup
    }
    
    protected override void OnUpdate(double deltaTime)
    {
        // Your custom update logic
        base.OnUpdate(deltaTime);
    }
}
```

### 2. Create Complex Movement Patterns

```csharp
public class SpiralController : PointController
{
    public override CausticPoint UpdatePoint(float time)
    {
        float angle = time * _speed + _phase;
        float radius = _baseRadius + time * _expansionRate;
        
        Vector2 offset = new Vector2(
            MathF.Cos(angle) * radius,
            MathF.Sin(angle) * radius
        );
        
        return CreateAnimatedPoint(offset);
    }
}
```

### 3. Dynamic Point Management

```csharp
// Add points at runtime
pointManager.AddCircularPoint(newPoint, 1.0f, 0.5f);

// Remove points
pointManager.Clear();

// Modify existing points
var controller = pointManager.GetController(index);
if (controller != null)
{
    // Modify controller properties
}
```

## Useful Features

### World Position Tracking
- All points maintain their world coordinates
- Easy to convert between screen and world space
- Consistent coordinate system across all components

### Neighbor Detection
- Automatic detection of nearby points
- Wall calculation between adjacent points
- Distance and direction information
- Useful for complex shader effects

### Shader Property System
- Per-point customizable properties
- Time-based animation support
- Easy to extend with new properties
- Automatic uniform management

### Modular Architecture
- Separate concerns (movement, rendering, properties)
- Easy to test individual components
- Simple to add new features
- Clean separation of logic

## Example Patterns

### 1. School of Fish
```csharp
// Create multiple points that follow each other
for (int i = 0; i < fishCount; i++)
{
    var point = new CausticPoint(/* ... */);
    var controller = new FollowingController(point, i, worldBounds, leaderIndex: i-1);
    pointManager.AddPoint(point, controller);
}
```

### 2. Ripple Effect
```csharp
// Create expanding ripples from center points
var centerPoint = new CausticPoint(center, wallWidth, agitation, color);
pointManager.AddPoint(centerPoint);

for (int ring = 0; ring < rippleRings; ring++)
{
    var ripplePoint = new CausticPoint(/* ... */);
    var controller = new RippleController(ripplePoint, ring, worldBounds, center);
    pointManager.AddPoint(ripplePoint, controller);
}
```

### 3. Particle System
```csharp
// Create a particle system with custom behaviors
for (int i = 0; i < particleCount; i++)
{
    var particle = new CausticPoint(/* ... */);
    var controller = new ParticleController(particle, i, worldBounds, lifeTime: 10.0f);
    pointManager.AddPoint(particle, controller);
}
```

## Tips for Customization

1. **Start Simple**: Begin with the basic SkeletonApp and add one feature at a time
2. **Use Built-ins**: Leverage the existing controllers before creating custom ones
3. **Test Incrementally**: Test each new feature before adding the next
4. **Profile Performance**: Monitor performance when adding many points
5. **Document Your Changes**: Keep track of custom modifications for future reference

## File Structure

```
src/
├── Core/
│   ├── PointController.cs      # Base class for point movement
│   ├── NeighborDetector.cs     # Neighbor detection and wall calculation
│   ├── ShaderProperties.cs     # Shader property management
│   └── PointManager.cs         # Main point management system
├── Examples/
│   ├── CustomSkeletonExample.cs # Example customizations
│   └── README_SKELETON.md      # This file
├── SkeletonApp.cs              # Main skeleton application
└── caustics/CausticExample.cs  # Entry point
```

This skeleton provides a solid foundation for creating complex caustics visualizations while keeping the useful features you need for wall calculations and world positioning.
