using System;
using System.Drawing;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MedievalSoundboard
{
    public class MainForm : Form
    {
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

            // Set background image (simple relative path)
            try
            {
                this.BackgroundImage = Image.FromFile("images/cobblewall.png");
                this.BackgroundImageLayout = ImageLayout.Stretch;
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

            // Row 0
            var swordButton = MakeButton("Sword Slash", "sounds/sword.wav");
            var barryShortButton = MakeButton("Barry Short Screech", "sounds/monkey_scream_short.wav");
            var barryLongButton = MakeButton("Barry Long Screech", "sounds/monkey_scream_long.wav");

            // Row 1
            var wolfGrowlButton = MakeButton("Wolf Growl", "sounds/wolf_growl.wav");
            var trumpetButton = MakeButton("Trumpet", "sounds/fail_trumpet.wav");
            var arrowButton = MakeButton("Arrow", "sounds/arrow_whizz.wav");

            // Row 2
            var dragonRoarButton = MakeButton("Dragon Roar", "sounds/dragon_roar.wav");
            var dragonBreatheFireButton = MakeButton("Dragon Breathe Fire", "sounds/dragon_breathing_fire.wav");
            var dragonStompButton = MakeButton("Dragon Stomp", "sounds/dragon_stomp.wav");

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
            var customMenu = new ToolStripMenuItem("Custom");
            menu.Items.AddRange(new ToolStripItem[] { presetsMenu, customMenu });

            // Content panel that will host either the presets layout or the custom page
            var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            // Create an empty custom panel (will reuse the same background if available)
            var customPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            if (this.BackgroundImage != null)
            {
                contentPanel.BackgroundImage = this.BackgroundImage;
                contentPanel.BackgroundImageLayout = ImageLayout.Stretch;
                customPanel.BackgroundImage = this.BackgroundImage;
                customPanel.BackgroundImageLayout = ImageLayout.Stretch;
            }

            // Put the presets layout into the content panel by default
            contentPanel.Controls.Add(tlp);

            // Menu actions switch the visible page
            presetsMenu.Click += (s, e) =>
            {
                contentPanel.Controls.Clear();
                contentPanel.Controls.Add(tlp);
            };
            customMenu.Click += (s, e) =>
            {
                contentPanel.Controls.Clear();
                contentPanel.Controls.Add(customPanel);
            };

            Controls.Add(contentPanel);
            Controls.Add(menu);
        }

        private void PlaySound(string filePath)
        {
            try
            {
                using (var player = new SoundPlayer(filePath))
                {
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not play sound: {filePath}\n{ex.Message}");
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
