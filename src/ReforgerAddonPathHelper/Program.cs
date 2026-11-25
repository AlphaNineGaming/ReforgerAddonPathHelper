using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Windows.Forms;

namespace ReforgerAddonPathHelper;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private static readonly Color Bg = Color.FromArgb(15, 23, 42);
    private static readonly Color Card = Color.FromArgb(30, 41, 59);
    private static readonly Color Field = Color.FromArgb(17, 27, 45);
    private static readonly Color TextMain = Color.FromArgb(241, 245, 249);
    private static readonly Color TextMuted = Color.FromArgb(148, 163, 184);
    private static readonly Color Accent = Color.FromArgb(34, 211, 238);
    private readonly TextBox _pathBox = new() { Dock = DockStyle.Fill, Text = @"D:\ArmaMods" };
    private readonly TextBox _commandBox = new() { Dock = DockStyle.Fill, ReadOnly = true };
    private readonly Label _status = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 4, 12, 4), ForeColor = TextMuted, BackColor = Card };

    public MainForm()
    {
        Text = "Reforger Addon Folder Helper";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = true;
        ClientSize = new Size(780, 510);
        MinimumSize = new Size(720, 470);
        BackColor = Bg;
        ForeColor = TextMain;
        Font = new Font("Segoe UI", 10F);

        var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Bg, Margin = new Padding(0, 0, 0, 14), AutoSize = true };
        header.Controls.Add(new Label { Text = "ARMA REFORGER  /  WORKSHOP SETUP", AutoSize = true, ForeColor = Accent, Font = new Font("Segoe UI Semibold", 9F), Margin = new Padding(0, 0, 0, 4) }, 0, 0);
        header.Controls.Add(new Label { Text = "Addon folder", AutoSize = true, ForeColor = TextMain, Font = new Font("Segoe UI Semibold", 21F), Margin = new Padding(0) }, 0, 1);
        var intro = new Label
        {
            Text = "Choose a drive with enough space. The helper checks that the folder is available, then can save the setting in Steam.",
            AutoSize = true, ForeColor = TextMuted, MaximumSize = new Size(700, 0), Margin = new Padding(0, 0, 0, 18)
        };

        var pathLabel = new Label { Text = "DOWNLOAD LOCATION", AutoSize = true, ForeColor = TextMuted, Font = new Font("Segoe UI Semibold", 9F), Margin = new Padding(0, 0, 0, 9) };
        var browse = new Button { Text = "Browse folder", Dock = DockStyle.Fill, Height = 38, Margin = new Padding(10, 0, 0, 0) };
        browse.Click += (_, _) => BrowseForFolder();
        var setup = new Button { Text = "Check folder", Width = 125, Height = 38, Margin = new Padding(0, 10, 0, 0) };
        setup.Click += (_, _) => PrepareFolder();
        var apply = new Button { Text = "Save to Steam", Width = 145, Height = 40, Margin = new Padding(8, 0, 0, 0) };
        apply.Click += (_, _) => ApplyToSteam();
        var copy = new Button { Text = "Copy command", Width = 135, Height = 40, Margin = new Padding(8, 0, 0, 0) };
        copy.Click += (_, _) => CopyCommand();
        var openSteam = new Button { Text = "Open Steam", Width = 115, Height = 40, Margin = new Padding(8, 0, 0, 0) };
        openSteam.Click += (_, _) => OpenSteam();

        var commandLabel = new Label { Text = "STEAM LAUNCH OPTIONS", AutoSize = true, ForeColor = TextMuted, Font = new Font("Segoe UI Semibold", 9F), Margin = new Padding(0, 0, 0, 8) };
        var note = new Label
        {
            Text = "Downloads use an addons subfolder inside the selected folder. Existing mods stay where they are. Steam must be fully exited before saving settings.",
            AutoSize = true, MaximumSize = new Size(700, 0), ForeColor = TextMuted, Margin = new Padding(0, 12, 0, 0)
        };

        var folderCard = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Card, Padding = new Padding(18), ColumnCount = 2, RowCount = 3, Margin = new Padding(0, 0, 0, 12) };
        folderCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        folderCard.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        folderCard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        folderCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        folderCard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        folderCard.Controls.Add(pathLabel, 0, 0); folderCard.SetColumnSpan(pathLabel, 2);
        folderCard.Controls.Add(_pathBox, 0, 1);
        folderCard.Controls.Add(browse, 1, 1);
        folderCard.Controls.Add(setup, 0, 2);

        var commandCard = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Card, Padding = new Padding(18), ColumnCount = 1, RowCount = 2, Margin = new Padding(0) };
        commandCard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        commandCard.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        commandCard.Controls.Add(commandLabel, 0, 0);
        commandCard.Controls.Add(_commandBox, 0, 1);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(26, 22, 26, 14), ColumnCount = 1, RowCount = 6, BackColor = Bg };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(intro, 0, 1);
        layout.Controls.Add(folderCard, 0, 2);
        _status.Text = "Choose a location, then check it or save it in Steam.";
        layout.Controls.Add(_status, 0, 3);
        layout.Controls.Add(commandCard, 0, 4);
        layout.Controls.Add(note, 0, 5);

        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 66, Padding = new Padding(22, 10, 22, 10), FlowDirection = FlowDirection.RightToLeft, BackColor = Card, WrapContents = false };
        footer.Controls.Add(openSteam);
        footer.Controls.Add(copy);
        footer.Controls.Add(apply);
        Controls.Add(layout);
        Controls.Add(footer);
        StyleTextBox(_pathBox, 11F);
        StyleTextBox(_commandBox, 9.5F);
        StyleButton(apply, true);
        StyleButton(copy, false);
        StyleButton(openSteam, false);
        StyleButton(browse, false);
        StyleButton(setup, false);
        UpdateCommand();
        _pathBox.TextChanged += (_, _) => UpdateCommand();
    }

    private static void StyleTextBox(TextBox box, float size)
    {
        box.BackColor = Field;
        box.ForeColor = TextMain;
        box.BorderStyle = BorderStyle.FixedSingle;
        box.Font = new Font("Consolas", size);
        box.Margin = new Padding(0);
    }

    private static void StyleButton(Button button, bool primary)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = primary ? 0 : 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(71, 85, 105);
        button.BackColor = primary ? Accent : Color.FromArgb(51, 65, 85);
        button.ForeColor = primary ? Color.FromArgb(8, 30, 40) : TextMain;
        button.Font = new Font("Segoe UI Semibold", 9.5F);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    private void SetStatus(string message, bool? success = null)
    {
        _status.Text = message;
        _status.ForeColor = success switch
        {
            true => Color.FromArgb(134, 239, 172),
            false => Color.FromArgb(252, 165, 165),
            _ => TextMuted
        };
        _status.BackColor = success switch
        {
            true => Color.FromArgb(22, 51, 42),
            false => Color.FromArgb(59, 31, 43),
            _ => Card
        };
    }

    private void BrowseForFolder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Choose a folder for Reforger's addon downloads", UseDescriptionForTitle = true };
        if (Directory.Exists(_pathBox.Text)) dialog.SelectedPath = _pathBox.Text;
        if (dialog.ShowDialog(this) == DialogResult.OK) _pathBox.Text = dialog.SelectedPath;
    }

    private void PrepareFolder()
    {
        try
        {
            var path = Path.GetFullPath(_pathBox.Text.Trim());
            var root = Path.GetPathRoot(path);
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                throw new DirectoryNotFoundException("That drive is not available. Connect or select a drive shown in File Explorer.");
            Directory.CreateDirectory(path);
            var probe = Path.Combine(path, ".reforger-write-check");
            using (File.Create(probe)) { }
            File.Delete(probe);
            _pathBox.Text = path;
            SetStatus($"Folder is ready and writable: {path}", true);
            UpdateCommand();
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, false);
        }
    }

    private void UpdateCommand()
    {
        var path = _pathBox.Text.Trim();
        _commandBox.Text = string.IsNullOrWhiteSpace(path) ? "" : $"-addonDownloadDir \"{path}\" -addonsDir \"{path}\"";
    }

    private void CopyCommand()
    {
        if (string.IsNullOrWhiteSpace(_commandBox.Text)) return;
        Clipboard.SetText(_commandBox.Text);
        SetStatus("Copied. Paste it into Steam's Launch Options, then restart Reforger through Steam.", true);
    }

    private void OpenSteam()
    {
        try { Process.Start(new ProcessStartInfo("steam://open/games") { UseShellExecute = true }); }
        catch (Exception ex) { SetStatus($"Couldn't open Steam: {ex.Message}", false); }
    }

    private void ApplyToSteam()
    {
        try
        {
            var folder = Path.GetFullPath(_pathBox.Text.Trim());
            var root = Path.GetPathRoot(folder);
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                throw new DirectoryNotFoundException("That drive is not available. Select a drive shown in File Explorer.");
            Directory.CreateDirectory(folder);
            var probe = Path.Combine(folder, ".reforger-write-check");
            using (File.Create(probe)) { }
            File.Delete(probe);

            if (Process.GetProcessesByName("steam").Length > 0)
                throw new InvalidOperationException("Fully exit Steam first (Steam menu → Exit), then reopen this helper and click Set in Steam. Steam can overwrite its config while it is running.");

            var configs = SteamConfigLocator.FindLocalConfigs();
            if (configs.Count == 0)
                throw new FileNotFoundException("Couldn't find Steam's userdata\\<account>\\config\\localconfig.vdf. Open Steam and sign in once, then try again.");

            string selected;
            if (configs.Count == 1) selected = configs[0];
            else
            {
                using var picker = new OpenFileDialog
                {
                    Title = "Choose your Steam account's localconfig.vdf",
                    Filter = "Steam local config (localconfig.vdf)|localconfig.vdf",
                    InitialDirectory = Path.GetDirectoryName(configs[0]),
                    CheckFileExists = true
                };
                if (picker.ShowDialog(this) != DialogResult.OK) return;
                selected = picker.FileName;
                if (!configs.Contains(selected, StringComparer.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Choose one of the localconfig.vdf files found in Steam's userdata folder.");
            }

            var backup = selected + ".backup-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            File.Copy(selected, backup, overwrite: false);
            SteamVdf.SetLaunchOptions(selected, BuildMergedLaunchOptions(folder));
            _pathBox.Text = folder;
            SetStatus("Steam setting saved. Backup created. Reopen Steam, then launch Reforger.", true);
            UpdateCommand();
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, false);
        }
    }

    private static string BuildMergedLaunchOptions(string folder)
    {
        // Existing launch options are read from the chosen VDF by SetLaunchOptions and merged there.
        return $"-addonDownloadDir \"{folder}\" -addonsDir \"{folder}\"";
    }
}

