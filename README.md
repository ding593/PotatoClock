# PotatoClock
**简体中文** | [English](README.en.md)
Windows 上的轻量番茄钟。无边框置顶小窗 + 托盘常驻 + 任务清单 + 中英双语 + 可换配色，界面按 Apple Human Interface Guidelines 的取值自绘（圆角、层级、动效曲线），基于 WinForms / .NET 10。

| 深色 | 浅色（English） |
| --- | --- |
| <img src="https://github.com/user-attachments/assets/c3acf5a2-431e-4644-a0ea-d16641499bd6" width="320" alt="深色主界面"> | <img src="https://github.com/user-attachments/assets/785c4d9f-d2ed-4e5d-a99e-ef70a3cdb913" width="308" alt="浅色主界面"> |

| 折叠小窗 | 设置窗口 |
| --- | --- |
| <img src="https://github.com/user-attachments/assets/f0797ef3-9ddd-44aa-a4d3-bad376005eca" width="318" alt="折叠小窗"> | <img src="https://github.com/user-attachments/assets/fabe6f1b-6ee2-4c69-927d-744abdbc60ff" width="537" alt="设置窗口"> |

## 功能

- **计时**：专注 / 短休息 / 长休息三段循环。默认 25 / 5 / 15 分钟，每完成 4 个专注进入一次长休。阶段结束**不自动开始**，由你点「开始」继续；阶段切换时倒计时会用强调色柔和闪一下。
- **轮次圆点**：一排圆点显示本组进度 —— 已完成的专注是实心强调色，当前那个是强调色圆环，其余是空心三级色。
- **托盘常驻**：点 ✕ 或 Alt+F4 最小化到托盘（首次有气泡提示），托盘右键可 显示/隐藏、开始/暂停、跳过、设置、退出；双击托盘图标切换显示。
- **提醒**：阶段自然结束时播放系统提示音并弹出气泡通知（跳过的阶段不提醒）。
- **任务清单**：圆角输入框加任务（回车或点 +），圆形勾选框标记完成（完成后标题变灰并加删除线），鼠标悬停行尾浮现删除按钮。改动实时落盘。
- **中英双语**：默认跟随系统（中文环境用中文，其余用英文），设置里可切「跟随系统 / 中文 / English」，**切换后立刻生效**，包括托盘菜单与通知文案。
- **配色**：内置 深色 / 浅色 / 暖阳 / 薄荷 / 紫罗兰 / 高对比 六套（取自 Apple 系统色），另有「自定义」可用三个色块调背景、文字、强调色，其余层级自动派生。
- **健壮性**：数据文件损坏自动回落默认值；目录不可写时红字提示而不是静默失败；同一时间只允许一个实例（上一个被强杀也能正常接管）；未处理异常写入 `error.log` 而不是弹崩溃对话框。

## 界面设计

- **窗口**：340×246（展开任务面板 340×522），无边框圆角（12 px）+ 发丝边框，按住顶部空白可拖动，始终置顶。
- **层级**：窗口底色 → 卡片表面 → 三级文字（主要 100% / 次要 60% / 三级 30%）→ 单一强调色 → 发丝分隔线。深色卡片用 `#1C1C1E` / `#2C2C2E`，不用纯黑做表面。
- **圆角**：窗口 12、卡片 10、控件 8、列表行 6（遵循 Apple「内半径 = 外半径 − 内边距」）。
- **字体**：优先 Segoe UI Variable（Windows 11），否则 Segoe UI / Segoe UI Semibold，中文由系统字体链接回退 Microsoft YaHei UI；只用 Regular 与 Semibold 两档字重。倒计时按**固定数字槽**绘制，秒变化不抖动。
- **动效**：展开/折叠 400 ms `cubic-bezier(0.32, 0.72, 0, 1)`，悬停 170 ms，开关与勾选 200 ms，进度条 200 ms 缓动，设置窗口淡入 400 ms。所有动画可被打断并从当前值续走。
- **图标**：全部用 GDI+ 手绘的线条图标（SF Symbols 风格），不依赖任何图标字体。

## 快捷键

| 按键 | 作用 |
| --- | --- |
| 空格 | 开始 / 暂停（焦点在任务输入框时除外） |
| Ctrl+N | 展开任务面板并定位到输入框 |
| Esc | 收起任务面板（设置窗口里为取消） |
| 回车 | 任务输入框里回车即添加任务；设置窗口里为确定 |

## 环境要求

- Windows 10 / 11
- 构建：.NET 10 SDK
- 运行：框架依赖版需要 .NET 10 桌面运行时（`Microsoft.WindowsDesktop.App`）；自包含版不需要任何前置

## 构建与运行

```powershell
dotnet build PotatoClock.csproj -c Release
dotnet run --project PotatoClock.csproj            # 默认 Debug 运行

# 运行 Release 构建产物
.\bin\Release\net10.0-windows\PotatoClock.exe
```

## 数据文件

都保存在 **exe 所在目录**，不会写到 C 盘用户目录：

| 文件 | 内容 |
| --- | --- |
| `settings.json` | 时长、提醒、窗口、语言、主题配置；首次运行自动生成，可直接手工编辑 |
| `tasks.json` | 任务清单与完成状态 |
| `error.log` | 仅在出现未处理异常时生成，内含完整堆栈 |

写入方式为先写 `.tmp` 再替换；读取失败（文件损坏或被改坏）自动回落默认值。若目录不可写（例如放在 `Program Files` 下），设置窗口会红字提示「当前目录不可写，修改不会保存」，程序仍可使用，且不会偷偷改写到别处 —— **所以安装包请装到用户目录**（如 `%LOCALAPPDATA%\Programs\PotatoClock`），不要装 `Program Files`。

