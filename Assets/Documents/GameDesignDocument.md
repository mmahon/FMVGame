---
description: Overall vision, design philosophy, core experience, mechanics, and structure for the FMV game.
applicability: Use throughout development to guide design decisions and understand how different elements of the game fit together.
---

# Game Design Document - FMV Investigation Game

## Core Vision
A full-motion video (FMV) game in the style of The Spectre Files: Deathstalker, featuring choice-driven narrative branching where player decisions and discoveries shape the available options and ultimate outcome.

**This system is designed as a reusable template** - the entire story can be edited by modifying a simple text file without any coding required.

## Genre & Style
- **Genre**: FMV Interactive Drama / Investigation
- **Style**: Choice-driven narrative with conditional branching
- **Reference**: The Spectre Files: Deathstalker

## Core Experience
Players navigate through video scenes, making meaningful choices at branch points. The game tracks their journey, collected evidence, and key decisions, which dynamically affect what choices become available later.

## Game Structure

### Scene Architecture
- **Total Scenes**: ~30 scenes   (TBD final amount)
- **Structure**: Hub-based flow with variety in the middle sections
- **Endings**: 3 distinct endings that the narrative collapses toward
- **Navigation**: Players can revisit locations (hub structure) based on available choices

### Branching Model
- **Choices per Branch**: A maximum of 3 choices are presented at each decision point.
- **Top Three Selection**: If a scene has more than 3 valid choices, the engine keeps the 3 highest-priority ones and shuffles them for display. Priority is: valid choices with conditions first (so a choice unlocked by an item, visit or decision always appears when it is valid), then unconditional choices, each group in the order written in the story text file.
- **Conditional Choices**: Available choices change based on:
  - Previously visited scenes
  - Collected inventory items/evidence
  - Key decisions made at critical story points

## Core Mechanics

### Choice System
- At each branch point, present up to 3 choices.
- If more than 3 choices are eligible (valid/available) after condition evaluation, the system keeps the 3 highest-priority eligible choices (conditional choices first, then file order) and shuffles their display order.
- Choices may have conditions that must be met to appear
- Conditions can check: visited scenes, inventory items, previous decisions

### State Tracking
The game maintains persistent state throughout the playthrough:

1. **Scene Visit Tracking**
   - Records which scenes the player has visited
   - Enables location-based conditional choices
   - Prevents/allows revisiting based on story logic

2. **Light Inventory System**
   - Players collect items/evidence throughout scenes
   - Inventory items can unlock new dialogue/choice options
   - Focus on narrative-relevant items rather than complex inventory management

3. **Decision Tracking**
   - Key story choices are flagged and remembered
   - Major decisions influence available paths later
   - Creates branching narrative consequences

### Progression
- **No Save System**: Single-session playthroughs (may be revisited later)
- **Replayability**: Multiple paths and endings encourage replays
- **State Persistence**: All choices and discoveries persist within a single playthrough

## Player Experience Flow

### Typical Loop (Video + Choice Flow)
1. **Cinematic Phase**: Watch video scene (5-15 seconds)
   - No choices shown during video
   - Scene title hidden for immersion
   - Full cinematic experience
2. **Choice Phase**: Video ends, freezes on static image
   - Scene title appears overlaid
   - Scene description visible (can be switched off with the `ShowDescription` setting)
   - Three choices appear (conditionally based on state)
3. **Decision**: Player evaluates and selects choice
4. **Transition**: Selected choice leads to next scene
5. **Repeat**: New scene starts with its video
6. **Ending**: Reach one of 7 endings, option to restart

### Fallback Behavior
- If no video exists for a scene, static image shows immediately
- Player doesn't notice missing videos (graceful degradation)
- System always shows choices after video or immediately with static image

### Hub Structure
- Central location(s) where multiple paths branch out
- Players can return to hub to explore different paths
- Unlocked choices appear as investigation progresses

## Content Design

### Visual Style
- Photoreal, cinematic imagery of a secure corporate research facility (the fictional Arkos Research Initiative)
- Cold and controlled: brushed steel, glass, concrete, hard linear lighting, long sight-lines
- Tension comes from being watched (cameras, glass walls, guards) rather than from gothic or noir shadow
- Desaturated cool palette with selective warm and red accents
- Flat, clean, corporate UI (no glow or holographic effects) that sits over the imagery
- Full details are in `ArtDesignDocument.md`; UI layouts and sprites are in `Art/UI/README.md`

### Current Implementation
- Each scene can have a pre-rendered video clip and a freeze-frame image. The clip plays first; the freeze-frame is the full-screen backdrop for the choice phase
- Scenes missing a clip or image fall back gracefully (see Fallback Behavior); some scenes still have neither
- The title screen and choice screen use the corporate UI kit, with title, subtitle and footer text driven by data
- The scene description can be switched off (`ShowDescription`), which is useful while videos are still missing
- Choice buttons grow to two lines when a label wraps

### Future Evolution
- Fill in the remaining clips and freeze-frames
- Maintain same narrative structure and branching
- Freeze-frames should match the final frame of their clip, so the choice phase continues the video without a visible jump

## Design Pillars
1. **Meaningful Choices**: Every decision should feel consequential
2. **Conditional Branching**: State-driven choice availability creates dynamic storytelling
3. **Investigation Focus**: Discovering items and information unlocks new paths
4. **Replayability**: Multiple endings and hidden paths reward exploration
5. **Accessibility**: Story editing requires no coding - just edit a text file

## Template System

This game is built on a **reusable FMV template** that allows anyone to create their own interactive video game:

### What Makes It Template-Ready
- **No Coding Required**: Edit story in `Scenes.txt` markdown file
- **Simple Format**: Easy-to-understand text structure
- **Full Feature Set**: Conditions, items, decisions, branching
- **Video Support**: Drop in MP4 files and they play automatically
- **Image Fallbacks**: Static images work if videos don't exist
- **Instant Testing**: Edit text → Press Play → See changes

### Using This as a Template
1. Open `Assets/Resources/FMV/Scenes.txt`
2. Replace scenes with your own story
3. Add your images/videos
4. Test and iterate

See `TEMPLATE_GUIDE.md` for complete instructions.

## Future Considerations
- Save/Load system (deferred)
- More complex inventory interactions
- Additional ending variations
- Timeline/flowchart visualization for players
- Scene editor GUI tool
