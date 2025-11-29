using System;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using NAudio.Wave;
using NAudio.MediaFoundation;

namespace MedievalSoundboard
{
    public class MainForm : Form
    {
        // Store references to the 9 custom buttons so we can persist their state
        private readonly System.Collections.Generic.List<Button> customButtons = new System.Collections.Generic.List<Button>();

        // Simple DTO for saving/loading custom slot data
        private class CustomSlot
        {
            public string? Name { get; set; }
            public string? FilePath { get; set; }
        }

        public MainForm()
        {
            this.Text = "Medieval Soundboard";
            this.Width = 420;
            this.Height = 420;

            // Try to set the window icon from a PNG (images/logo.png)
            try
            {
                TrySetIcon("images/logo.png");
            }
            catch { /* ignore icon errors */ }

            // Preload background images so switching pages doesn't re-open files and cause flicker
            Image? bgImage = null;
            Image? animalsImage = null;
            try
            {
                if (File.Exists("images/cobblewall.png"))
                {
                    bgImage = Image.FromFile("images/cobblewall.png");
                    this.BackgroundImage = bgImage;
                    this.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load background image: {ex.Message}");
            }

            // Use a TableLayoutPanel to keep a stable 3-column x 3-row layout
            var tlp = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 3,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(8)
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));

            Button MakeButton(string text, string soundFile)
            {
                var btn = new Button
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(8),
                    BackColor = Color.DarkGoldenrod,
                    ForeColor = Color.Black,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                btn.Click += (s, e) => PlaySound(soundFile);
                return btn;
            }

            Button MakeAnimalButton(string text, string soundFile)
            {
                var btn = new Button
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(8),
                    BackColor = Color.SaddleBrown,
                    ForeColor = Color.White,
                    Font = new Font("Arial", 10, FontStyle.Bold)
                };
                btn.Click += (s, e) => PlaySound(soundFile);
                return btn;
            }

            // Row 0
            var swordButton = MakeButton("Sword Slash", "sounds/presets/sword.wav");
            var barryShortButton = MakeButton("Barry Short Screech", "sounds/presets/monkey_scream_short.wav");
            var barryLongButton = MakeButton("Barry Long Screech", "sounds/presets/monkey_scream_long.wav");

            // Row 1
            var wolfGrowlButton = MakeButton("Wolf Growl", "sounds/presets/wolf_growl.wav");
            var trumpetButton = MakeButton("Trumpet", "sounds/presets/fail_trumpet.wav");
            var arrowButton = MakeButton("Arrow", "sounds/presets/arrow_whizz.wav");

            // Row 2
            var dragonRoarButton = MakeButton("Dragon Roar", "sounds/presets/dragon_roar.wav");
            var dragonBreatheFireButton = MakeButton("Dragon Breathe Fire", "sounds/presets/dragon_breathing_fire.wav");
            var dragonStompButton = MakeButton("Dragon Stomp", "sounds/presets/dragon_stomp.wav");

            tlp.Controls.Add(swordButton, 0, 0);
            tlp.Controls.Add(barryShortButton, 1, 0);
            tlp.Controls.Add(barryLongButton, 2, 0);

            tlp.Controls.Add(wolfGrowlButton, 0, 1);
            tlp.Controls.Add(trumpetButton, 1, 1);
            tlp.Controls.Add(arrowButton, 2, 1);

            tlp.Controls.Add(dragonRoarButton, 0, 2);
            tlp.Controls.Add(dragonBreatheFireButton, 1, 2);
            tlp.Controls.Add(dragonStompButton, 2, 2);

            // Create a top MenuStrip (like VSCode's top menu) and a content panel
            var menu = new MenuStrip { Dock = DockStyle.Top };
            var presetsMenu = new ToolStripMenuItem("Presets");
            var animalsMenu = new ToolStripMenuItem("Animals");
            var customMenu = new ToolStripMenuItem("Custom");
            menu.Items.AddRange(new ToolStripItem[] { presetsMenu, animalsMenu, customMenu });

            // Content panel that will host either the presets layout or the custom page
            var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            // Create panels for Animals and Custom (will reuse the same background if available)
            var animalsPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var customPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            // Apply preloaded background to content/custom panels
            if (bgImage != null)
            {
                contentPanel.BackgroundImage = bgImage;
                contentPanel.BackgroundImageLayout = ImageLayout.Stretch;
                customPanel.BackgroundImage = bgImage;
                customPanel.BackgroundImageLayout = ImageLayout.Stretch;
            }

