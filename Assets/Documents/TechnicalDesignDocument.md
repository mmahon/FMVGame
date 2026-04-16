---
description: Technical architecture and systems for the FMV game, including scene management, state tracking, and choice systems.
applicability: Use whenever implementing new features or modifying existing systems to ensure architectural consistency.
---

# Technical Design Document - FMV Game

## Architecture Overview

The FMV game is built around a scene-based state machine with conditional branching. Core systems include scene management, game state tracking, choice evaluation, and UI presentation.

## Core Systems

### 1. FMV Scene System

**FMVScene**
- Represents a single scene in the narrative
- Contains:
  - Unique scene ID
  - Scene title and description text
  - Placeholder visual identifier (color)
  - List of up to 3 choices leading to other scenes
  - Optional: Items to add to inventory when visited
  - Optional: Decision flags to set when visited

**FMVChoice**
- Represents a choice option presented to the player
- Contains:
  - Display text (what the player sees)
  - Target scene ID (where this choice leads)
  - Conditions (requirements to show this choice)

**FMVCondition**
- Defines requirements for a choice to be available
- Condition types:
  - Has visited scene(s)
  - Has inventory item(s)
  - Has made decision(s)
  - Custom logic flags

### 2. Game State System

**FMVGameState**
- Singleton that persists throughout a playthrough
- Tracks:
  - **Visited Scenes**: HashSet<string> of scene IDs
  - **Inventory**: List<string> of collected items
  - **Decisions**: Dictionary<string, bool> of key choice flags
- Methods:
  - VisitScene(sceneId)
  - AddItem(itemId)
  - SetDecision(decisionId, value)
  - HasVisitedScene(sceneId)
  - HasItem(itemId)
  - HasMadeDecision(decisionId)

### 3. FMV Manager

**FMVManager**
- Main controller for the FMV experience
- Responsibilities:
  - Scene database management (all 30 scenes)
  - Current scene tracking
  - Scene transitions
  - Choice evaluation and filtering
  - UI coordination
  - Game state management

**Scene Flow**
1. Load scene by ID
2. Mark scene as visited in state
3. Apply any automatic state changes (add items, set flags)
4. Display scene content (placeholder video + text)
5. Evaluate and display available choices (filter by conditions)
6. Wait for player choice
7. Transition to target scene
8. Repeat

### 4. UI System (Unity UI Canvas)

**FMVUIController**
- Renders the game UI using Unity's modern UI Canvas (UnityEngine.UI) and generic events.
- Displays:
  - **Video Area**: RawImage component displaying the RenderTexture with scene title (placeholder).
  - **Description Area**: Text for Scene description text.
  - **Choice Buttons**: Up to 3 Unity UI Buttons for filtered choices.
  - **Debug Info** (optional): Current scene, visited scenes, inventory.

**Layout Structure**
```text
+----------------------------------+
|      Video Area (RawImage)       |
|      (Colored Screen + Title)    |
+----------------------------------+
|      Scene Description Text      |
+----------------------------------+
| [Choice 1 Button]                |
| [Choice 2 Button]                |
| [Choice 3 Button]                |
+----------------------------------+
| Debug: Inventory / Visited       |
+----------------------------------+
```

## Data Structure

### Scene Definition Approach - Markdown-Based (NO CODE REQUIRED)

**All scenes are defined in a simple text file: `Resources/FMV/Scenes.txt`**

This makes the system:
- ✅ Easy to edit (any text editor)
- ✅ No C# knowledge required
- ✅ Perfect for writers/designers
- ✅ Great for templates and reuse
- ✅ Version control friendly
- ✅ No recompilation needed

**Example Scene Definition:**
```
===== SCENE: hub_main =====
Title: Main Hub
Description: You stand at the crossroads. Multiple paths lie ahead.
Ending: No

CHOICES:
- Investigate the warehouse → warehouse_entrance
- Visit the old library → library
  [requires: item:library_key]
- Return home → home
```

**The System:**
1. `FMVSceneParser` reads and parses `Scenes.txt`
2. Creates `FMVScene` objects automatically
3. `FMVSceneDatabase` serves parsed scenes to the game
4. Edit text file → Press Play → See changes immediately

**Fallback**: If markdown file is missing/invalid, falls back to hardcoded scenes (deprecated)

### Scene Graph Structure
- **Hub Scenes**: Central locations with multiple branching paths
- **Investigation Scenes**: Linear or branching sequences
- **Ending Scenes**: Terminal nodes (3 total)

