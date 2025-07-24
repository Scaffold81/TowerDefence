# Path System Usage Guide

## 🚀 Quick Start

### 1. Creating a Level
1. Create an empty GameObject in your scene
2. Add `LevelMap` component to it
3. Assign your Terrain to the LevelMap component
4. Open **Tools → Tower Defence → Path Manager**

### 2. Adding Waypoints
**Method 1: Using Path Manager Window**
- Click "Add SpawnPoint", "Add IntermediateWaypoint", or "Add EndPoint" buttons
- Position waypoints manually in Scene View

**Method 2: Using Placement Tool**
- Click "Open Placement Tool" in Path Manager
- Or use **Tools → Tower Defence → Toggle Waypoint Placement Mode**
- Left-click in Scene View to place waypoints
- Hold Shift for multiple placement
- Press ESC to exit placement mode

### 3. Configuring Waypoints

#### SpawnPoint Settings:
- **Spawn Direction**: Direction enemies face when spawned
- **Spawn Radius**: Random positioning area
- **Max Concurrent Enemies**: Limit for simultaneous spawns

#### EndPoint Settings:
- **Base Radius**: Protected area around base
- **Damage Zone Radius**: Area where enemies damage base
- **Base Health**: Starting health value

#### IntermediateWaypoint Settings:
- **Turn Type**: Sharp, Smooth, or Custom turning behavior
- **Influence Radius**: Controls spline smoothing
- **Smoothing Factor**: Custom curve adjustment (for Custom turn type)

### 4. Validation and Testing
- Use "Validate Level" button to check for issues
- "Auto-Fix Issues" resolves common problems automatically
- Check validation results for warnings and recommendations

### 5. Path Analysis
- **Analyze Timing**: Calculate enemy traversal time
- **Find Tower Positions**: Get recommended defensive positions
- **Export/Import**: Save and load path configurations

## 🛠️ Editor Tools

### Path Manager Window
**Location**: `Tools → Tower Defence → Path Manager`

**Features**:
- Level overview and statistics
- Quick waypoint creation
- Comprehensive validation
- Path timing analysis
- Tower position recommendations
- Import/Export functionality

### Waypoint Placement Tool
**Location**: `Tools → Tower Defence → Toggle Waypoint Placement Mode`

**Usage**:
- Activate placement mode
- Select waypoint type in GUI overlay
- Left-click to place waypoints
- ESC to exit

### Custom Inspectors
Each waypoint type has specialized inspector with:
- Quick actions (snap to terrain, delete, etc.)
- Type-specific settings
- Real-time validation feedback
- Scene View handles for visual editing

## 🎯 Best Practices

### Level Design Guidelines:
1. **Start with SpawnPoint** - Place at level entrance
2. **End with EndPoint** - Position at player base
3. **Add Intermediate Points** - Create interesting path curves
4. **Maintain Proper Spacing** - Keep waypoints 1-50m apart
5. **Use Terrain Snapping** - Ensure waypoints align with ground

### Path Quality Tips:
- **Avoid Sharp Turns** - Use Smooth turn type for natural flow
- **Balance Path Length** - 20-200m for good gameplay timing
- **Consider Tower Placement** - Leave space for defensive positions
- **Test Enemy Movement** - Use timing analysis to verify balance

### Validation Checklist:
- ✅ Exactly one SpawnPoint and one EndPoint
- ✅ All waypoints on terrain surface
- ✅ Reasonable distances between points
- ✅ No obstacles blocking path
- ✅ Manageable slopes and terrain

## 🔧 Troubleshooting

### Common Issues:

**"Waypoint is not positioned on terrain surface"**
- Solution: Use "Snap to Terrain" button or enable auto-snapping

**"Waypoints too close/far apart"**
- Solution: Manually adjust positions or use validation recommendations

**"Path has many sharp turns"**
- Solution: Change IntermediateWaypoint turn types to Smooth

**"Very steep slope between waypoints"**
- Solution: Add intermediate waypoints or adjust terrain

### Performance Tips:
- Keep waypoint count reasonable (< 20 per path)
- Use "Collect Waypoints from Scene" to organize existing points
- Export/Import paths for reuse across levels

## 📁 File Structure
```
Game/Path/
├── LevelMap.cs              # Main level component
├── Waypoint.cs             # Base waypoint class
├── SpawnPoint.cs           # Enemy spawn points
├── EndPoint.cs             # Player base points
├── IntermediateWaypoint.cs # Path control points
└── PathSystemTest.cs       # Testing utilities

Editor/Path/
├── LevelMapEditor.cs       # Custom LevelMap inspector
├── WaypointEditor.cs       # Waypoint inspectors
├── PathValidator.cs        # Validation system
├── PathManagerWindow.cs    # Main editor window
└── WaypointPlacementHelper.cs # Placement tool
```

## 🎮 Integration with Game Systems

The path system is designed to integrate with:
- **Enemy AI**: Enemies will follow waypoint paths using splines
- **Wave System**: SpawnPoints coordinate with wave management
- **Tower System**: EndPoints manage base damage and health
- **Balance System**: Path analysis helps tune difficulty

Next steps: Implement spline generation and enemy movement systems!