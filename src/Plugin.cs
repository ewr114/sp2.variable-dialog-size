using System;
using System.Reflection;
using System.Xml.Linq;
using Assets.Scripts.Design.UI.Variables;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Jundroo.Juicy;

namespace SP2VariableDialogSize
{
    /// <summary>
    /// SimplePlanes 2 —— 变量对话框尺寸调整。
    ///
    /// 两件事：
    /// 1. 打开对话框时按配置设置初始尺寸（改 Xml/Design/VariableSettersDialog 的样式与 ScrollView 高度）；
    /// 2. 在对话框右下角/右边/下边挂上拖动条，拖动即可实时改变大小，松手后记进配置。
    ///
    /// 对话框宽度由样式 .dialog-wide 决定（游戏默认 640），变量列表可视高度由
    /// &lt;ScrollView height="400"&gt; 决定，因此这两处就是“窗口大小”。
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "sp2.variable-dialog-size";
        public const string PluginName = "SP2 Variable Dialog Size";
        public const string PluginVersion = "1.0.0";

        internal static Plugin Instance;
        internal static ManualLogSource Log;

        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> ApplyToOutputsDialog;
        internal static ConfigEntry<bool> AllowResizeByDragging;
        internal static ConfigEntry<int> Width;
        internal static ConfigEntry<int> Height;
        internal static ConfigEntry<int> RowHeight;
        internal static ConfigEntry<int> OpenRowHeight;

        private Harmony _harmony;
        private bool _xmlPatched;
        private bool _dialogPatched;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            Enabled = Config.Bind(
                "General", "Enabled", true,
                "是否启用本插件。 / Enable this plugin.");

            ApplyToOutputsDialog = Config.Bind(
                "General", "ApplyToOutputsDialog", true,
                "是否同时调整“变量输出 (Variable Outputs)”对话框。 / Also resize the Variable Outputs dialog.");

            AllowResizeByDragging = Config.Bind(
                "General", "AllowResizeByDragging", true,
                "是否允许在游戏里拖动对话框右下角/右边/下边改变大小。 / Allow dragging the dialog edges to resize.");

            Width = Config.Bind(
                "Size", "Width", 1000,
                "对话框宽度，游戏默认 640。设为 0 则保持游戏默认值（拖动改变大小后会自动写回这里）。 / Dialog width, game default is 640. 0 = keep default.");

            Height = Config.Bind(
                "Size", "Height", 700,
                "变量列表可视区域高度，游戏默认 400。设为 0 则保持游戏默认值（拖动改变大小后会自动写回这里）。 / Height of the variable list area, game default is 400. 0 = keep default.");

            RowHeight = Config.Bind(
                "Advanced", "RowHeight", 0,
                "每一行（收起状态）的高度，游戏默认 50。设为 0 则保持游戏默认值。 / Row height, game default is 50. 0 = keep default.");

            OpenRowHeight = Config.Bind(
                "Advanced", "OpenRowHeight", 0,
                "展开状态每一行的高度，游戏默认 105。设为 0 则保持游戏默认值。 / Expanded row height, game default is 105. 0 = keep default.");

            _harmony = new Harmony(PluginGuid);
            TryPatch();

            if (!_xmlPatched || !_dialogPatched)
            {
                // 游戏程序集可能尚未加载，等它加载完成后再挂接。
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            }