主题名与设置结构向后兼容：旧的 `深色/浅色/暖橙/青绿/紫罗兰/高对比/自定义` 会自动映射到新的 `dark/light/warm/mint/violet/contrast/custom`。

## 命令行参数

```powershell
PotatoClock.exe --help                        # 显示用法
PotatoClock.exe --test-seconds=3              # 开发用：把所有阶段压缩成 3 秒，便于验证阶段切换
PotatoClock.exe --data-dir=D:\tmp\pclock      # 覆盖数据目录，避免污染 exe 目录
PotatoClock.exe --log=D:\tmp\phase.log        # 开发用：把每次阶段切换追加写入日志
PotatoClock.exe --language=en                 # 覆盖界面语言（zh / en）
```

`--test-seconds` 与 `--log` 组合可自动化验证阶段切换：每段结束写一行
`时间 / completed=阶段 / skipped=是否跳过 / focusCount=累计完成专注数 / next=下一阶段 / nextDurationSeconds=时长`。

## 打包与发布

```powershell
# A. 自包含单文件（对方机器无需任何前置，体积较大）
dotnet publish PotatoClock.csproj -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true `
  -p:PublishTrimmed=false -o dist\PotatoClock-win-x64

# B. 框架依赖（体积小，对方需装 .NET 10 桌面运行时）
dotnet publish PotatoClock.csproj -c Release -o dist\PotatoClock
```

注意：

- `PublishTrimmed=false` 必须保留 —— **WinForms 不支持裁剪**。
- 首次自包含发布会联网下载 win-x64 运行时包。
- 分发目录里**不应有** `*.pdb`、`settings.json`、`tasks.json`、`error.log`。
- 想做成带「下一步 / 开始菜单 / 卸载」的安装包，可用 [Inno Setup](https://jrsoftware.org/isinfo.php)，把 `DefaultDirName` 指到 `{localappdata}\Programs\PotatoClock` 并设 `PrivilegesRequired=lowest`（免 UAC 且目录可写）。
- 发布到 GitHub：先在本地 `dotnet publish` 出包并压成 zip，然后 **Releases → Draft a new release**，Tag 填 `v1.0.0`，把 zip 作为附件上传即可。

建议的 `.gitignore`：

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

## 目录结构

```
App/                   设置、主题、本地化、任务模型、存储
  AppSettings.cs       可持久化设置（含语言）与范围校验
  AppTheme.cs          主题层级、六套 Apple 系统色预设、自定义派生
  AppStore.cs          settings.json / tasks.json 读写（原子写、容错）
  Localization.cs      中英文案表 + 系统语言识别
  ColorHex.cs          #AARRGGBB / #RRGGBB 解析
  TaskItem.cs          任务模型
Controls/              全部自绘控件
  ThemedControl.cs     基类（双缓冲 + 主题 + 圆角工具）
  FlatButton.cs        圆角按钮（Primary / Secondary / Ghost / Icon，带悬停按下动画）
  ProgressBar.cs       细圆角进度条（数值缓动）
  TimeDisplay.cs       固定数字槽倒计时（阶段切换闪烁）
  PhaseDots.cs         轮次圆点指示器
  SurfacePanel.cs      卡片表面
  ToggleSwitch.cs      Apple 风格开关
  StepperField.cs      − / + 步进器
  SegmentedControl.cs  分段控件（滑动选中药丸）
  ThemeSwatchRow.cs    主题色块选择
  ColorWell.cs         自定义颜色色块
  TaskRow.cs           任务行（圆形勾选 + 删除线 + 悬停删除）
  RoundedField.cs      圆角输入框容器（内含原生 TextBox 以支持中文输入法）
  LogoMark.cs          窗口内 logo
Core/                  计时状态机（与界面完全解耦）
  PomodoroTimer.cs     按 UtcNow 截止时刻结算，休眠唤醒不丢阶段
  PomodoroPhase.cs     阶段枚举
  PhaseCompletedEventArgs.cs  阶段结束事件参数
Design/                设计令牌与基础设施
  Motion.cs            圆角 / 间距 / 时长刻度、缓动曲线、统一动画驱动器
  Icons.cs             SF Symbols 风格手绘图标
  Typography.cs        字体解析与缓存
  Shapes.cs            圆角路径
  WindowChrome.cs      无边框窗口的圆角裁剪、发丝边、阴影、拖动
Forms/                 主窗口与设置窗口
Assets/                app.ico（exe 与托盘图标）、logo.png（窗口标识）
Program.cs             入口、单实例、语言初始化、异常兜底
CommandLineOptions.cs  命令行参数
```

## 已知行为与限制

- 关闭按钮默认最小化到托盘，可在设置里关掉（关掉后 ✕ 直接退出）。
- 被外部强制关闭窗口（任务管理器、任务栏右键关闭）会直接结束进程；任务与设置此时已实时保存，不会丢数据。
- 任务列表超过面板高度时会出现系统原生滚动条（未自绘）。
- Windows 10 没有 Segoe UI Variable，会回退 Segoe UI；Windows 11 上自动使用其光学尺寸变体，字形更接近 SF Pro。
- 刻意不做：阶段结束自动开始、今日/历史番茄数统计、任务维度统计。
- 托盘图标若因强杀残留在通知区域，鼠标划过或重启资源管理器即可消失。

## 许可证

MIT
