---
description: Technical architecture and systems for the FMV game, including scene management, state tracking, and choice systems.
applicability: Use whenever implementing new features or modifying existing systems to ensure architectural consistency.
---

# Technical Design Document - FMV Game

## Architecture Overview

The FMV game is a data-driven, scene-based state machine with conditional branching. Scenes are plain-text files parsed at runtime. Core systems are scene parsing, scene management and video playback, game state tracking, choice selection, and UI presentation.

Code lives in `Assets/Scripts/FMV/` and compiles into the `FMV` assembly (`FMV.asmdef`):

| Folder | Contents |
|---|---|
| `Data/` | Plain data and rules: `FMVScene`, `FMVChoice`, `FMVCondition`, `FMVAutoAdvance`, `FMVChoiceSelector` |
| `Core/` | `FMVManager` (flow and video), `FMVGameState` (playthrough state), `FMVSceneParser` (text to scenes) |
| `UI/` | `FMVUIController` (game screen), `FMVTitleScreen` (title and options) |

Editor-only tooling is in `Assets/Scripts/Editor/` (`FMVSceneSetup`, `FMVSceneLinter`). Tests are in `Assets/Tests/EditMode/`.

## Core Systems

### 1. Scene Data

**FMVScene**
- One scene in the narrative.
- Contains:
  - Unique scene ID, title and description text
  - `IsEnding` flag
  - Choices leading to other scenes (see "Choice selection" below)
  - Auto-advance rules: zero or more `FMVAutoAdvance` entries, each with a target and optional conditions
  - Auto items: items added to the inventory on entry
  - Decision flags: flags set on entry

**FMVChoice**
- One option the player can pick.
- Contains display text, target scene ID, conditions (all must pass for it to be available), and decision flags to set when it is picked.

**FMVAutoAdvance**
- A target scene plus conditions. Used to move on automatically when a scene's video ends instead of showing choices.

**FMVCondition**
- A single requirement, evaluated against `FMVGameState`. Six types:
  - Has item / does not have item
  - Has visited scene / has not visited scene
  - Has made decision / has not made decision

### 2. Game State System

**FMVGameState**
- Singleton for the current playthrough.
- Tracks:
  - **Visited scenes**: `HashSet<string>` of scene IDs
  - **Inventory**: `List<string>` of item IDs
  - **Decisions**: `Dictionary<string, bool>` of decision flags
- Methods: `VisitScene`, `AddItem`, `SetDecision`, `HasVisitedScene`, `HasItem`, `HasMadeDecision`, and `Reset` (starts a new playthrough).

### 3. FMV Manager

**FMVManager**
- Main controller. Lives on a GameObject with a `VideoPlayer`.
- Inspector settings: `TextAssetsPath`, `VideoAssetsPath`, `ImageAssetsPath` (Resources-relative), `InitialSceneId`, `AutoStart`, `PrepareTimeoutSeconds`.
- Responsibilities:
  - Loads and parses the scene file into a dictionary of scenes at startup
  - Tracks the current scene and performs scene transitions
  - Applies a scene's automatic state changes
  - Runs video playback (or a placeholder timer) and decides what happens when it ends
- Events (used by the UI):
  - `OnSceneLoaded(scene)`: a scene was entered
  - `OnVideoPlaybackStarted(texture)`: a clip began playing, with the texture its frames render into
  - `OnChoicePhaseStarted(scene)`: the video phase is over and the player should choose

**Scene flow**
1. `LoadScene(id)`: set the current scene, mark it visited, add its auto items, set its decision flags.
2. Fire `OnSceneLoaded`. The UI clears the text and choices and shows the scene's freeze-frame image.
3. Cancel anything pending from the previous scene, stop the player and unload the old clip. Then play this scene's clip, or run a 3-second placeholder timer if there is no clip (see "Video Playback System").
4. When the video ends (or is skipped after a failure), `HandleVideoFinished` runs:
   1. The first auto-advance whose conditions pass is followed.
   2. Otherwise, if the scene defines any auto-advance, its first target is followed. (This fallback applies even when every auto-advance has conditions and none pass.)
   3. Otherwise the choice phase starts (`OnChoicePhaseStarted`).
5. In the choice phase the UI shows title, description and the selected choices.
6. The player picks a choice. `MakeChoice` sets the choice's decision flags and loads its target. Go to step 1.

An ending scene with no choices shows a **Restart** button, which returns to the title screen (or reloads the initial scene if there is none).

