# TowerDefence Project Architecture Documentation

## 📋 Project Overview

**Project Name:** TowerDefence (Witch Defense Game)  
**Genre:** Tower Defense with RPG elements  
**Target Platform:** PC (Steam)  
**Unity Version:** 2023.x with HDRP  
**Architecture Pattern:** Service-oriented with Dependency Injection (Zenject)

### Game Concept
A story-driven tower defense game featuring a young witch and her squad defending against monster invasions. The game combines classic tower defense mechanics with RPG character progression, magic systems, and a rich narrative about witches defending humanity from interdimensional threats.

---

## 🏗️ Technical Architecture

### Core Technologies & Dependencies

#### **Dependency Injection Framework**
- **Zenject** - Primary DI container for service management
- Provides clean separation of concerns and testability

#### **Reactive Extensions**
- **R3** (v1.3.0) - Reactive programming for Unity
- **ObservableCollections** (v3.3.4) - Reactive collections
- Used for UI bindings and event-driven programming

#### **Async Programming**
- **UniTask** - High-performance async/await for Unity
- **Microsoft.Bcl.AsyncInterfaces** (v6.0.0) - Async interfaces support

#### **Animation & Effects**
- **DOTween** - Powerful animation library for UI and gameplay
- HDRP - High Definition Render Pipeline for advanced graphics

#### **Data Management**
- **Newtonsoft.Json** (v13.0.3) - JSON serialization for save system
- **System.ComponentModel.Annotations** (v5.0.0) - Data validation

#### **Performance**
- **System.Runtime.CompilerServices.Unsafe** (v6.0.0) - Low-level optimizations
- **System.Threading.Channels** (v8.0.0) - High-performance channels

---

## 📁 Project Structure

```
Assets/Scripts/
├── Core/                           # Core engine and infrastructure
│   ├── ConfigsExample/             # ScriptableObject configuration examples
│   │   ├── AudioConfig.cs          # Audio settings configuration
│   │   ├── BalanceConfig.cs        # Game balance parameters
│   │   ├── GameConfig.cs           # General game settings
│   │   └── UIConfig.cs             # UI animation and behavior settings
│   ├── Controllers/                # Scene and flow controllers
│   │   └── LoadSceneController.cs  # Handles scene transitions
│   ├── Enums/                      # Game enumerations
│   │   └── SceneId.cs              # Scene identifier enum
│   ├── Installers/                 # Zenject dependency injection setup
│   │   ├── GameSceneInstaller.cs   # Scene-level service bindings
│   │   └── ProjectContextInstaller.cs # Global service bindings
│   └── Services/                   # Core service implementations
│       ├── Camera/                 # Camera management system
│       │   ├── ICameraController.cs # Camera controller interface
│       │   ├── CameraController.cs # Camera positioning and control
│       │   └── CameraControllerTester.cs # Controller testing
│       ├── Config/                 # Configuration management
│       ├── Factory/                # Object creation services
│       ├── Level/                  # Level management
│       ├── Pool/                   # Object pooling system
│       ├── Save/                   # Save/load functionality
│       ├── Scene/                  # Scene management
│       ├── UI/                     # UI page management
│       └── Wave/                   # Enemy wave management
├── Game/                           # Game-specific functionality
│   ├── Enemy/                      # Enemy system implementation
│   │   ├── Components/             # Enemy components (Health, Movement, etc.)
│   │   ├── Configs/                # Enemy configurations and repository
│   │   ├── Services/               # Enemy management services
│   │   └── Enemy.cs                # Main enemy class and enums
│   ├── Objects/                    # Game entities
│   │   ├── Projectile.cs           # Projectile behavior with pooling
│   │   └── Tower.cs                # Tower behavior and targeting
│   └── Path/                       # Path and waypoint system
│       ├── Waypoint.cs             # Base waypoint component
│       ├── SpawnPoint.cs           # Enemy spawn point
│       ├── EndPoint.cs             # Player base/end point
│       ├── IntermediateWaypoint.cs # Intermediate path points
│       └── LevelMap.cs             # Main level map component
└── UI/                             # User interface components
    └── Base/                       # Base UI components
        ├── IPageBase.cs            # UI page interface
        └── PageBase.cs             # Base UI page implementation
```

