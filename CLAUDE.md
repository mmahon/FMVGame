# FMV Game ("They Cloned Me")

Data-driven FMV (full-motion-video) branching game in Unity 6.4 (`6000.4.2f1`, URP 2D). Scenes, choices, items and flags live in plain-text scene files parsed at runtime; videos and freeze-frame images load from `Resources/`. Code is in `Assets/Scripts/FMV/` (`Core`, `Data`, `UI`).

## Read these first
- Architecture: `Assets/Documents/TechnicalDesignDocument.md`
- Writing scenes: `Assets/Documents/HowToBuildYourOwnScene.md`
- Editor tooling, CLI, gotchas: `UserDocumentation/DevEnvironment.md`

## Working rules
- **The user handles version control.** Do not commit, push or open PRs unless asked. Say so when you change tracked files like `Packages/manifest.json` or `packages-lock.json`.
- After editing any scene file under `Assets/Resources/FMV/`, run **FMV > Lint Scene Files** and report errors. Missing videos and images are warnings, not errors.
- Media (`*.mp4`, `*.png`, `*.jpg` etc.) is stored with Git LFS. Add new media types to `.gitattributes` first.

## Driving Unity
Use the Unity CLI, not MCP Unity (it was removed; do not try to reconnect it).
- `unity` should be on `PATH` (setup in `UserDocumentation/DevEnvironment.md`). If a shell can't find it, the binary is at `%LOCALAPPDATA%\Unity\bin\unity.exe`; in PowerShell: `$env:Path += ";$env:LOCALAPPDATA\Unity\bin"`.
- Pass `--caller plugin --skill unity-cli` on every `unity command` call.
- Use `unity command eval_file <path.cs>` for C#, not `eval`: PowerShell 5.1 strips double quotes from arguments. The file is a statement body; end with `return <value>;`.
- **Unity must be the foreground window** for video to play. In the background the game loop stalls and clips never start. Ask the user to bring Unity to the front for any test with real clips.
- Compile errors put the Editor in Safe Mode, where the CLI cannot connect. Check `unity pipeline list`.
- `screenshot` has returned a blank blue frame so far; do not treat it as evidence of how the screen looks.

## Known open items
- Choice selection: valid conditional choices rank first, then unconditional ones, each in file order; the top 3 are kept and then shuffled (implemented in `FMVChoiceSelector`, documented in `HowToBuildYourOwnScene.md`, tested in `Assets/Tests/EditMode`). Run tests with `unity command run_tests --mode EditMode`.
- Several scenes have no video or freeze-frame yet; `a2_end_win` is unreachable from the start scene.
