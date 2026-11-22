[English](../README.md)

# SP2 Variable Dialog Size（简体中文）

`SP2 Variable Dialog Size` 是适用于 Windows x64 版 `SimplePlanes 2` 的本地界面插件。
当前版本为 `1.0.0`，使用 `BepInEx 5 Mono x64` 加载，已针对游戏版本 `0.7.10.102` 构建。
插件只改变设计器里“变量设置”和“变量输出”两个对话框的大小，并记住你调整后的尺寸；
它不修改载具、不改变物理、不发送任何网络消息，也不依赖其他自定义插件。

## 直接安装（已包含 BepInEx）

仓库的 `release` 目录提供可部署压缩包：

[`release/SP2VariableDialogSize-v1.0.0-win-x64.zip`](../release/SP2VariableDialogSize-v1.0.0-win-x64.zip)

压缩包已经包含插件 DLL 和完整的 BepInEx `5.4.23.5` Windows x64 运行时，
**不需要另行安装 BepInEx**。

1. 完全关闭 `SimplePlanes 2`。
2. 打开压缩包，将其中全部内容直接解压到游戏根目录，也就是 `SimplePlanes 2.exe` 所在目录。
3. 如果系统询问是否合并 `BepInEx` 文件夹，选择合并。
4. 启动游戏，进入设计器，打开变量对话框即可。

安装后的关键文件结构如下：

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

发布包不包含 `BepInEx/config`、日志、缓存或其他插件，所以覆盖安装不会重置现有配置。
压缩包的 SHA-256 位于 [`release/SHA256SUMS.txt`](../release/SHA256SUMS.txt)。
> 如果你已经为其他模组装过 BepInEx 5，建议只从压缩包里复制 `BepInEx/plugins/SP2VariableDialogSize.dll`，
> 不要覆盖随包附带的 BepInEx 运行时，以免把你现有的新版本降级。

## 第一次安装 BepInEx 5（手动）

BepInEx 是让这类插件能够加载的“模组加载器”。如果你已经为其他 SimplePlanes 2 模组装过
BepInEx 5，可以直接跳到第 5 步。