**Choice selection** (`FMVChoiceSelector.SelectForDisplay`)
1. Drop choices whose conditions do not pass.
2. Rank the rest: choices with conditions (including negative ones) first, then unconditional ones, each group in file order.
3. Keep the top 3.
4. Shuffle the display order of those 3.

The cap and the ranking are covered by EditMode tests. `HowToBuildYourOwnScene.md` explains the rule for authors.

### 4. UI System (Unity UI Canvas)

**FMVUIController**
- Renders the game screen with Unity UI (uGUI + TextMeshPro). It only reacts to `FMVManager` events and holds no game logic.
- Elements:
  - **Video Area**: a full-screen `RawImage` with an `FMVAspectFillImage` (crops to cover, never stretches). It shows the freeze-frame image, then the live video, and (see "Video area texture") can hold the last frame.
  - **Text Block** (left): scene title, accent rule and description. Hidden while the video plays, shown in the choice phase.
  - **Choice stack** (right): up to 3 `FMVChoiceButton`s instantiated from `Assets/UI/Prefabs/ChoiceButton.prefab`, in the order returned by the selector. The Restart button on ending scenes uses the same prefab.
  - **Default Fallback Texture** (optional): shown when a scene has no freeze-frame image.
- **`ShowDescription`** (serialized bool, default on; also `SetShowDescription(bool)`): when off, the left block holds only the title and accent rule. The description is also hidden for a scene that has none. Only the left block changes: the choice stack is anchored independently and never moves.

**FMVChoiceButton** (on the choice button prefab)
- Sprite Swap transition: normal, hover, pressed, selected (keyboard or gamepad focus), disabled.
- A chevron at the right end shows only while the button is hovered or selected.
- Height: minimum 84, and the button grows with its label (a Vertical Layout Group sizes it). `Refit` measures the label and caps the label's height at `MaxLines` (2) lines; the label uses TMP auto-size (26 to 32) with Ellipsis overflow, so longer text shrinks first and then truncates. A two-line button is 102 px. The stack is bottom-anchored, so a taller button pushes the ones above it up.

**FMVTitleScreen**
- Title panel with Play and Options. Play calls `FMVManager.StartGame`. `ShowTitleScreen` resets the game state, stops video and clears the current scene.
- Content comes from an `FMVTitleScreenConfig` asset (`Assets/UI/TitleScreenConfig.asset`): title, subtitle, footer, the freeze-frame shown behind the menu and a tint that darkens it (`FreezeFrameTint`, default 0.65 grey). `ApplyConfig` copies them into the UI at start. Change the config, not the scene. The freeze-frame is drawn by an `FMVAspectFillImage` (crops to cover the screen).
- Art and layout come from `Art/UI/README.md` (title screen and choice screen kits). Sprites in `Assets/UI/Sprites/` get their import settings from `TitleSpriteImporter` (9-slice borders by file name). **FMV > Initialize Sample Scene** builds both screens and never overwrites an existing config asset. **FMV > Rebuild Title Screen** and **FMV > Rebuild Choice Screen** bring an older scene up to date without touching the rest of it.

**Choice screen layout** (1920x1080 reference, from `Art/UI/README.md`)
```text
+--------------------------------------------------+
|  video / freeze-frame, full screen, aspect-fill  |
|  choice_scrim (transparent at the top, dark below)|
|                                                  |
|                         +---------------------+  |  stack top y=700
|                         | choice 1            |  |
| TITLE  (x120, w860)     +---------------------+  |
| ---- accent rule        | choice 2          > |  |  16 px gaps, min 84 px tall
| description text        +---------------------+  |
|                         | choice 3            |  |
+--------------------------------------------------+  bottom edge 96 px above the bottom
```
The left block (x120, width 860) and the choice stack (x1060, width 740) are both anchored to the bottom and grow upward.

**UI behavior**
- During video playback: the video area shows the clip; the text block and choices are hidden.
- Choice phase: the title (and description, if enabled) appear, the freeze-frame image is applied (see below), and the choice buttons are shown.

## Data Structure

### Scene Definition Approach - Text Files

Scenes are defined in plain text files under `Resources/FMV/`, so writers can edit them without touching code:
- `Resources/FMV/Scenes.txt` is the small template/sample game.
- `Resources/FMV/ClonedMahon/ClonedMahonScenes.txt` is the real game ("They Cloned Me").

Which file is used is set by `FMVManager.TextAssetsPath`. Editing a scene file and pressing Play picks up the change; no recompilation is needed.