internal static class SteamConfigLocator
{
    public static List<string> FindLocalConfigs()
    {
        var steamRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var keyPath in new[] { @"Software\Valve\Steam", @"Software\WOW6432Node\Valve\Steam" })
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(keyPath);
                var value = key?.GetValue("SteamPath") as string;
                if (!string.IsNullOrWhiteSpace(value)) steamRoots.Add(value.Replace('/', '\\'));
            }
            catch { }
        }
        foreach (var root in new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Steam", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Steam" })
            if (Directory.Exists(root)) steamRoots.Add(root);

        var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in steamRoots)
        {
            var userData = Path.Combine(root, "userdata");
            if (!Directory.Exists(userData)) continue;
            foreach (var account in Directory.EnumerateDirectories(userData))
            {
                var candidate = Path.Combine(account, "config", "localconfig.vdf");
                if (File.Exists(candidate)) files.Add(candidate);
            }
        }
        return files.OrderByDescending(File.GetLastWriteTimeUtc).ToList();
    }
}

internal static class SteamVdf
{
    private sealed record Token(string Value, int Start, int End, bool IsBrace);
    private sealed class Entry
    {
        public required string Key { get; init; }
        public string? Value { get; init; }
        public int ValueToken { get; init; } = -1;
        public int OpenToken { get; init; } = -1;
        public int CloseToken { get; set; } = -1;
        public List<Entry> Children { get; } = new();
    }

