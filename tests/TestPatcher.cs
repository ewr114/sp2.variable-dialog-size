using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SP2VariableDialogSize;

internal static class TestPatcher
{
    private static int _failures;

    private static void Check(bool ok, string message)
    {
        Console.WriteLine((ok ? "PASS  " : "FAIL  ") + message);
        if (!ok)
        {
            _failures++;
        }
    }

    private static XElement Load(string path)
    {
        return XDocument.Parse(File.ReadAllText(path)).Root;
    }

    private static XElement FindStyle(XElement root, string className)
    {
        return root.Descendants("Styles")
            .SelectMany(s => s.Elements("Style"))
            .FirstOrDefault(e => (string)e.Attribute("class") == className);
    }

    private static void Main()
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        string fixture = Path.Combine(dir, "VariableSettersDialog.sample.xml");

        // ---------- default size apply ----------
        XElement dialog = Load(fixture);
        DialogSizeSettings settings = new DialogSizeSettings();
        bool changed = DialogSizePatcher.Apply(dialog, settings);

        Check(changed, "Apply() reports a change");
        XElement wide = FindStyle(dialog, "dialog-wide");
        Check(wide != null && (string)wide.Attribute("width") == "1000", "dialog-wide width = 1000");
        XElement scroll = dialog.Descendants("ScrollView").FirstOrDefault();
        Check(scroll != null && (string)scroll.Attribute("height") == "700", "ScrollView height = 700");
        Check((string)FindStyle(dialog, "variable-row").Attribute("height") == "50", "row height stays 50 by default");
        Check((string)FindStyle(dialog, "row-open").Attribute("height") == "105", "open row height stays 105 by default");

        // The override must be added to the inline stylesheet (the one without a path),
        // because inline styles load after Xml/Styles and therefore win.
        XElement inlineStylesheet = dialog
            .Descendants("Stylesheet")
            .FirstOrDefault(e => e.Attribute("path") == null);
        Check(inlineStylesheet != null && inlineStylesheet.Descendants("Style").Contains(wide),
            "override style lives in the inline stylesheet");

        // ---------- custom values ----------
        XElement custom = Load(fixture);
        DialogSizePatcher.Apply(custom, new DialogSizeSettings
        {
            Width = 1280,
            Height = 900,
            RowHeight = 60,
            OpenRowHeight = 126
        });
        Check((string)FindStyle(custom, "dialog-wide").Attribute("width") == "1280", "custom width 1280");
        Check((string)FindStyle(custom, "variable-row").Attribute("height") == "60", "custom row height 60");
        Check((string)FindStyle(custom, "row-open").Attribute("height") == "126", "custom open row height 126");
        Check((string)custom.Descendants("ScrollView").First().Attribute("height") == "900", "custom list height 900");

        // ---------- 0 means "keep the game default" ----------
        XElement untouched = Load(fixture);
        bool untouchedChanged = DialogSizePatcher.Apply(untouched, new DialogSizeSettings { Width = 0, Height = 0 });
        Check(!untouchedChanged, "Width=0/Height=0 makes no changes");
        Check(FindStyle(untouched, "dialog-wide") == null, "Width=0 does not add a dialog-wide style");
        Check((string)untouched.Descendants("ScrollView").First().Attribute("height") == "400",
            "Height=0 keeps the original list height");

        // ---------- path matching ----------
        DialogSizeSettings s = new DialogSizeSettings();
        Check(DialogSizePatcher.IsTargetPath("Xml/Design/VariableSettersDialog", s), "matches variable setters path");
        Check(DialogSizePatcher.IsTargetPath("Xml/Design/VariableOutputsDialog", s), "matches variable outputs path");
        Check(DialogSizePatcher.IsTargetPath("xml/design/variablesettersdialog", s), "path match is case-insensitive");
        Check(!DialogSizePatcher.IsTargetPath("Xml/Design/OtherDialog", s), "ignores unrelated dialogs");
        s.ApplyToOutputsDialog = false;
        Check(!DialogSizePatcher.IsTargetPath("Xml/Design/VariableOutputsDialog", s), "outputs dialog can be excluded");
        s.ApplyToOutputsDialog = true;
        s.Enabled = false;
        Check(!DialogSizePatcher.IsTargetPath("Xml/Design/VariableSettersDialog", s), "disabled settings ignore every dialog");

        // ---------- dump the patched XML for manual inspection ----------
        string outDir = Path.Combine(dir, "out");
        Directory.CreateDirectory(outDir);
        XElement sample = Load(fixture);
        DialogSizePatcher.Apply(sample, new DialogSizeSettings());
        File.WriteAllText(Path.Combine(outDir, "VariableSettersDialog.patched.xml"),
            new XDocument(sample).ToString(), new System.Text.UTF8Encoding(false));

        Console.WriteLine();
        Console.WriteLine(_failures == 0 ? "ALL TESTS PASSED" : _failures + " TEST(S) FAILED");
        Environment.Exit(_failures == 0 ? 0 : 1);
    }
}