            // Preload animals background (cartoon grass) and fall back to main bgImage
            try
            {
                if (File.Exists("images/farm_grass.png"))
                {
                    animalsImage = Image.FromFile("images/farm_grass.png");
                    animalsPanel.BackgroundImage = animalsImage;
                    animalsPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
                else if (bgImage != null)
                {
                    animalsPanel.BackgroundImage = bgImage;
                    animalsPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch
            {
                if (bgImage != null)
                {
                    animalsPanel.BackgroundImage = bgImage;
                    animalsPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }

            // Build a 3x3 grid for the Custom page
            var customTlp = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 3,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(8)
            };
            customTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            customTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            customTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            customTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            customTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            customTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));

            // Build a 3x3 grid for the Animals page
            var animalsTlp = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 3,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(8)
            };
            animalsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            animalsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            animalsTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            animalsTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            animalsTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            animalsTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));

            // Animals grid buttons in order: Dog, Meow, Rooster, Lion, Moo, Crickets, Wolf Howl, Horse Gallop, Horse Neigh
            var dogButton = MakeAnimalButton("Dog", "sounds/animals/dog_bark.wav");
            var meowButton = MakeAnimalButton("Meow", "sounds/animals/meow.wav");
            var roosterButton = MakeAnimalButton("Rooster", "sounds/animals/rooster.wav");
            var lionButton = MakeAnimalButton("Lion", "sounds/animals/lion_roar.wav");
            var mooButton = MakeAnimalButton("Moo", "sounds/animals/moo.wav");
            var cricketButton = MakeAnimalButton("Crickets", "sounds/animals/crickets.wav");
            var wolfHowlButton = MakeAnimalButton("Wolf Howl", "sounds/animals/wolf_howl.wav");
            var horseGallopButton = MakeAnimalButton("Horse Gallop", "sounds/animals/horse_gallop.wav");
            var horseNeighButton = MakeAnimalButton("Horse Neigh", "sounds/animals/horse_neigh.wav");

            animalsTlp.Controls.Add(dogButton, 0, 0);
            animalsTlp.Controls.Add(meowButton, 1, 0);
            animalsTlp.Controls.Add(roosterButton, 2, 0);

            animalsTlp.Controls.Add(lionButton, 0, 1);
            animalsTlp.Controls.Add(mooButton, 1, 1);
            animalsTlp.Controls.Add(cricketButton, 2, 1);

            animalsTlp.Controls.Add(wolfHowlButton, 0, 2);
            animalsTlp.Controls.Add(horseGallopButton, 1, 2);
            animalsTlp.Controls.Add(horseNeighButton, 2, 2);

            animalsPanel.Controls.Add(animalsTlp);

            // Add the three major pages to the content panel once, and show/hide them
            contentPanel.Controls.Add(tlp);
            contentPanel.Controls.Add(animalsPanel);
            contentPanel.Controls.Add(customPanel);

            // Enable double-buffering on panels to reduce flicker
            void EnableDoubleBuffer(Control ctrl)
            {
                try
                {
                    var prop = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    prop?.SetValue(ctrl, true, null);
                }
                catch { }
            }
            EnableDoubleBuffer(contentPanel);
            EnableDoubleBuffer(tlp);
            EnableDoubleBuffer(animalsPanel);
            EnableDoubleBuffer(customPanel);

            // Create 9 placeholder buttons for custom sounds
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var btn = new Button
                    {
                        Text = "Click to add sound",
                        Dock = DockStyle.Fill,
                        Margin = new Padding(8),
                        BackColor = Color.DarkGoldenrod,
                        ForeColor = Color.Black,
                        Font = new Font("Arial", 9, FontStyle.Regular),
                        Tag = null // will store the file path when set
                    };

                    // Add a context menu so right-click can Replace, Rename or Clear the assigned sound (only for custom buttons)
                    var cms = new ContextMenuStrip();
                    var replaceItem = new ToolStripMenuItem("Replace/Set...");
                    replaceItem.Click += (sender, ev) =>
                    {
                        using (var ofd = new OpenFileDialog())
                        {
                            ofd.Filter = "Audio files|*.wav;*.mp3|WAV files|*.wav|MP3 files|*.mp3|All files|*.*";
                            ofd.Title = "Select an audio file to assign";
                            if (ofd.ShowDialog(this) == DialogResult.OK)
                            {
                                btn.Tag = ofd.FileName;
                                btn.Text = Path.GetFileNameWithoutExtension(ofd.FileName);
                                SaveCustomConfig();
                            }
                        }
                    };

                    var renameItem = new ToolStripMenuItem("Rename...");
                    renameItem.Click += (sender, ev) =>
                    {
                        // Show a small dialog to rename the display name without changing the assigned file
                        var current = btn.Text ?? string.Empty;
                        using (var input = new Form())
                        {
                            input.Text = "Rename Sound";
                            input.FormBorderStyle = FormBorderStyle.FixedDialog;
                            input.StartPosition = FormStartPosition.CenterParent;
                            input.MinimizeBox = false;
                            input.MaximizeBox = false;
                            input.ClientSize = new Size(360, 100);

                            var lbl = new Label { Text = "Name:", Left = 8, Top = 12, Width = 50 };
                            var txt = new TextBox { Left = 70, Top = 8, Width = 270, Text = current };
                            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 190, Top = 40, Width = 70 };
                            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 270, Top = 40, Width = 70 };

                            input.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
                            input.AcceptButton = ok;
                            input.CancelButton = cancel;

                            if (input.ShowDialog(this) == DialogResult.OK)
                            {
                                var newName = txt.Text.Trim();
                                if (!string.IsNullOrWhiteSpace(newName))
                                {
                                    btn.Text = newName;
                                    SaveCustomConfig();
                                }
                                else
                                {
                                    MessageBox.Show(this, "Name cannot be empty.", "Invalid name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    };

                    var clearItem = new ToolStripMenuItem("Clear");
                    clearItem.Click += (sender, ev) =>
                    {
                        btn.Text = "Click to add sound";
                        btn.Tag = null;
                        SaveCustomConfig();
                    };

                    var clearAllItem = new ToolStripMenuItem("Clear All Buttons");
                    clearAllItem.Click += (sender, ev) =>
                    {
                        var resp = MessageBox.Show(this, "Clear all custom buttons? This will remove all assigned sounds.", "Confirm Clear All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (resp == DialogResult.Yes)
                        {
                            foreach (var cbt in customButtons)
                            {
                                cbt.Text = "Click to add sound";
                                cbt.Tag = null;
                            }
                            SaveCustomConfig();
                        }
                    };

                    cms.Items.AddRange(new ToolStripItem[] { replaceItem, renameItem, clearItem, new ToolStripSeparator(), clearAllItem });
                    cms.Opening += (sender, ev) =>
                    {
                        // Enable Rename and Clear only when there's an assigned file; Replace is always enabled
                        var hasFile = !string.IsNullOrEmpty(btn.Tag as string);
                        renameItem.Enabled = hasFile;
                        clearItem.Enabled = hasFile;

                        // Enable Clear All only when any custom button has an assigned file
                        bool anyAssigned = false;
                        foreach (var cbt in customButtons)
                        {
                            if (!string.IsNullOrEmpty(cbt.Tag as string))
                            {
                                anyAssigned = true;
                                break;
                            }
                        }
                        clearAllItem.Enabled = anyAssigned;
                    };
                    btn.ContextMenuStrip = cms;

                    btn.Click += (s, e) =>
                    {
                        var b = s as Button;
                        if (b == null)
                            return;

                        string assignedPath = b.Tag as string ?? string.Empty;
                        if (!string.IsNullOrEmpty(assignedPath))
                        {
                            // Play the assigned sound
                            PlaySound(assignedPath);
                            return;
                        }

                        // Prompt user for name and WAV file
                        if (PromptForSound(out string soundName, out string soundPath))
                        {
                            b.Text = soundName;
                            b.Tag = soundPath;
                            // Persist immediately when the user assigns a new custom sound
                            SaveCustomConfig();
                        }
                    };

                    customTlp.Controls.Add(btn, c, r);
                    // track button for persistence (order is row-major)
                    customButtons.Add(btn);
                }
            }

            customPanel.Controls.Add(customTlp);

            // Load saved custom assignments (if any)
            LoadCustomConfig();

            // Show presets by default; hide others
            tlp.Visible = true;
            animalsPanel.Visible = false;
            customPanel.Visible = false;

            // Also enable double-buffering for the inner TableLayoutPanels to reduce redraw flicker
            EnableDoubleBuffer(animalsTlp);
            EnableDoubleBuffer(customTlp);

            // Force creation of control handles for the content subtree so switching pages doesn't incur first-time handle creation delays
            void EnsureHandles(Control ctrl)
            {
                try { ctrl.CreateControl(); } catch { }
                foreach (Control ch in ctrl.Controls)
                {
                    EnsureHandles(ch);
                }
            }
            EnsureHandles(contentPanel);

            // Menu actions switch the visible page without recreating controls
            presetsMenu.Click += (s, e) =>
            {
                tlp.Visible = true; animalsPanel.Visible = false; customPanel.Visible = false;
                tlp.BringToFront();
            };
            animalsMenu.Click += (s, e) =>
            {
                tlp.Visible = false; animalsPanel.Visible = true; customPanel.Visible = false;
                animalsPanel.BringToFront();
            };
            customMenu.Click += (s, e) =>
            {
                tlp.Visible = false; animalsPanel.Visible = false; customPanel.Visible = true;
                customPanel.BringToFront();
            };

            Controls.Add(contentPanel);
            Controls.Add(menu);

            // Ensure custom assignments are saved when the form closes
            this.FormClosing += (s, e) => SaveCustomConfig();
        }

        private async void PlaySound(string filePath)
        {
            await Task.Run(() =>
            {
                try
                {
                    // Use NAudio for all audio formats (MP3, WAV, ADPCM, etc.)
                    using (var reader = new MediaFoundationReader(filePath))
                    using (var output = new WaveOutEvent())
                    {
                        output.Init(reader);
                        output.Play();
                        // Wait for playback to complete
                        while (output.PlaybackState == PlaybackState.Playing)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Can't show message box directly from background thread; marshal to UI thread
                    this.Invoke((Action)(() => MessageBox.Show($"Could not play sound: {filePath}\n{ex.Message}")));
                }
            });
        }

        // Shows a small dialog to ask the user for a display name and a .wav file path.
        // Returns true and fills out parameters if the user confirmed, otherwise false.
        private bool PromptForSound(out string soundName, out string soundPath)
        {
            soundName = string.Empty;
            soundPath = string.Empty;

            using (var dlg = new Form())
            {
                dlg.Text = "Add Sound";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ClientSize = new Size(480, 160);

                var lblName = new Label { Text = "Name:", Left = 12, Top = 16, Width = 50 };
                var txtName = new TextBox { Left = 70, Top = 12, Width = 390 };

                var lblFile = new Label { Text = "File:", Left = 12, Top = 52, Width = 50 };
                var txtFile = new TextBox { Left = 70, Top = 48, Width = 310, ReadOnly = true };
                var btnBrowse = new Button { Text = "Browse...", Left = 390, Top = 46, Width = 70 };

                var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 300, Width = 80, Top = 100 };
                var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 390, Width = 80, Top = 100 };

                btnBrowse.Click += (s, e) =>
                {
                    using (var ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "Audio files|*.wav;*.mp3|WAV files|*.wav|MP3 files|*.mp3|All files|*.*";
                        ofd.Title = "Select an audio file";
                        if (ofd.ShowDialog(dlg) == DialogResult.OK)
                        {
                            txtFile.Text = ofd.FileName;
                        }
                    }
                };

                btnOk.Click += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        MessageBox.Show(dlg, "Please enter a name for the sound.", "Missing name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        dlg.DialogResult = DialogResult.None;
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtFile.Text) || !System.IO.File.Exists(txtFile.Text))
                    {
                        MessageBox.Show(dlg, "Please choose an existing .wav file.", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        dlg.DialogResult = DialogResult.None;
                        return;
                    }
                };

                dlg.Controls.AddRange(new Control[] { lblName, txtName, lblFile, txtFile, btnBrowse, btnOk, btnCancel });
                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancel;

                var result = dlg.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    soundName = txtName.Text.Trim();
                    soundPath = txtFile.Text.Trim();
                    return true;
                }
            }

            return false;
        }

        // Persistence: save custom button assignments to AppData
        private string GetConfigPath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DND_Soundboard");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "custom.json");
        }

        private void SaveCustomConfig()
        {
            try
            {
                var slots = new System.Collections.Generic.List<CustomSlot>();
                foreach (var b in customButtons)
                {
                    var fp = b.Tag as string;
                    slots.Add(new CustomSlot { Name = string.IsNullOrWhiteSpace(fp) ? null : b.Text, FilePath = fp });
                }
                var json = JsonSerializer.Serialize(slots, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GetConfigPath(), json);
            }
            catch
            {
                // ignore save errors
            }
        }

        private void LoadCustomConfig()
        {
            try
            {
                var path = GetConfigPath();
                if (!File.Exists(path)) return;
                var json = File.ReadAllText(path);
                var slots = JsonSerializer.Deserialize<System.Collections.Generic.List<CustomSlot>>(json);
                if (slots == null) return;
                for (int i = 0; i < Math.Min(slots.Count, customButtons.Count); i++)
                {
                    var s = slots[i];
                    if (s != null && !string.IsNullOrWhiteSpace(s.FilePath) && File.Exists(s.FilePath))
                    {
                        var b = customButtons[i];
                        b.Text = string.IsNullOrWhiteSpace(s.Name) ? Path.GetFileNameWithoutExtension(s.FilePath) : s.Name;
                        b.Tag = s.FilePath;
                    }
                }
            }
            catch
            {
                // ignore load errors
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        private void TrySetIcon(string pngPath)
        {
            try
            {
                if (!System.IO.File.Exists(pngPath))
                    return;

                using (var bmp = new Bitmap(pngPath))
                {
                    IntPtr hIcon = bmp.GetHicon();
                    try
                    {
                        using (var tmp = Icon.FromHandle(hIcon))
                        {
                            this.Icon = (Icon)tmp.Clone();
                        }
                    }
                    finally
                    {
                        DestroyIcon(hIcon);
                    }
                }
            }
            catch
            {
                // ignore errors setting icon
            }
        }
    }
}
