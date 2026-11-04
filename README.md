# They Cloned Me

A branching full-motion-video (FMV) thriller built in Unity, set around a guarded facility. Each scene plays a short clip, holds on its last frame, and offers up to three choices. Items you pick up and decisions you make unlock or close off later options.

The game is also a small **data-driven FMV engine**: the whole story lives in plain-text scene files, so scenes, choices, items and flags can be written and changed without touching C#.

![Title screen design](Art/UI/title_screen_mock.png)
![Choice screen design](Art/UI/choice_mock_with_description.png)

*These are the UI design targets the in-game screens were built to match. In-game captures will replace them.*

## What it shows

- **Data-driven scenes.** A text format with choices, `[requires: item:key, !visited:scene]` conditions, `[sets: flag]` decisions, auto-advance and auto-items, parsed at runtime (`FMVSceneParser`).
- **Choice selection rule.** Valid choices that depend on game state rank first, then the rest in file order; the top three are kept and shuffled (`FMVChoiceSelector`).
- **Resilient video playback.** Clips are prepared before playing; a missing clip falls back to a placeholder timer, and a decode error or prepare timeout skips the video instead of hanging the game.
- **Scene linter.** **FMV > Lint Scene Files** checks dangling targets, dead ends, unreachable scenes and requirements nothing can satisfy.
- **Tests.** EditMode tests for the choice rules and the scene parser.
- **UI built from a spec.** Title and choice screens are laid out from a written UI kit (`Art/UI/README.md`) with sprites imported by script, a 9-sliced choice button that grows with its label, and a toggleable scene description.

## Requirements

- **Unity 6.4 (`6000.4.2f1`)**, Universal Render Pipeline (2D). Open it with Unity Hub.
- **[Git LFS](https://git-lfs.com/)**, installed *before* you clone. Videos and images are stored with LFS (about 120 MB). Without it you get small text pointer files instead of the media, and Unity shows broken assets.

## Run it

```bash
git lfs install
git clone https://github.com/mmahon/FMVGame.git
```

1. Add the project folder in Unity Hub and open it with Unity `6000.4.2f1`. The first import takes a few minutes.
2. Open `Assets/Scenes/CloneMahon.unity`.
3. Press **Play**, then **PLAY** on the title screen.

Some scenes have no video or freeze-frame yet. Those show a short placeholder instead, so the story is still playable end to end.

## Playable build

<!-- Add the link here once a build is published, e.g. a GitHub Releases zip (WebGL or Windows). -->
A prebuilt version is not published yet. Until then, run it from the Unity Editor as above.

To make your own build, use **File > Build Profiles**. The project targets Windows and WebGL, and `CloneMahon` is the scene in the build list.

## Tests and tools

| What | How |
|---|---|
| EditMode tests | **Window > General > Test Runner > EditMode**, or `unity command run_tests --mode EditMode` with the [Unity CLI](UserDocumentation/DevEnvironment.md) |
| Lint the scene files | **FMV > Lint Scene Files** (run after editing `Assets/Resources/FMV/**`) |
| Rebuild the UI in a scene | **FMV > Initialize Sample Scene**, **FMV > Rebuild Title Screen**, **FMV > Rebuild Choice Screen** |

## Project layout

```
Assets/
  Scripts/FMV/        Core (manager, state, parser), Data (scenes, choices, conditions), UI
  Scripts/Editor/     Scene linter, UI scene builder, sprite importer
  Tests/EditMode/     Choice selector and parser tests
  Resources/FMV/      Scene text files, videos and freeze-frame images (Git LFS)
  UI/                 Sprites, prefabs and the title-screen config
  Documents/          Design documents
Art/UI/               UI kit spec, SVG sources and design mocks
UserDocumentation/    Dev environment notes and new-game setup
```

## Documentation

- [Technical Design Document](Assets/Documents/TechnicalDesignDocument.md): architecture and systems
- [Game Design Document](Assets/Documents/GameDesignDocument.md)
- [Writing your own scenes](Assets/Documents/HowToBuildYourOwnScene.md)
- [Dev environment and Unity CLI notes](UserDocumentation/DevEnvironment.md)

## License

The **source code** is released under the [MIT License](LICENSE). You are free to reuse it.

**The game's media is not covered by that license.** This includes the AI-generated video and audio, the freeze-frame images, and the UI art (`Assets/Resources/FMV`, `Assets/UI`, `Art/`). All rights to it are reserved. Please do not reuse or redistribute it without permission.

Third-party material keeps its own terms:
- **Archivo** and **Archivo Condensed** fonts (`Assets/Fonts`) are under the [SIL Open Font License 1.1](Assets/Fonts/OFL.txt).
- Unity and its packages are under Unity's own terms.