    public static void SetLaunchOptions(string file, string requested)
    {
        var original = File.ReadAllText(file, Encoding.UTF8);
        var tokens = Tokenize(original);
        var index = 0;
        var roots = ParseEntries(tokens, ref index, false);
        var userLocal = Find(roots, "UserLocalConfigStore") ?? roots.FirstOrDefault();
        var apps = Follow(userLocal, "Software", "Valve", "Steam", "apps");
        if (apps is null) throw new InvalidDataException("Steam's local config did not contain its expected apps section; no changes were made.");
        var app = Find(apps.Children, "1874880");
        string current = app is null ? "" : Find(app.Children, "LaunchOptions")?.Value ?? "";
        var merged = Regex.Replace(current, @"(?i)(?:^|\s)-(?:addonDownloadDir|addonsDir)(?:\s+""[^""]*""|\s+\S+)?", " ").Trim();
        merged = string.IsNullOrEmpty(merged) ? requested : merged + " " + requested;

        var appToken = app;
        if (appToken is null)
        {
            var insertion = tokens[apps.CloseToken].Start;
            var indent = "\t\t\t\t\t";
            var text = $"{indent}\"1874880\"\n{indent}{{\n{indent}\t\"LaunchOptions\"\t\"{Escape(merged)}\"\n{indent}}}\n";
            original = original.Insert(insertion, text);
        }
        else
        {
            var launch = Find(appToken.Children, "LaunchOptions");
            if (launch is not null)
            {
                var value = tokens[launch.ValueToken];
                original = original[..value.Start] + "\"" + Escape(merged) + "\"" + original[value.End..];
            }
            else
            {
                var insertion = tokens[appToken.CloseToken].Start;
                original = original.Insert(insertion, $"\n\t\t\t\t\t\t\"LaunchOptions\"\t\"{Escape(merged)}\"\n\t\t\t\t\t");
            }
        }
        File.WriteAllText(file, original, new UTF8Encoding(false));
    }

