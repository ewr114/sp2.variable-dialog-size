[简体中文](doc/README.zh-CN.md)

# SP2 Variable Dialog Size

`SP2 Variable Dialog Size` is a local UI plugin for the Windows x64 edition of `SimplePlanes 2`.
The current version is `1.0.0`. It is loaded with the BepInEx 5 Mono x64 runtime and is built
against game version `0.7.10.102`.

The plugin only resizes two designer dialogs — *Variable Setters* and *Variable Outputs* — and
remembers the size you pick. It does not modify crafts, physics or network messages, and it does
not depend on other custom plugins.

## Direct installation (BepInEx included)

The `release` directory contains a ready-to-install archive:

[`release/SP2VariableDialogSize-v1.0.0-win-x64.zip`](release/SP2VariableDialogSize-v1.0.0-win-x64.zip)

The archive includes the complete BepInEx `5.4.23.5` Windows x64 runtime together with the plugin,
so **you do not need to install BepInEx separately**.

1. Close `SimplePlanes 2` completely.
2. Open the archive and extract **everything** into the game directory — the folder that contains
   `SimplePlanes 2.exe`.
3. If Windows asks whether to merge the `BepInEx` folder, allow it.
4. Start the game, enter the designer and open a variable dialog.

The installed files should look like this:

```text
SimplePlanes 2/
├─ .doorstop_version
├─ doorstop_config.ini
├─ winhttp.dll
├─ BepInEx/
│  ├─ core/
│  └─ plugins/
│     └─ SP2VariableDialogSize.dll
└─ SimplePlanes 2.exe
```

The archive does not contain `BepInEx/config`, logs, caches or other plugins, so extracting it over
an existing setup will not reset your settings. The SHA-256 of the archive is recorded in
[`release/SHA256SUMS.txt`](release/SHA256SUMS.txt).
> If you already use BepInEx 5 for other SimplePlanes 2 mods, prefer copying only
> `BepInEx/plugins/SP2VariableDialogSize.dll` from the archive instead of extracting the bundled runtime, so you do not
> overwrite a newer BepInEx installation.

## First-time BepInEx 5 installation (manual)

BepInEx is the mod loader this plugin runs inside. If you already use BepInEx 5 for other
SimplePlanes 2 mods, skip to step 5.

1. **Close the game completely.** BepInEx cannot be installed while `SimplePlanes 2.exe` is running.
2. Download the official BepInEx 5 Windows x64 archive:
   [`BepInEx_win_x64_5.4.23.5.zip`](https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip)
   (SHA-256 `82f9878551030f54657792c0740d9d51a09500eeae1fba21106b0c441e6732c4`).
3. Extract the archive **into the game directory** — the folder containing `SimplePlanes 2.exe`.
   After extracting you must see `winhttp.dll`, `doorstop_config.ini`, `.doorstop_version` and the
   `BepInEx` folder **next to the game executable**, not inside another folder.
4. Start the game once, then close it. A new file `BepInEx\LogOutput.log` should appear and contain
   the line `Chainloader startup complete`. This proves BepInEx is working.
5. Copy `dist\SP2VariableDialogSize.dll` from this repository (or from the release archive) into:

   ```text
   <game folder>\BepInEx\plugins\SP2VariableDialogSize.dll
   ```

6. Start the game again. `BepInEx\LogOutput.log` should now contain:

   ```text
   [Info   :SP2 Variable Dialog Size] SP2 Variable Dialog Size 1.0.0 已加载（宽度 1000，高度 700）。
   [Info   :SP2 Variable Dialog Size] 已挂接 ResourceLoader.LoadXml，变量对话框将按配置调整初始尺寸。
   [Info   :SP2 Variable Dialog Size] 已挂接变量对话框初始化，可拖动边角缩放。
   ```

### Common installation mistakes