---

## 🛣️ Path System Architecture

### Level Design Tools

The path system provides comprehensive tools for level designers to create and manage enemy paths:

#### **Core Components**
- **LevelMap** - Main component managing all waypoints and paths
- **Waypoint** - Base class for all path points
- **SpawnPoint** - Enemy spawn locations with direction and radius
- **EndPoint** - Player base with damage zones
- **IntermediateWaypoint** - Path control points with turn settings

#### **Editor Tools**
- **LevelMapEditor** - Custom inspector with quick actions
- **WaypointEditor** - Specialized editors for each waypoint type
- **WaypointPlacementTool** - Scene view tool for interactive placement
- **PathValidator** - Comprehensive validation and analysis
- **PathManagerWindow** - Centralized path management interface

#### **Key Features**
- **Visual Path Preview** - Real-time path visualization in Scene View
- **Automatic Terrain Snapping** - Waypoints automatically align to terrain
- **Validation System** - Comprehensive error checking and warnings
- **Path Analysis** - Timing analysis and tower placement recommendations
- **Import/Export** - JSON-based path data exchange
- **Undo/Redo Support** - Full Unity Editor integration

### Waypoint System Design

```csharp
// Hierarchy and specialization
Waypoint (base)
├── SpawnPoint - Enemy spawn with direction and radius
├── EndPoint - Player base with damage zones  
└── IntermediateWaypoint - Path control with turn types
```

#### **Waypoint Properties**
- **Index** - Order in path sequence
- **Gizmo Visualization** - Color-coded type indication
- **Terrain Snapping** - Automatic surface alignment
- **Validation** - Position and connectivity checks

#### **SpawnPoint Features**
- **Spawn Direction** - Enemy facing when spawned
- **Spawn Radius** - Random positioning area
- **Concurrent Limit** - Maximum simultaneous enemies
- **Visual Indicators** - Direction arrow and radius circle

#### **EndPoint Features**
- **Base Radius** - Protected area around base
- **Damage Zone** - Area where enemies damage base
- **Health System** - Base integrity tracking
- **Zone Visualization** - Concentric circles for areas

#### **IntermediateWaypoint Features**
- **Turn Types** - Sharp, Smooth, or Custom turning
- **Influence Radius** - Spline smoothing control
- **Smoothing Factor** - Custom curve adjustment
- **Visual Feedback** - Turn type indicators

### Editor Workflow

#### **Level Creation Process**
1. **Setup** - Create LevelMap component and assign Terrain
2. **Path Design** - Use Placement Tool to add waypoints
3. **Validation** - Run automated checks for issues
4. **Optimization** - Analyze and adjust based on recommendations
5. **Testing** - Preview enemy movement and timing

#### **Quick Actions Available**
- **Add Waypoints** - One-click creation of different types
- **Collect from Scene** - Automatically find and organize waypoints
- **Snap to Terrain** - Batch terrain alignment
- **Refresh Indices** - Renumber waypoints in sequence
- **Auto-Fix Issues** - Resolve common validation problems

#### **Validation Checks**
- **Structural** - Required waypoints and proper sequencing
- **Positioning** - Terrain bounds and obstacle detection
- **Distances** - Minimum/maximum spacing between points
- **Terrain Compatibility** - Slope analysis and accessibility
- **Path Quality** - Length, complexity, and balance analysis

