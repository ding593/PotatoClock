**English** | [简体中文](README.md)

# PotatoClock

A lightweight pomodoro timer for Windows: a borderless always-on-top mini window with a tray icon, a task list, a bilingual UI (Chinese / English) and switchable color themes. The interface is hand-drawn following Apple's Human Interface Guidelines (corner radii, hierarchy, motion curves) on WinForms / .NET 10.

| Dark | Light (English) |
| --- | --- |
| <img src="https://github.com/user-attachments/assets/c3acf5a2-431e-4644-a0ea-d16641499bd6" width="320" alt="Dark theme"> | <img src="https://github.com/user-attachments/assets/785c4d9f-d2ed-4e5d-a99e-ef70a3cdb913" width="308" alt="Light theme"> |

| Collapsed window | Settings |
| --- | --- |
| <img src="https://github.com/user-attachments/assets/f0797ef3-9ddd-44aa-a4d3-bad376005eca" width="318" alt="Collapsed window"> | <img src="https://github.com/user-attachments/assets/fabe6f1b-6ee2-4c69-927d-744abdbc60ff" width="537" alt="Settings"> |

## Features

- **Timer**: Focus / Short Break / Long Break cycle. Defaults are 25 / 5 / 15 minutes, with a long break after every 4 focus sessions. The next phase does **not** auto-start — you press Start. When a phase switches, the countdown flashes softly in the accent color.
- **Round dots**: a row of dots shows progress within the current set — completed focus sessions are filled with the accent color, the current one is an accent ring, the rest are hollow.
- **Tray**: clicking ✕ or Alt+F4 minimizes to the tray (with a one-time balloon hint). The tray menu offers Show/Hide, Start/Pause, Skip, Settings and Quit; double-clicking the tray icon toggles the window.
- **Alerts**: when a phase ends naturally the app plays a system sound and shows a balloon notification (skipped phases stay silent).
- **Task list**: add tasks in the rounded input (Enter or the + button), click the circle to mark one done (the title dims and gains a strikethrough), hover the end of a row to reveal its delete button. Every change is saved immediately.
- **Bilingual UI**: follows the system language by default (Chinese on Chinese systems, English otherwise). Switch between System / 中文 / English in Settings — it applies instantly, including the tray menu and notifications.
- **Themes**: six presets (Dark / Light / Warm / Mint / Violet / Contrast, taken from Apple's system colors) plus Custom, where you pick background, text and accent colors and the remaining levels are derived automatically.
- **Robustness**: corrupted data files fall back to defaults; an unwritable folder shows a red warning instead of failing silently; only a single instance runs (it also recovers if the previous one was force-killed); unhandled exceptions are written to `error.log` instead of popping a crash dialog.

## Interface design

- **Window**: 340×246 (340×522 with the task panel expanded), borderless, 12 px rounded corners plus a hairline border. Drag anywhere on the header; always on top.
- **Hierarchy**: window background → card surface → three text levels (primary 100% / secondary 60% / tertiary 30%) → a single accent color → hairline separators. Dark cards use `#1C1C1E` / `#2C2C2E`; never pure black for surfaces.
- **Corner radii**: 12 for the window, 10 for cards, 8 for controls, 6 for list rows (Apple's "inner radius = outer radius − padding").
- **Type**: Segoe UI Variable on Windows 11, otherwise Segoe UI / Segoe UI Semibold, with Microsoft YaHei UI as the CJK fallback through system font linking. Only the Regular and Semibold weights are used. The countdown is drawn in fixed digit slots, so it never jitters.
- **Motion**: expand/collapse 400 ms `cubic-bezier(0.32, 0.72, 0, 1)`, hover 170 ms, switches and checkmarks 200 ms, progress bar 200 ms, settings window fade-in 400 ms. Every animation is interruptible and continues from its current value.
- **Icons**: all hand-drawn GDI+ line icons (SF Symbols style) — no icon font dependency.

## Shortcuts

| Key | Action |
| --- | --- |
| Space | Start / pause (unless the task input has focus) |
| Ctrl+N | Expand the task panel and focus the input |
| Esc | Collapse the task panel (cancel in Settings) |
| Enter | In the task input, add the task; in Settings, confirm |

## Requirements

- Windows 10 / 11
- Build: .NET 10 SDK
- Run: the framework-dependent build needs the .NET 10 Desktop Runtime (`Microsoft.WindowsDesktop.App`); the self-contained build needs nothing

## Build and run

```powershell
dotnet build PotatoClock.csproj -c Release
dotnet run --project PotatoClock.csproj            # runs the Debug build

# run the Release build
.\bin\Release\net10.0-windows\PotatoClock.exe
```

## Data files

Everything is stored **next to the executable** — nothing is written into your C: user profile:

| File | Contents |
| --- | --- |
| `settings.json` | durations, alerts, window, language and theme settings; created on first run, safe to edit by hand |
| `tasks.json` | the task list and completion state |
| `error.log` | only created when an unhandled exception occurs; contains the full stack trace |

Writes go to a `.tmp` file first and then replace the original. If reading fails (corrupted or hand-edited file) the app falls back to defaults. If the folder is not writable (for example when installed under `Program Files`), Settings shows a red "this folder is not writable, changes won't be saved" warning; the app still works and never silently writes elsewhere — **so install it into a user folder** (e.g. `%LOCALAPPDATA%\Programs\PotatoClock`), not `Program Files`.

Theme names and the settings schema are backward compatible: older values such as `深色/浅色/暖橙/青绿/紫罗兰/高对比/自定义` are mapped to `dark/light/warm/mint/violet/contrast/custom`.

## Command-line options

```powershell
PotatoClock.exe --help                        # show usage
PotatoClock.exe --test-seconds=3              # dev: compress every phase to 3 seconds
PotatoClock.exe --data-dir=D:\tmp\pclock      # override the data folder
PotatoClock.exe --log=D:\tmp\phase.log        # dev: append every phase transition to a log
PotatoClock.exe --language=en                 # override the UI language (zh / en)
```

`--test-seconds` together with `--log` makes the phase logic verifiable automatically: every phase completion appends a line
`time / completed=<phase> / skipped=<bool> / focusCount=<n> / next=<phase> / nextDurationSeconds=<n>`.

## Packaging and releases

```powershell
# A. Self-contained single file (the target machine needs nothing installed; larger)
dotnet publish PotatoClock.csproj -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true `
  -p:PublishTrimmed=false -o dist\PotatoClock-win-x64

# B. Framework-dependent (small; requires the .NET 10 Desktop Runtime)
dotnet publish PotatoClock.csproj -c Release -o dist\PotatoClock
```

Notes:

- Keep `PublishTrimmed=false` — **WinForms does not support trimming**.
- The first self-contained publish downloads the win-x64 runtime pack.
- A distribution folder must **not** contain `*.pdb`, `settings.json`, `tasks.json` or `error.log`.
- For a real installer (Next button, Start Menu entry, uninstaller) use [Inno Setup](https://jrsoftware.org/isinfo.php) with `DefaultDirName={localappdata}\Programs\PotatoClock` and `PrivilegesRequired=lowest` (no UAC, and the folder stays writable).
- To publish on GitHub: build the package locally, zip it, then go to **Releases → Draft a new release**, tag it `v1.0.0` and attach the zip.

Suggested `.gitignore`:

```gitignore
bin/
obj/
dist/
.vs/
*.user
settings.json
tasks.json
error.log
*.pdb
```

## Project layout

```
App/                   settings, themes, localization, task model, storage
  AppSettings.cs       persistable settings (including language) with range validation
  AppTheme.cs          theme hierarchy, six Apple system-color presets, custom derivation
  AppStore.cs          settings.json / tasks.json IO (atomic writes, fault tolerant)
  Localization.cs      Chinese/English strings and system-language detection
  ColorHex.cs          #AARRGGBB / #RRGGBB parsing
  TaskItem.cs          task model
Controls/              all hand-drawn controls
  ThemedControl.cs     base class (double buffering, theme, rounded-path helpers)
  FlatButton.cs        rounded button (Primary / Secondary / Ghost / Icon, hover + press animation)
  ProgressBar.cs       thin rounded progress bar with eased value changes
  TimeDisplay.cs       countdown drawn in fixed digit slots (flashes on phase change)
  PhaseDots.cs         round-progress dot indicator
  SurfacePanel.cs      card surface
  ToggleSwitch.cs      Apple-style switch
  StepperField.cs      − / + stepper
  SegmentedControl.cs  segmented control with a sliding selection pill
  ThemeSwatchRow.cs    theme swatch picker
  ColorWell.cs         custom-color well
  TaskRow.cs           task row (circle check, strikethrough, hover-revealed delete)
  RoundedField.cs      rounded input container (hosts a native TextBox so the IME works)
  LogoMark.cs          in-window logo
Core/                  the timer state machine, fully decoupled from the UI
  PomodoroTimer.cs     settles against a UtcNow deadline, so sleep/resume loses nothing
  PomodoroPhase.cs     phase enum
  PhaseCompletedEventArgs.cs  phase-completion event args
Design/                design tokens and infrastructure
  Motion.cs            radii, spacing and duration scales, easing curves, shared animator
  Icons.cs             hand-drawn SF Symbols-style glyphs
  Typography.cs        font resolution and caching
  Shapes.cs            rounded-rectangle path
  WindowChrome.cs      borderless window: rounded region, hairline, shadow, dragging
Forms/                 main window and settings window
Assets/                app.ico (exe + tray icon), logo.png (in-window mark)
Program.cs             entry point, single instance, language init, crash logging
CommandLineOptions.cs  command-line parsing
```

## Known behavior and limitations

- The close button minimizes to the tray by default; you can turn that off in Settings (then ✕ quits directly).
- Closing the window from outside (Task Manager, taskbar right-click → Close) ends the process directly; tasks and settings are already saved by then, so nothing is lost.
- When the task list overflows the panel you get the native scrollbar (it is not custom-drawn).
- Windows 10 has no Segoe UI Variable and falls back to Segoe UI; on Windows 11 the optical-size variants are used, which look closer to SF Pro.
- Deliberately out of scope: auto-starting the next phase, daily/history pomodoro statistics, per-task statistics.
- If the tray icon lingers after a force kill, hovering it or restarting Explorer clears it.

## License

MIT