### Condition Evaluation
Conditions are evaluated at choice display time:
1. For each choice in current scene
2. Check all conditions using FMVGameState
3. If all conditions pass, display the choice
4. If any condition fails, hide the choice

## Integration with Existing Systems

### UpdateManager Integration
- FMVManager can register with UpdateManager for frame updates if needed
- Currently using MonoBehaviour Update for simplicity

### Event System Integration
- Could use GameEventBus for scene transitions if needed
- For now, direct method calls are sufficient

## Performance Considerations

### State Management
- HashSet for visited scenes (O(1) lookup)
- Dictionary for decisions (O(1) lookup)
- List for inventory (small size, O(n) acceptable)

### Scene Loading
- All scenes loaded into memory at startup (30 scenes is manageable)
- No dynamic loading required
- Future: Could implement lazy loading if scene count increases significantly

## Development Workflow

### Adding New Scenes (Markdown-Based)
1. Open `Assets/Resources/FMV/Scenes.txt` in any text editor
2. Add new scene using the format:
   ```
   ===== SCENE: new_scene_id =====
   Title: Scene Title
   Description: What happens here...
   Ending: No
   
   CHOICES:
   - Choice text → target_scene
   ```
3. Save the file
4. Press Play in Unity - changes are automatically loaded
5. No C# coding or recompilation required!

### Scene Format Features
- **Conditional Choices**: `[requires: item:key]`
- **Auto Items**: `Auto Items: key, photo`
- **Decision Flags**: `[sets: made_choice]`
- **Multiple Conditions**: `[requires: item:a, item:b, visited:c]`
- **Negative Conditions**: `[requires: !item:bomb]`

See `SCENE_FORMAT_GUIDE.md` for complete documentation.

### Testing Branching
- Debug UI shows current state (visited, inventory, decisions)
- Can manually add items/flags for testing via debug controls
- Scene graph visualization would be beneficial (future enhancement)

## Visual Content Pipeline

### Video Playback System (IMPLEMENTED)

**Two-Phase Visual Flow**
1. **Video Phase**: Video plays when scene is entered (if exists)
2. **Choice Phase**: Static image displays after video ends, player makes choice

**Video Player Implementation**
- Unity VideoPlayer component attached to FMVManager
- RenderTexture output (1920x1080)
- Video files loaded from `Resources/FMVVideos/{sceneId}`
- Supported formats: .mp4 (H.264), .mov, .webm, .avi
- Event-driven: `loopPointReached` triggers transition to static phase
- Error handling: Falls back to static image on video load failure

**Video Playback State Machine**
```
Scene Transition
    ↓
Try Load Video → Video Found? → YES → Play Video
    ↓                              ↓
    NO                         Video Ends
    ↓                              ↓
Show Static Image ← ← ← ← ← ← ← ← ←
    ↓
Display Choices
    ↓
Wait for Player Decision
    ↓
Next Scene Transition
```

**Static Image System**
- 2D atmospheric images for each scene
- Images stored in `Resources/Generated2D/FMV/Scenes/`
- Loaded via Resources.Load and cached in memory
- Displayed using a Canvas RawImage configured for appropriate scaling
- Serves dual purpose:
  1. Fallback when video doesn't exist
  2. "Freeze frame" after video ends during choice selection

**Image Loading Strategy**
- Lazy loading: Images loaded when scene is first visited
- Cached in Dictionary for subsequent visits
- Path format: `"Generated2D/FMV/Scenes/{sceneId}"`

**UI Behavior**
- During video playback:
  - Video fills top 40% of screen
  - Scene title hidden
  - Choices hidden
  - Description still visible
- After video / with static image:
  - Static image fills top 40% of screen
  - Scene title overlaid with semi-transparent background
  - Choices visible and interactive
  - Description visible

**Resource Management**
- Videos loaded on-demand per scene (not cached)
- Static images cached after first load
- RenderTexture created once at initialization
- VideoPlayer component reused for all scenes

## Scalability Considerations

- **Scene Count**: Current architecture supports 50+ scenes easily
- **Choice Complexity**: Condition system is extensible for complex logic
- **State Size**: Current state tracking is minimal; can expand as needed

## Future Technical Enhancements
- Save/Load serialization of FMVGameState
- Scene flowchart editor/visualizer
- Video preloading/streaming
- Localization support for choice text
- Analytics/telemetry for player choices