#### **Analysis Features**
- **Timing Analysis** - Enemy traversal time calculations
- **Tower Recommendations** - Optimal defensive positions
- **Path Statistics** - Length, turns, and complexity metrics
- **Balance Assessment** - Difficulty and gameplay flow evaluation
```

---

## 🔧 Service Architecture

### Dependency Injection Pattern

The project uses **Zenject** for dependency injection with a two-tier architecture:

#### **Project Context (Global Services)**
Services that persist throughout the entire game lifecycle:

| Service | Interface | Implementation | Purpose |
|---------|-----------|----------------|---------|
| Save System | `ISaveService` | `SaveService` | Player data persistence with JSON serialization |
| Scene Management | `ISceneManagerService` | `SceneManagerService` | Scene transitions and loading |
| Object Factory | `IGameFactory` | `GameFactory` | Centralized object instantiation |
| UI Management | `IUIPageService` | `UIPageService` | Global UI page orchestration |
| Object Pooling | `IPoolService` | `PoolService` | Performance optimization through object reuse |
| Configuration | `IConfigService` | `ConfigService` | ScriptableObject configuration management |

#### **Scene Context (Local Services)**
Services and controllers recreated for each scene:

- **ICameraController** - Camera positioning and control (MonoBehaviour)
- **ILevelService** - Level management and configuration
- **IWaveService** - Enemy wave spawning and management
- **IBattleService** - Combat system management (planned)
- **IHeroService** - Witch squad management (planned)
- **IEnemyService** - Enemy behavior and AI ✅ **IMPLEMENTED**
- **ITowerService** - Tower placement and upgrades (planned)
- **ISpellService** - Magic system implementation (planned)

### Service Injection Example

```csharp
public class ExampleGameClass : MonoBehaviour
{
    [Inject] private ISaveService saveService;
    [Inject] private IPoolService poolService;
    [Inject] private IConfigService configService;
    [Inject] private ICameraController cameraController;
    
    private void Start()
    {
        var gameConfig = configService.GetConfig<GameConfig>();
        var savedData = saveService.LoadJson<PlayerData>("player_progress");
        
        // Position camera at specific location
        cameraController.PositionCamera(Vector3.zero, height: 20f, angle: 45f);
    }
}
```

---

## 🎮 Game Systems

### Object Pooling System

**Purpose:** Optimize performance by reusing GameObjects instead of constant instantiation/destruction.

**Key Components:**
- `IPoolService` - Main pooling interface
- `PoolService` - MonoBehaviour implementation with global lifetime
- `IPoolable` - Interface for poolable objects
- `PoolableObject` - Base class for pooled objects

**Usage Example:**
```csharp
// Get object from pool
var projectile = poolService.Get(projectilePrefab, firePoint.position, firePoint.rotation);

// Configure and launch
projectile.Launch(startPos, targetDirection, speed, damage);

// Return to pool when done
projectile.ReturnToPool();
```

### Configuration Management

**Purpose:** Centralized management of game settings through ScriptableObjects.

**Features:**
- Automatic loading from `Resources/Configs/` folder
- Type-safe configuration retrieval
- Runtime configuration reloading
- Validation and constraints

**Available Configurations:**
- `AudioConfig` - Volume settings, mixer parameters, audio pooling
- `BalanceConfig` - Tower stats, enemy scaling, economy parameters
- `GameConfig` - Core game settings, performance options
- `UIConfig` - Animation settings, UI behavior parameters

### Save System

**Purpose:** Persistent player data storage using PlayerPrefs with JSON serialization.

**Features:**
- String and object serialization
- JSON support through Newtonsoft.Json
- Type-safe loading with generics
- Error handling for missing data

### Scene Management

**Purpose:** Seamless transitions between game scenes.

**Features:**
- Async scene loading
- Target scene tracking
- Integration with save system

---

## 🎯 Game Objects

### Tower System

**Tower.cs** - Main tower behavior:
- **Targeting System** - Finds closest enemies within range
- **Attack System** - Rotates to target and fires projectiles
- **Projectile Integration** - Uses object pooling for efficient projectile management
- **Upgrade Support** - Configurable damage, range, and attack speed

### Projectile System

**Projectile.cs** - Pooled projectile behavior:
- **Movement** - Linear and homing projectile support
- **Damage System** - Configurable damage values
- **Lifetime Management** - Automatic return to pool
- **Collision Detection** - Trigger-based enemy damage

### Enemy System ✅ **IMPLEMENTED**

**Universal Enemy Architecture** - Single `Enemy` class handles all enemy types through configuration:

#### **Core Components:**
- **HealthComponent** - Health management and death handling
- **MovementComponent** - Path-following movement with async operations
- **ResistanceComponent** - Damage type resistances and immunities
- **AbilityComponent** - Enemy special abilities and cooldown management

#### **Configuration System:**
- **EnemyConfig** - Game logic configuration (health, speed, resistances, abilities)
- **EnemyVisualConfig** - Visual assets (prefabs, animations, sounds, effects)
- **EnemyConfigRepository** - Centralized configuration management

#### **Enemy Types Supported:**
11 enemy types from design document:
- **Basic**: Animals, Magical Creatures
- **Infantry**: Bandits, Warriors, Knights, Mercenaries
- **Support**: Bards, Alchemists, Priests
- **Elite**: Monsters, Succubi

#### **Key Features:**
- **Reactive Programming** - R3 integration for health, position, movement state
- **Object Pooling** - Full IPoolable implementation for performance
- **Path Integration** - Uses existing LevelMap and SpawnPoint system
- **Dependency Injection** - Zenject integration for service access
- **Component-Based** - Modular design for easy extension

#### **Service Architecture:**
```
WaveService (planned) → EnemyService → Enemy instances
                     ↓
              EnemyConfigRepository → Configs
                     ↓
              IPoolService + IGameFactory