**Example scene**
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

**How it is loaded**
1. `FMVSceneParser.ParseScenes` turns the text into a dictionary of `FMVScene` objects.
2. `FMVManager` keeps that dictionary and serves scenes by ID.
3. If the text asset is missing or yields no scenes, the manager logs an error and does not start. There is no hardcoded fallback.

**Parser notes**
- Conditions in `[requires: ...]` (on choices and on `Auto Advance:` lines) share one parsing routine (`ParseConditions`). Unrecognized conditions are logged as warnings and skipped.
- Scene-level properties are `Title`, `Description`, `Ending`, `Auto Advance`, `Auto Items`, `Decision Flags`. See `HowToBuildYourOwnScene.md` for the full syntax.

### Scene Graph Structure
- **Hub scenes**: central locations with several branching paths
- **Investigation scenes**: linear or branching sequences
- **Ending scenes**: terminal nodes; the count depends on the game

### Condition Evaluation
Conditions are evaluated when they are needed, against the live `FMVGameState`:
- **Choices**: at choice display time, a choice is available only if all of its conditions pass; then selection and ranking apply (see "Choice selection").
- **Auto-advances**: when the video ends, in file order; the first one whose conditions all pass is taken.

## Performance Considerations

### State Management
- `HashSet` for visited scenes (O(1) lookup), `Dictionary` for decisions (O(1) lookup), `List` for inventory (small, O(n) is acceptable).

### Scene Loading
- All scenes are parsed into memory at startup. A few dozen scenes is trivial.
- Videos load one at a time, on demand (see "Resource management").

## Development Workflow

### Adding New Scenes
1. Open the scene file (for example `Assets/Resources/FMV/ClonedMahon/ClonedMahonScenes.txt`) in any text editor.
2. Add a scene in the format above.
3. Run **FMV > Lint Scene Files** and fix any errors.
4. Press Play. No recompilation is needed.

To start a new game from the template, see `UserDocumentation/InitialSetup.md`.

### Scene Format Features
- **Conditional choices**: `[requires: item:key]`
- **Auto items**: `Auto Items: key, photo`
- **Decision flags set on entry**: `Decision Flags: flag1, flag2`
- **Flags set when a choice is picked**: `[sets: made_choice]`
- **Multiple conditions**: `[requires: item:a, item:b, visited:c]`
- **Negative conditions**: `[requires: !item:bomb]`
- **Auto advance** (optionally conditional): `Auto Advance: next_scene [requires: !visited:next_scene]`

See `HowToBuildYourOwnScene.md` for complete documentation.

### Scene Linter

Run **FMV > Lint Scene Files** (`Assets/Scripts/Editor/FMVSceneLinter.cs`) after editing any scene file. It checks every scene file under `Resources/FMV` and reports to the Console. The checks live in a plain `Lint()` function, so they can be called from tests or a build step.

- **Errors**: choice or auto-advance targets that don't exist; choices with no target; `visited:` conditions pointing at unknown scenes; dead-end scenes that aren't endings.
- **Warnings**: missing video or freeze-frame image for a scene (expected in `Videos/` and `Images/` next to the scene file); scenes unreachable from the first-defined scene; `item:` or `decision:` requirements that nothing ever grants or sets; endings that still have choices; scenes with both auto-advance and choices.

Missing videos and images are warnings, not errors, because the game degrades gracefully without them (see "Failure handling").

### Automated Tests

EditMode tests live in `Assets/Tests/EditMode/` (assembly `FMV.Tests.EditMode`, which references the `FMV` assembly).
- `FMVChoiceSelectorTests`: choice ranking, the cap of 3, and that the shuffle happens after the cap.
- `FMVSceneParserTests`: scene properties, all six condition types, `[sets:]` flags, auto-advance, and unrecognized-condition warnings.

Run them from the Test Runner, or with `unity command run_tests --mode EditMode`. The Editor must not be in Play mode.

### Testing Branching
- Run the linter for static graph problems (dangling targets, unreachable scenes, ungranted requirements).
- To play a scene directly, call `FMVManager.Instance.LoadScene("scene_id")` from the Editor (the CLI can do this; see `UserDocumentation/DevEnvironment.md`). Items and flags can be set through `FMVGameState.Instance`.
- Playtesting can be driven from the command line; see `UserDocumentation/DevEnvironment.md`.
- A scene graph visualizer would still be useful (future enhancement).

## Visual Content Pipeline