| Symptom | Cause / fix |
| --- | --- |
| Game starts, nothing happens, no `BepInEx\LogOutput.log` | Files were extracted one level too deep. `winhttp.dll` must sit directly next to `SimplePlanes 2.exe`. |
| `LogOutput.log` exists but has no plugin lines | The DLL is in the wrong folder. It must be inside `BepInEx\plugins\`. |
| The game closes instantly after installation | Another copy of BepInEx / an incompatible loader is present. Remove the old `BepInEx` folder and redo step 3. |
| Plugin DLL cannot be replaced | The game is still running. Close it and try again. |

## Upgrading

Close `SimplePlanes 2` and download the newest archive from the
[releases page](releases/latest).

- **Plugin only:** replace `BepInEx/plugins/SP2VariableDialogSize.dll` with the new one. Your
  configuration and other plugins are kept.
- **Bundled BepInEx too:** extract the whole archive into the game directory and allow files to be
  overwritten. The package contains no personal configuration, so your settings remain.
- Do **not** delete the whole `BepInEx` folder if other plugins are installed.

## Usage

| Action | How |
| --- | --- |
| Resize width and height | Drag the small square in the **bottom-right corner** of the dialog |
| Resize width only | Drag the **right edge** of the dialog |
| Resize height only | Drag the **bottom edge** of the dialog |
| Apply on next open | Let go of the mouse — the size is saved automatically |

The plugin affects the *Variable Setters* dialog (File menu → Variable Setters) and, by default,
also the *Variable Outputs* dialog. Both dialogs live in the designer.

## Configuration

Created after the first launch:

```text
BepInEx/config/sp2.variable-dialog-size.cfg
```

| Key | Default | Description |
| --- | --- | --- |
| `General / Enabled` | `true` | Master switch |
| `General / ApplyToOutputsDialog` | `true` | Also resize the *Variable Outputs* dialog |
| `General / AllowResizeByDragging` | `true` | Enable the drag handles |
| `Size / Width` | `1000` | Dialog width (game default **640**; `0` = keep default) |
| `Size / Height` | `700` | Variable list height (game default **400**; `0` = keep default) |
| `Advanced / RowHeight` | `0` | Row height (game default **50**; `0` = keep default) |
| `Advanced / OpenRowHeight` | `0` | Expanded row height (game default **105**; `0` = keep default) |

Editing the file requires a game restart. Dragging updates `Width` / `Height` immediately; reopen
the dialog to use the new size.

## Uninstall

Delete only the plugin file:

```text
BepInEx/plugins/SP2VariableDialogSize.dll
```

Keep `BepInEx`, `winhttp.dll` and `doorstop_config.ini` if other mods still use BepInEx.
The configuration file `BepInEx/config/sp2.variable-dialog-size.cfg` may be deleted as well.

## Troubleshooting

- **No effect:** check `BepInEx\LogOutput.log` for the three lines shown above. A `未找到 ...` message
  means the game update renamed an internal method; open an issue with the log lines.
- **Dialog still small:** another UI mod may override the same `.dialog-wide` style. Disable other UI
  mods and try again.
- **Demo build:** the demo version of SimplePlanes 2 does not include these dialogs.

## Privacy and release scope

- No telemetry, no online updates, no data upload.
- The repository and release archive contain no personal configuration, logs, caches, account
  details or developer-machine paths.
- BepInEx in the release archive comes from the official `5.4.23.5` Windows x64 release and is
  verified against the pinned SHA-256 above.

See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).

## Building from source

Requirements: Windows, the .NET Framework C# compiler (bundled with Windows) and a local
SimplePlanes 2 installation.

```powershell
.\build.ps1
.\build.ps1 -GamePath "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

Build a ready-to-install archive (downloads and verifies the official BepInEx 5.4.23.5 zip):

```powershell
.\package.ps1 -Version 1.0.0
```

Run the offline tests (no game required):

```powershell
.\tests\run-tests.ps1
```

## How it works

The two dialogs are defined in XML packed into the game's `resources.assets`. The plugin postfixes
`Jundroo.Juicy.ResourceLoader.LoadXml(string)` and, for those two paths only, injects an inline style
that overrides `.dialog-wide`'s width and sets the `ScrollView` height. Dragging the edges sets the
panel's `Width` and the scroll view's `Height` at runtime, and the result is written back to the
config.

## License

[MIT](LICENSE)
