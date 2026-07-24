 # How to Build Your Own Scene

This guide explains how to create, format, and add new scenes to the FMV Game Engine, as well as where to place the associated assets.

## 1. How to Format `Scenes.txt`

The FMV Game Engine uses a simple text file (`Scenes.txt`) to define all scenes, properties, choices, and conditions.

A typical scene block looks like this:

```text
===== SCENE: my_scene_id =====
Title: My Scene Title
Description: This is the text description that appears after the video plays.
Ending: no
Auto Advance: next_scene_id
Auto Items: key_item, sword
Decision Flags: met_npc

CHOICES:
- Go to the forest -> forest_scene
[requires: item:sword]
[sets: visited_forest]
- Talk to the NPC -> npc_scene
[requires: !decision:met_npc]
```

### Formatting Rules:
*   **Scene Header:** Every scene MUST start with `===== SCENE: [SceneID] =====`.
*   **Properties:** Key-value pairs separated by a colon (`:`). They must appear before `CHOICES:`.
*   **Choices Header:** The choices section MUST begin with the exact text `CHOICES:`.
*   **Choice Syntax:** Each choice starts with a hyphen (`-`), followed by the text displayed on the button, an arrow (`->` or `→`), and the target SceneID. Example: `- Let's go -> destination_id`
*   **Ending Scenes:** If a scene has `Ending: yes` and no choices are available, the game will present a "Restart" button that returns to the initial scene.

## 2. Supported Options

### Scene Properties (Parsed per Scene)
*   **Title:** (`Title: text`) - The header text shown during the choice phase.
*   **Description:** (`Description: text`) - The narrative text shown during the choice phase.
*   **Ending:** (`Ending: yes/true`) - Marks the scene as an ending. If no choices exist, a "Restart" button is spawned.
*   **Auto Advance:** (`Auto Advance: target_scene_id`) - Automatically loads the target scene after the video/image playback finishes, bypassing the choice phase.
*   **Auto Items:** (`Auto Items: item1, item2`) - A comma-separated list of items given to the player immediately upon entering the scene.
*   **Decision Flags:** (`Decision Flags: flag1, flag2`) - A comma-separated list of flags set to true immediately upon entering the scene.

### Choice Conditions & Flags
Modifiers can be attached to specific choices. They must be placed directly beneath the choice they modify.

**`[requires: ...]`**
Sets conditions that must be met for the choice to appear. Multiple conditions can be separated by commas.
*   `item:[itemName]` - Player must have the item.
*   `!item:[itemName]` - Player must NOT have the item.
*   `visited:[sceneId]` - Player must have visited the specified scene.
*   `!visited:[sceneId]` - Player must NOT have visited the specified scene.
*   `decision:[flagName]` - Player must have triggered the specified decision flag.
*   `!decision:[flagName]` - Player must NOT have triggered the specified decision flag.

**`[sets: ...]`**
*   `[sets: flag1, flag2]` - Sets one or more decision flags when the player clicks this choice.

## 3. Where to Place Assets

The engine uses Unity's `Resources` folder to dynamically load content based on the `SceneID`.

*   **Scenes File:**
    *   Path: `Assets/Resources/FMV/Scenes.txt`
    *   *Note: This path can be changed on the FMVManager component (`Text Assets Path`).*
*   **Videos:**
    *   Path: `Assets/Resources/FMV/[GameName]/Videos/[SceneID].mp4` (or `Assets/Resources/FMV/Videos/[SceneID].mp4` for the default game)
    *   *Note: Place video clips here. If a video is found matching the Scene ID, it will play. The folder name can be changed via `Video Assets Path` on FMVManager.*
*   **Static Images / Fallbacks:**
    *   Path: `Assets/Resources/FMV/[GameName]/Images/[SceneID].png` (or `Assets/Resources/FMV/Images/[SceneID].png` for the default game)
    *   *Note: An image matching the Scene ID is shown during video playback (acting as the video if no video exists or while it loads). The folder name can be changed via `Image Assets Path` on FMVManager.*

## 4. How to Add a Scene to Unity

1.  **Define the Text Data:** Open your `Scenes.txt` file and add a new `===== SCENE: your_scene_id =====` block using the formatting above.
2.  **Add Visuals:** Drop your video file (`your_scene_id.mp4`) into the `Videos/` directory OR drop a static image (`your_scene_id.png`) into the `Images/` directory for your game.
3.  **Link to it:** Ensure another scene points to your new scene via a Choice (`- Go there -> your_scene_id`) or an Auto Advance (`Auto Advance: your_scene_id`). Alternatively, set it as the `Initial Scene Id` on the `FMVManager` in your active Unity scene to test it immediately.
4.  **Play:** Press Play in Unity! The `FMVManager` dynamically parses the text file on start, so no extra code or Unity scene creation is required.
