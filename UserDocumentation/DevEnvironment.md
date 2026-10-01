# Development Environment

How this project is set up for driving the Unity Editor from the command line (for scripted playtesting, compile checks and AI-assisted work), and the things that will trip you up. For creating a new game from the template, see `InitialSetup.md`. For the game's architecture, see `Assets/Documents/TechnicalDesignDocument.md`.

## Unity version

Unity 6.4 (`6000.4.2f1`), URP 2D. Targets Windows and WebGL.

## Git LFS for media

Video and image files are stored with Git LFS so re-encoded clips don't bloat the repository. `.gitattributes` routes `*.mp4`, `*.mov`, `*.webm`, `*.avi`, `*.png`, `*.jpg` and `*.jpeg` through LFS.

- **Install Git LFS before cloning** (`git lfs install` once per machine). Without it you get small text pointer files instead of the real videos and images, and Unity will show broken or empty assets.
- **Existing history was not rewritten.** Media committed before LFS was enabled stays in earlier commits as ordinary blobs, so `.git` is still around 120 MB. Only the current files and anything new go through LFS.
- **Quota.** GitHub Free includes a limited LFS allowance (storage and monthly bandwidth; check github.com/settings/billing for the current figures). Each re-encoded clip adds a full new copy to storage, and each clone or CI download counts against bandwidth. At the current ~120 MB of media this is far from the limit. If the limit is hit, GitHub blocks LFS pushes and downloads until the next month or until a data pack is bought.
- **When adding a new media type** (for example audio), add it with `git lfs track "*.ext"` and commit `.gitattributes` first, so the files are stored correctly from their first commit.

## Driving the Editor with the Unity CLI

The Unity CLI talks to a running Editor through the `com.unity.pipeline` package. It can enter and exit Play mode, run C#, read the Console, recompile, run tests and capture the Game view, without touching the Editor window.

Both are beta/experimental (CLI `1.0.0-beta.12`, `com.unity.pipeline` `0.8.0-exp.1`), so expect rough edges.

### One-time setup

1. Install the CLI (Windows PowerShell):
   ```powershell
   $env:UNITY_CLI_CHANNEL='beta'; irm https://public-cdn.cloud.unity3d.com/hub/prod/cli/install.ps1 | iex
   ```
   The binary lands at `%LOCALAPPDATA%\Unity\bin\unity.exe`. It is only on `PATH` in shells opened after the install, so open a new terminal. If `unity` still isn't found, add that folder to your user `PATH` permanently (Settings > System > About > Advanced system settings > Environment Variables), or for one PowerShell session: `$env:Path += ";$env:LOCALAPPDATA\Unity\bin"`.
2. Add the package to the project (Unity can stay open):
   ```powershell
   unity pipeline install
   ```
   This adds `com.unity.pipeline` to `Packages/manifest.json` and `packages-lock.json`. Commit both.
3. Wait for Unity to finish importing, then check the connection:
   ```powershell
   unity status
   ```
   You want `state: ready`. The port is not fixed (it was 7800, then 7801 after a script reload), and the CLI finds it itself.

### Everyday commands

Run these from inside the project folder, or pass `--project-path`.

```powershell
unity status                      # is an Editor connected?
unity command                     # list every command the Editor exposes
unity command editor_play         # enter Play mode
unity command editor_stop         # exit Play mode
unity command recompile           # recompile scripts (the CLI reconnects after the reload)
unity command console             # read Console output; add --level warn|error to filter
unity command clear_console
unity command eval_file <path>    # run C# from a .cs file in the live Editor
unity command screenshot          # capture the Game view to Temp/pipeline-screenshots
unity pipeline list               # per-project server state, including Safe Mode
```

Tool-driven (AI agent) calls should also pass `--caller plugin --skill <skill-name>`; the flags only label the call.

### Tips

- **Use `eval_file`, not `eval`, from PowerShell.** Windows PowerShell 5.1 strips the double quotes out of arguments passed to native programs, which breaks C# string literals in `unity command eval '...'`. Put the C# in a `.cs` file instead. The file is a statement body: end it with `return <value>;` to get a result back.
- **Clicking UI in a test.** Invoke the button's `onClick` from C# (for example find the `Button` by its label text and call `onClick.Invoke()`). It is more reliable than simulating pointer coordinates.
- **Jumping to a scene.** `FMV.Core.FMVManager.Instance.LoadScene("scene_id")` starts any scene directly, which is how scenes that need an inventory item (such as `a2_hallway_2`) can be reached without playing through.
- **Compile errors put the Editor in Safe Mode.** In Safe Mode the pipeline package does not load and the CLI cannot connect. `unity pipeline list` shows this. Fix the compile errors and restart Unity.

## Gotchas

### Unity must be the foreground window for video to play

While the Editor is not the foreground window, the game loop barely ticks and the VideoPlayer will not advance, even if you call `Play()` yourself. Clips prepare but never start. Scenes with no video still work, since they use a timer. For any test that plays real clips, click the Unity window and keep it in front.

`Application.runInBackground = true` (set from an `eval_file` script after entering Play mode) lets the game loop run in the background. It is not enough on its own to make video play, and it resets each Play session. We have not tried the persistent Player setting (Project Settings > Player > Run In Background).

### The Game view screenshot has come back blank

`unity command screenshot` has returned a flat blue frame in every test so far, even while video was playing. Do not rely on it to check layout or video content until the cause is found (the capture `source` option, `screen` versus `camera`, is the first thing to try).

### Enter Play Mode settings

The project uses Unity's default: Enter Play Mode Options are off, so both the domain and the scene reload when you press Play. We briefly turned reloads off while working around the MCP Unity package (see below), then went back. The CLI keeps its connection through Play either way, so there is no reason to disable reloads for it. If you turn them off later for iteration speed, static state is not reset on Play; `FMVManager.Start` calls `FMVGameState.Instance.Reset()`, so playthrough state is still cleared, but keep it in mind for any new static or singleton state.

### AI package warnings

`com.unity.ai.assistant` needs a Unity AI subscription. Without one it logs repeated "NoSubscription" errors from its model selector. They are unrelated to the game and the CLI does not depend on that package, so it can be removed from the Package Manager if the noise is a problem.

## Why there is no MCP Unity package

We first tried the `com.gamelovers.mcp-unity` package (a WebSocket MCP server on port 8090) and removed it. When Play mode starts it stops its server and tries to restart it. With domain reload disabled the old listener kept port 8090 for longer than the package's 10 retries, so it logged "Port 8090 is already in use" and the connection stayed down until the Editor was restarted. A git dependency without a pinned commit also moved its package-cache folder name, which broke the hard-coded path in `.mcp.json`. The Unity CLI does not use that port and kept its connection through Play mode and script reloads.

If you ever bring it back, pin it to a commit (`...git#<hash>` inside the quotes of the manifest entry) and expect to restart its server after Play.