```

**EnemyService** provides:
- `SpawnEnemy(EnemyType, Vector3)` - Create enemy at position
- `SpawnEnemyAtSpawnPoint(EnemyType)` - Use level's spawn point
- `GetAliveEnemiesCount()` - Track active enemies
- `ClearAllEnemies()` - Cleanup for level transitions
- Events: `OnEnemySpawned`, `OnEnemyDied`, `OnEnemyReachedBase`

#### **Integration Points:**
- **SpawnPoint** - Uses existing path system for enemy spawn locations
- **LevelMap** - Automatic path following from spawn to end point
- **IPoolService** - Performance optimization through object reuse
- **IConfigService** - Configuration loading from Resources/Configs/

#### **Configuration Assets:**

**Game Logic Configurations (Assets/Resources/Configs/Enemies/):**
- AnimalConfig.asset - Animal stats (HP: 50, Speed: 4.0)
- MagicalCreatureConfig.asset - Magical creature stats (HP: 80, Speed: 3.5)
- BanditConfig.asset - Bandit stats (HP: 100, Speed: 3.5)
- WarriorConfig.asset - Warrior stats (HP: 150, Speed: 2.5)
- KnightConfig.asset - Knight stats (HP: 250, Speed: 2.0)
- MercenaryConfig.asset - Mercenary stats (HP: 180, Speed: 3.0)
- BardConfig.asset - Bard stats with speed boost ability
- AlchemistConfig.asset - Alchemist stats with heal ability
- PriestConfig.asset - Priest stats with shield ability
- MonsterConfig.asset - Monster stats with area attack
- SuccubusConfig.asset - Succubus stats with charm ability

**Visual Configurations (Assets/Resources/Configs/EnemiesVisual/):**
- AnimalVisualConfig.asset - Animal prefabs and animations
- MagicalCreatureVisualConfig.asset - Magical creature visuals
- BanditVisualConfig.asset - Bandit visuals
- WarriorVisualConfig.asset - Warrior visuals
- KnightVisualConfig.asset - Knight visuals
- MercenaryVisualConfig.asset - Mercenary visuals
- BardVisualConfig.asset - Bard visuals
- AlchemistVisualConfig.asset - Alchemist visuals
- PriestVisualConfig.asset - Priest visuals
- MonsterVisualConfig.asset - Monster visuals
- SuccubusVisualConfig.asset - Succubus visuals

**Repository:**
- EnemyConfigRepository.asset - Central registry ready for Unity Editor setup (11 game + 11 visual configs)

**Status:** All configurations created with correct GUID references and Unity recognition. Ready for EnemyService usage.

#### **File Structure:**
```
Assets/Scripts/Game/Enemy/
├── Components/           # Modular enemy components
│   ├── HealthComponent.cs
│   ├── MovementComponent.cs
│   ├── ResistanceComponent.cs
│   └── AbilityComponent.cs
├── Configs/             # Configuration system
│   ├── EnemyConfig.cs
│   ├── EnemyVisualConfig.cs
│   └── EnemyConfigRepository.cs
├── Services/            # Enemy management
│   ├── IEnemyService.cs
│   └── EnemyService.cs
├── Enemy.cs            # Main enemy class
├── EnemyType.cs        # Enemy type enumeration
├── EnemyCategory.cs    # Category grouping
├── ResistanceType.cs   # Resistance types
└── AbilityType.cs      # Ability types
```

---

## 🎨 UI Architecture

### Page-Based UI System

**Base Components:**
- `IPageBase` - Interface for all UI pages
- `PageBase` - Base implementation with DOTween animations
- `UIPageService` - Global page management and transitions

**Features:**
- Fade in/out animations with DOTween
- Page layering and z-order management
- Centralized page registry
- Smooth transitions between states

---

## 📊 Current Implementation Status

### ✅ Implemented Systems
- **Core Infrastructure** - DI container, service architecture
- **Object Pooling** - Full implementation with MonoBehaviour support
- **Configuration Management** - ScriptableObject-based system
- **Save System** - JSON serialization with PlayerPrefs
- **Camera Controller** - MonoBehaviour-based camera positioning and control
- **Level Service** - Level management and configuration
- **Wave Service** - Enemy wave spawning system
- **Enemy System** - Complete universal enemy architecture with all 11 enemy types ✅ **FULLY CONFIGURED**
- **Basic Gameplay** - Tower and projectile mechanics
- **UI Foundation** - Page-based system with animations
- **Path System** - Complete level design tools and waypoint system
- **Editor Tools** - Comprehensive path management and validation
- **Enemy Configurations** - All 11 enemy configs with proper Unity recognition and .meta files ✅ **COMPLETE**

### 🔄 In Development
- **Spline System** - Smooth path generation from waypoints
- **Game Scene Services** - Battle and hero management
- **Advanced UI** - Game-specific pages and HUD
- **Audio System** - Sound effects and music management

### 📋 Planned Features
- **Magic System** - Spell casting and effects
- **Character Progression** - Experience and skill trees
- **Story System** - Narrative progression and cutscenes
- **Advanced AI** - Complex enemy behaviors
- **Visual Effects** - Particle systems and shader effects

---

## 🚀 Development Workflow

### Adding New Services

1. **Create Interface** in appropriate service folder
2. **Implement Service** with proper logging
3. **Register in Installer** (Project or Scene context)
4. **Inject Dependencies** where needed
5. **Add Unit Tests** (when testing framework is added)

### Creating New Game Objects

1. **Inherit from PoolableObject** for pooled objects
2. **Implement Required Interfaces** (`IPoolable`, `IDamageable`, etc.)
3. **Add Configuration Support** via ScriptableObjects
4. **Use Dependency Injection** for service access

### Configuration Management

1. **Create ScriptableObject Class** with `[CreateAssetMenu]`
2. **Add to Resources/Configs/** folder
3. **Access via ConfigService** with type-safe methods
4. **Add Validation** in `OnValidate()` method

---

## 🔍 Architecture Benefits

### Scalability
- **Service-oriented design** allows easy feature addition
- **Dependency injection** provides loose coupling
- **Object pooling** ensures performance at scale

### Maintainability
- **Clear separation of concerns** between systems
- **Interface-based design** enables easy testing and mocking
- **Centralized configuration** simplifies parameter tuning

### Performance
- **Object pooling** eliminates garbage collection spikes
- **Async operations** prevent frame drops during loading
- **Reactive programming** optimizes UI updates

### Flexibility
- **ScriptableObject configs** allow runtime parameter changes
- **Service abstraction** enables easy implementation swapping
- **Page-based UI** supports complex navigation flows

---

## 📝 Development Guidelines

### Code Style
- Use **interface-first design** for all services
- Implement **proper logging** with service tags
- Follow **async/await patterns** for long operations
- Use **object pooling** for frequently instantiated objects

### Performance Considerations
- **Pool projectiles, effects, and UI elements**
- **Use UniTask** instead of Coroutines for async operations
- **Minimize allocations** in Update loops
- **Cache component references** where possible

### Testing Strategy
- **Unit test services** in isolation
- **Integration test** service interactions
- **Performance test** pooling and memory usage
- **Playtesting** for game balance and feel

---

## 🔗 Key Dependencies Summary

| Dependency | Version | Purpose |
|------------|---------|---------|
| Zenject | Latest | Dependency injection container |
| UniTask | Latest | High-performance async operations |
| DOTween | Latest | UI and gameplay animations |
| R3 | 1.3.0 | Reactive programming |
| Newtonsoft.Json | 13.0.3 | JSON serialization |
| HDRP | Unity 2023.x | Advanced rendering pipeline |

---

## 📷 Camera System Architecture (Updated)

### Simplified Camera Design

The camera system has been refactored for better maintainability and clearer separation of concerns:

#### **Architecture Components**
- **ICameraService** - High-level interface for camera operations
- **CameraService** - Handles calculations, level analysis, and coordination
- **ICameraController** - Low-level interface for camera manipulation
- **CameraController** - Direct camera positioning and projection control
- **CameraParams** - Simple data structure for camera parameters

#### **Key Improvements**

**✅ Removed Complexity:**
- Eliminated complex `CameraData` structure
- Removed duplicate calculation logic
- Simplified parameter passing
- Reduced code by ~40%

**✅ Better Architecture:**
- Clear separation: Service (calculations) vs Controller (execution)
- Proper Zenject injection instead of `FindAnyObjectByType`
- Single responsibility principle
- Easier testing and maintenance

**✅ Enhanced Features:**
- Automatic level analysis and optimal positioning
- Support for both orthographic and perspective projection
- CameraTarget detection for custom positioning
- Configurable default parameters

#### **Usage Examples**

**Basic Usage:**
```csharp
[Inject] private ICameraService cameraService;