**Source control for media**: videos and images are stored with Git LFS (see `.gitattributes` and `UserDocumentation/DevEnvironment.md`). Anyone cloning the repo needs Git LFS installed, or the media will be pointer text files.

### Video Playback System

**Two-phase visual flow**
1. **Video phase**: the scene's video plays when the scene is entered (a 3-second placeholder if there is none).
2. **Choice phase**: a freeze-frame image (or the held last frame) stays on screen while the player chooses.

**Video player implementation**
- Unity `VideoPlayer` on the `FMVManager` object, `APIOnly` render mode. The player owns the texture its frames render into; `FMVManager` hands it to the UI through `OnVideoPlaybackStarted`.
- Clips are loaded with `Resources.Load<VideoClip>` from `{VideoAssetsPath}/{sceneId}` (for the real game, `Resources/FMV/ClonedMahon/Videos/`). Any format Unity's `VideoClip` importer supports works; the project uses `.mp4` (H.264).
- Event-driven: `loopPointReached` ends the video phase.
- Clips are prepared (`Prepare()`) and only played once `isPrepared` is true, rather than played immediately.

**Failure handling** (all paths end in the same place as a normal video end: auto-advance, or the choice phase)
- **Missing clip**: no video file for the scene, so a 3-second placeholder timer runs instead (`SimulateVideoPlayback`). This lets a story be playtested before it is filmed.
- **Decode/playback error**: `VideoPlayer.errorReceived` logs an error and treats the video as finished, so a failed decode (e.g. on WebGL) cannot hang the game.
- **Prepare timeout**: if a clip is not prepared within `PrepareTimeoutSeconds` (Inspector field, default 10s), an error is logged and the video is skipped. Each frame's contribution to the timeout is capped at 0.25s, so time spent paused or backgrounded does not count against the budget.
- **Stale callbacks**: `LoadScene` cancels any pending placeholder or prepare routine from the previous scene before starting the new one, so a timer from an earlier visit can never fire for the current scene. `HandleVideoFinished` also ignores any call whose scene is no longer current.

**Video playback state machine**
```
Scene Transition
    ↓
Cancel pending routine, stop player, unload previous clip
    ↓
Try Load Video → Video Found? → YES → Prepare → Prepared in time? → YES → Play Video
    ↓                                              ↓                       ↓
    NO                                             NO (timeout/error)  Video Ends
    ↓                                              ↓                       ↓
3s placeholder timer                       Log error, skip video           ↓
    ↓                                              ↓                       ↓
Video phase over (auto-advance, or choice phase) ← ← ← ← ← ← ← ← ← ← ← ← ←
    ↓
Display Choices
    ↓
Wait for Player Decision
    ↓
Next Scene Transition
```

**Static image system**
- A 2D freeze-frame image per scene, stored as `Resources/FMV/{game}/Images/{sceneId}` (`ImageAssetsPath` + scene ID).
- Loaded with `Resources.Load<Texture2D>` each time it is needed; there is no separate cache.
- Shown in a `RawImage` and used for two things:
  1. Shown immediately on scene load, so the screen is never blank while a clip prepares (and used as the picture for placeholder-timer scenes).
  2. The "freeze frame" during the choice phase.
- If a scene has no image, `DefaultFallbackTexture` is used if set.

**Video area texture**
- `FMVUIController` shows the scene's freeze-frame image as soon as the scene loads. `FMVManager.OnVideoPlaybackStarted` fires once when a clip starts playing, and the controller then swaps in the video's texture. Nothing reassigns it per frame.
- When the clip ends, the choice phase applies the scene's freeze-frame image (or `DefaultFallbackTexture`). If neither exists, the video texture stays on screen, which holds the clip's **last frame**. Keep this in mind if you add a default fallback texture: it would replace the last frame on every video scene that has no image of its own.

**Resource management**
- Videos load on demand per scene; the previous scene's clip is unloaded (`Resources.UnloadAsset`) on each scene change, so memory does not grow over a long playthrough.
- The `VideoPlayer` component is reused for all scenes.

## Scalability Considerations

- **Scene count**: the architecture comfortably supports 50+ scenes.
- **Choice complexity**: the condition system is extensible (add a `ConditionType`, its parser prefix and its `Evaluate` case).
- **State size**: current state tracking is minimal and can expand as needed.

## Future Technical Enhancements
- Save/Load serialization of `FMVGameState`
- Scene flowchart editor/visualizer
- Video preloading/streaming
- Localization support for choice text
- Analytics/telemetry for player choices