1. **完全关闭游戏。** 游戏（`SimplePlanes 2.exe`）运行时无法安装 BepInEx。
2. 下载官方 BepInEx 5 Windows x64 压缩包：
   [`BepInEx_win_x64_5.4.23.5.zip`](https://github.com/BepInEx/BepInEx/releases/download/v5.4.23.5/BepInEx_win_x64_5.4.23.5.zip)
   （SHA-256 `82f9878551030f54657792c0740d9d51a09500eeae1fba21106b0c441e6732c4`）。
3. 把压缩包**解压到游戏根目录**，也就是 `SimplePlanes 2.exe` 所在目录。解压后应当能直接看到
   `winhttp.dll`、`doorstop_config.ini`、`.doorstop_version` 和 `BepInEx` 文件夹，
   它们必须和游戏可执行文件**同级**，不能多套一层文件夹。
4. 启动一次游戏再退出。此时应生成 `BepInEx\LogOutput.log`，里面包含
   `Chainloader startup complete` 一行，说明 BepInEx 已正常工作。
5. 把本仓库的 `dist\SP2VariableDialogSize.dll`（或发布压缩包里的同名文件）复制到：

   ```text
   <游戏目录>\BepInEx\plugins\SP2VariableDialogSize.dll
   ```

6. 再次启动游戏。`BepInEx\LogOutput.log` 中应出现：

   ```text
   [Info   :SP2 Variable Dialog Size] SP2 Variable Dialog Size 1.0.0 已加载（宽度 1000，高度 700）。
   [Info   :SP2 Variable Dialog Size] 已挂接 ResourceLoader.LoadXml，变量对话框将按配置调整初始尺寸。
   [Info   :SP2 Variable Dialog Size] 已挂接变量对话框初始化，可拖动边角缩放。
   ```

### 常见安装错误

| 现象 | 原因与处理 |
| --- | --- |
| 游戏能启动，但完全没有 `BepInEx\LogOutput.log` | 解压多套了一层文件夹。`winhttp.dll` 必须直接放在 `SimplePlanes 2.exe` 旁边。 |
| 有 `LogOutput.log`，但没有插件加载信息 | DLL 放错位置。必须放进 `BepInEx\plugins\` 目录。 |
| 装完后游戏一启动就闪退 | 存在旧版/不兼容的 BepInEx。删除旧 `BepInEx` 文件夹后按第 3 步重装。 |
| 覆盖 DLL 时提示文件被占用 | 游戏还在运行，完全退出后再复制。 |

## 升级插件

先完全关闭 `SimplePlanes 2`，从 [最新发布页](../releases/latest)
下载新的 Windows x64 压缩包，然后任选一种方式：

- **只替换插件 DLL：** 用新包里的 `BepInEx/plugins/SP2VariableDialogSize.dll` 覆盖游戏目录中的同名文件，
  现有配置和其他插件都会保留。
- **同时更新随包附带的 BepInEx：** 把新压缩包全部内容解压到游戏根目录，允许合并文件夹并覆盖同名文件。
  发布包不含个人配置，因此现有设置会保留。
- 如果 `BepInEx/plugins` 里还有别的插件，**不要**删除整个 `BepInEx` 文件夹。

## 使用方法

| 操作 | 方法 |
| --- | --- |
| 同时改变宽度和高度 | 拖动对话框**右下角**的小方块 |
| 只改变宽度 | 拖动对话框**右边**的拖动带 |
| 只改变高度 | 拖动对话框**下边**的拖动带 |
| 让尺寸生效 | 松开鼠标即可自动保存，下次打开沿用 |

插件默认同时作用于“变量设置”（文件菜单 → Variable Setters）和“变量输出”（Variable Outputs）两个对话框。
两个对话框都只在设计器里出现。

## 配置

首次启动后生成：

```text
BepInEx/config/sp2.variable-dialog-size.cfg
```

| 配置项 | 默认值 | 说明 |
| --- | --- | --- |
| `General / Enabled` | `true` | 总开关 |
| `General / ApplyToOutputsDialog` | `true` | 是否同时调整“变量输出”对话框 |
| `General / AllowResizeByDragging` | `true` | 是否启用拖动边角缩放 |
| `Size / Width` | `1000` | 对话框宽度（游戏默认 **640**；`0` = 保持默认） |
| `Size / Height` | `700` | 变量列表可视高度（游戏默认 **400**；`0` = 保持默认） |
| `Advanced / RowHeight` | `0` | 单行高度（游戏默认 **50**；`0` = 保持默认） |
| `Advanced / OpenRowHeight` | `0` | 展开行高度（游戏默认 **105**；`0` = 保持默认） |

修改配置文件后需要重启游戏。拖动改变的大小会立即写入配置，重新打开对话框即可看到效果。

## 卸载

只删除插件文件即可：

```text
BepInEx/plugins/SP2VariableDialogSize.dll
```

如果还有其他模组在使用 BepInEx，请不要删除 `BepInEx` 文件夹、`winhttp.dll` 或
`doorstop_config.ini`。配置文件 `BepInEx/config/sp2.variable-dialog-size.cfg` 可以一并删除。

## 常见问题

- **装了没反应：** 先看 `BepInEx\LogOutput.log` 里有没有上面那三行日志。出现
  `未找到 ...` 说明游戏更新改动了内部方法，请带上日志内容反馈。
- **对话框还是很小：** 可能被其他界面模组覆盖了同一个 `.dialog-wide` 样式，临时停用其他界面模组再试。
- **试用版无效：** 试用版游戏本身没有这两个对话框。

## 隐私与发布范围

- 插件没有遥测、联网更新或数据上传功能。
- 仓库和发布包不包含个人配置、运行日志、缓存、账户信息或开发机器路径。
- 发布包中的 BepInEx 来自官方 `5.4.23.5` Windows x64 发行包，并使用上面固定的 SHA-256 校验。

第三方组件及许可证见 [THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md)。

## 从源码构建

需要 Windows、.NET Framework 自带的 C# 编译器，以及本机安装的 `SimplePlanes 2`。

```powershell
.\build.ps1
.\build.ps1 -GamePath "D:\SteamLibrary\steamapps\common\SimplePlanes 2"
```

生成可直接分发的压缩包（会自动下载并校验官方 BepInEx 5.4.23.5）：

```powershell
.\package.ps1 -Version 1.0.0
```

运行离线测试（不需要游戏）：

```powershell
.\tests\run-tests.ps1
```

## 工作原理

两个对话框的布局写在游戏 `resources.assets` 里的 XML 中。插件在
`Jundroo.Juicy.ResourceLoader.LoadXml(string)` 返回后，只针对这两个路径注入一条内联样式，
覆盖 `.dialog-wide` 的宽度并设置 `ScrollView` 的高度；拖动时直接修改面板的 `Width` 和滚动区的
`Height`，最后把结果写回配置文件。

## 许可证

[MIT](../LICENSE)