    private static Entry? Follow(Entry? node, params string[] keys)
    {
        foreach (var key in keys) { if (node is null) return null; node = Find(node.Children, key); }
        return node;
    }
    private static Entry? Find(IEnumerable<Entry> entries, string key) => entries.FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
    private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static List<Entry> ParseEntries(List<Token> tokens, ref int i, bool untilBrace)
    {
        var entries = new List<Entry>();
        while (i < tokens.Count)
        {
            if (tokens[i].IsBrace && tokens[i].Value == "}") { if (untilBrace) i++; break; }
            if (tokens[i].IsBrace && tokens[i].Value == "{") { i++; continue; }
            var key = tokens[i++];
            if (key.IsBrace) continue;
            if (i >= tokens.Count) break;
            if (tokens[i].IsBrace && tokens[i].Value == "{")
            {
                var open = i++;
                var entry = new Entry { Key = key.Value, OpenToken = open };
                entry.Children.AddRange(ParseEntries(tokens, ref i, true));
                entry.CloseToken = i - 1;
                entries.Add(entry);
            }
            else
            {
                var valueIndex = i;
                var value = tokens[i++];
                if (!value.IsBrace) entries.Add(new Entry { Key = key.Value, Value = value.Value, ValueToken = valueIndex });
            }
        }
        return entries;
    }

    private static List<Token> Tokenize(string text)
    {
        var list = new List<Token>();
        for (int i = 0; i < text.Length;)
        {
            if (char.IsWhiteSpace(text[i])) { i++; continue; }
            if (text[i] == '/' && i + 1 < text.Length && text[i + 1] == '/')
            { while (i < text.Length && text[i] != '\n') i++; continue; }
            if (text[i] is '{' or '}') { list.Add(new Token(text[i].ToString(), i, i + 1, true)); i++; continue; }
            if (text[i] == '"')
            {
                var start = i++;
                var value = new StringBuilder();
                while (i < text.Length)
                {
                    if (text[i] == '\\' && i + 1 < text.Length && text[i + 1] is '\\' or '"') { value.Append(text[i + 1]); i += 2; continue; }
                    if (text[i] == '"') { i++; break; }
                    value.Append(text[i++]);
                }
                list.Add(new Token(value.ToString(), start, i, false));
                continue;
            }
            var bareStart = i;
            while (i < text.Length && !char.IsWhiteSpace(text[i]) && text[i] is not '{' and not '}') i++;
            list.Add(new Token(text[bareStart..i], bareStart, i, false));
        }
        return list;
    }
}