            Log.LogInfo(PluginName + " " + PluginVersion + " 已加载（宽度 " + Width.Value +
                        "，高度 " + Height.Value + "）。");
        }

        private void Start()
        {
            TryPatch();
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            TryPatch();
            if (_xmlPatched && _dialogPatched)
            {
                AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
            }
        }

        private void TryPatch()
        {
            if (!_xmlPatched)
            {
                _xmlPatched = TryPatchXmlLoader();
            }

            if (!_dialogPatched)
            {
                _dialogPatched = TryPatchDialogs();
            }
        }

        private bool TryPatchXmlLoader()
        {
            try
            {
                MethodInfo target = AccessTools.Method(
                    typeof(ResourceLoader), "LoadXml", new[] { typeof(string) });

                if (target == null)
                {
                    Log.LogError("未找到 Jundroo.Juicy.ResourceLoader.LoadXml(string)，" +
                                 "可能是游戏版本变化，插件无法生效。");
                    return false;
                }

                MethodInfo postfix = typeof(Plugin).GetMethod(
                    "OnLoadXml", BindingFlags.NonPublic | BindingFlags.Static);

                _harmony.Patch(target, postfix: new HarmonyMethod(postfix));
                Log.LogInfo("已挂接 ResourceLoader.LoadXml，变量对话框将按配置调整初始尺寸。");
                return true;
            }
            catch (Exception e)
            {
                Log.LogError("挂接 LoadXml 失败：" + e);
                return false;
            }
        }

        private bool TryPatchDialogs()
        {
            try
            {
                MethodInfo postfix = typeof(Plugin).GetMethod(
                    "OnDialogInitialized", BindingFlags.NonPublic | BindingFlags.Static);
                HarmonyMethod patch = new HarmonyMethod(postfix);

                MethodInfo setters = AccessTools.Method(typeof(VariableSettersDialogScript), "Initialize");
                MethodInfo outputs = AccessTools.Method(typeof(VariableOutputsDialogScript), "Initialize");

                if (setters == null || outputs == null)
                {
                    Log.LogError("未找到变量对话框的 Initialize 方法，拖动缩放不可用。");
                    return false;
                }

                _harmony.Patch(setters, postfix: patch);
                _harmony.Patch(outputs, postfix: patch);
                Log.LogInfo("已挂接变量对话框初始化，可拖动边角缩放。");
                return true;
            }
            catch (Exception e)
            {
                Log.LogError("挂接变量对话框失败：" + e);
                return false;
            }
        }

        /// <summary>
        /// XML 解析完成后修改其内容。此时界面还没构建，所以改动会直接体现在布局上。
        /// </summary>
        private static void OnLoadXml(string path, ref XElement __result)
        {
            try
            {
                DialogSizeSettings settings = CurrentSettings();
                if (!DialogSizePatcher.IsTargetPath(path, settings))
                {
                    return;
                }

                if (DialogSizePatcher.Apply(__result, settings))
                {
                    Log.LogInfo("已调整 " + path + " 的尺寸：宽度 " + settings.Width +
                                "，列表高度 " + settings.Height + "。");
                }
            }
            catch (Exception e)
            {
                Log.LogError("调整 " + path + " 的 XML 时出错：" + e);
            }
        }

        /// <summary>
        /// 对话框创建完成（行也建好了）后调用：设置尺寸并挂上拖动条。
        /// </summary>
        private static void OnDialogInitialized(object __instance)
        {
            try
            {
                PanelDialogScript dialog = __instance as PanelDialogScript;
                if (dialog == null || !Enabled.Value)
                {
                    return;
                }

                if (AllowResizeByDragging.Value)
                {
                    DialogResize.Attach(dialog.Panel);
                }
            }
            catch (Exception e)
            {
                Log.LogError("初始化对话框拖动缩放时出错：" + e);
            }
        }

        /// <summary>
        /// 拖动结束后记住大小（写回配置文件，下次打开沿用）。
        /// </summary>
        internal static void SaveSize(int width, int height)
        {
            try
            {
                if (width <= 0 || height <= 0)
                {
                    return;
                }

                bool changed = false;
                if (Width.Value != width)
                {
                    Width.Value = width;
                    changed = true;
                }

                if (Height.Value != height)
                {
                    Height.Value = height;
                    changed = true;
                }

                if (changed)
                {
                    Instance.Config.Save();
                    Log.LogInfo("已记住对话框大小：" + width + " x " + height + "。");
                }
            }
            catch (Exception e)
            {
                Log.LogError("保存对话框大小失败：" + e);
            }
        }

        private static DialogSizeSettings CurrentSettings()
        {
            return new DialogSizeSettings
            {
                Enabled = Enabled.Value,
                ApplyToOutputsDialog = ApplyToOutputsDialog.Value,
                Width = Width.Value,
                Height = Height.Value,
                RowHeight = RowHeight.Value,
                OpenRowHeight = OpenRowHeight.Value
            };
        }
    }
}
