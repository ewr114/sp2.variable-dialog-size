using System;
using System.Linq;
using System.Xml.Linq;

namespace SP2VariableDialogSize
{
    /// <summary>
    /// 用户可配置的尺寸参数。0 表示保持游戏原值。
    /// </summary>
    public sealed class DialogSizeSettings
    {
        public bool Enabled = true;
        public bool ApplyToOutputsDialog = true;
        public int Width = 1000;
        public int Height = 700;
        public int RowHeight = 0;
        public int OpenRowHeight = 0;
    }

    /// <summary>
    /// 纯 XML 修改逻辑，不依赖 BepInEx / Unity，便于离线测试。
    /// 目标 XML 结构（游戏内 Xml/Design/VariableSettersDialog）：
    ///   &lt;Xml&gt;
    ///     &lt;Stylesheets&gt;
    ///       &lt;Stylesheet path="Xml/Styles" /&gt;         全局样式，.dialog-wide 宽度 640
    ///       &lt;Stylesheet&gt;&lt;Styles&gt;...&lt;/Styles&gt;...&lt;/Stylesheet&gt;   内联样式，优先生效
    ///     &lt;/Stylesheets&gt;
    ///     &lt;Widgets&gt;
    ///       &lt;Instance templateId="dialog" class="dialog-wide" ...&gt;
    ///         &lt;Section sectionId="content"&gt;
    ///           &lt;ScrollView height="400"&gt;             变量列表的可视区域
    ///             &lt;VerticalLayout id="items-parent" ... /&gt;
    ///           &lt;/ScrollView&gt;
    ///         &lt;/Section&gt;
    ///       &lt;/Instance&gt;
    ///     &lt;/Widgets&gt;
    ///   &lt;/Xml&gt;
    /// 因此：宽度 = 内联覆盖 .dialog-wide 的 width；高度 = ScrollView 的 height。
    /// </summary>
    public static class DialogSizePatcher
    {
        public const string SettersDialogPath = "Xml/Design/VariableSettersDialog";
        public const string OutputsDialogPath = "Xml/Design/VariableOutputsDialog";

        public static bool IsTargetPath(string path, DialogSizeSettings settings)
        {
            if (string.IsNullOrEmpty(path) || settings == null || !settings.Enabled)
            {
                return false;
            }

            if (string.Equals(path, SettersDialogPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return settings.ApplyToOutputsDialog &&
                   string.Equals(path, OutputsDialogPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 就地修改已解析的 XML 根节点，返回是否做了改动。
        /// </summary>
        public static bool Apply(XElement root, DialogSizeSettings settings)
        {
            if (root == null || settings == null || !settings.Enabled)
            {
                return false;
            }

            bool changed = false;

            XElement styles = root.Descendants("Styles").FirstOrDefault();
            if (styles != null)
            {
                if (settings.Width > 0)
                {
                    changed |= SetStyleValue(styles, "dialog-wide", "width", settings.Width);
                }

                if (settings.RowHeight > 0)
                {
                    changed |= SetStyleValue(styles, "variable-row", "height", settings.RowHeight);
                }

                if (settings.OpenRowHeight > 0)
                {
                    changed |= SetStyleValue(styles, "row-open", "height", settings.OpenRowHeight);
                }
            }

            if (settings.Height > 0)
            {
                XElement scrollView = root.Descendants("ScrollView").FirstOrDefault();
                if (scrollView != null)
                {
                    scrollView.SetAttributeValue("height", settings.Height);
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>
        /// 修改（或新增）某条内联样式里的属性。内联样式加载在 Xml/Styles 之后，
        /// 因此可以覆盖全局样式表中的同名规则。
        /// </summary>
        private static bool SetStyleValue(XElement styles, string className, string attribute, int value)
        {
            XElement target = styles
                .Elements("Style")
                .FirstOrDefault(e => string.Equals((string)e.Attribute("class"), className, StringComparison.Ordinal));

            if (target == null)
            {
                styles.Add(new XElement(
                    "Style",
                    new XAttribute("class", className),
                    new XAttribute(attribute, value)));
                return true;
            }

            target.SetAttributeValue(attribute, value);
            return true;
        }
    }
}