public void PositionForLevel(LevelMap levelMap)
{
    // Service automatically calculates optimal parameters
    cameraService.PositionCameraForLevel(levelMap);
}

public void PositionManually(Vector3 center)
{
    // Manual positioning with defaults
    cameraService.PositionCamera(center, height: 25f, angle: 45f);
}
```

**Advanced Controller Access:**
```csharp
[Inject] private ICameraController cameraController;

public void CustomCameraSetup()
{
    // Direct controller access for complex scenarios
    cameraController.SetIsometricView(
        Vector3.zero, 
        height: 30f, 
        angle: 60f, 
        useOrthographic: true, 
        orthographicSize: 15f
    );
}
```

#### **Dependency Injection Setup**

In `GameSceneInstaller.cs`:
```csharp
private void BindGameplayServices()
{
    // Camera System - improved architecture
    Container.Bind<ICameraService>().To<CameraService>().AsSingle().NonLazy();
    Container.Bind<ICameraController>().FromComponentInHierarchy().AsSingle();
}
```

#### **Configuration**

CameraService supports configurable defaults:
- **Default Height**: 20f units
- **Default Angle**: 45° for isometric view
- **Default Orthographic Size**: 10f units
- **Projection Type**: Orthographic by default

#### **Level Integration**

Automatic integration with level system:
1. **Level Loading** → Camera automatically positions for new level
2. **CameraTarget Detection** → Uses custom positioning if CameraTarget exists
3. **Bounds Calculation** → Analyzes waypoints for optimal framing
4. **Smart Defaults** → Calculates height and size based on level dimensions

---


