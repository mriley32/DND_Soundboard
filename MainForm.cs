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
            this.Width = 400;
            this.Height = 300;

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

            var swordButton = new Button
            {
                Text = "Sword Slash",
                Left = 30,
                Top = 30,
                Width = 150,
                Height = 80,
                BackColor = Color.DarkGoldenrod,
                ForeColor = Color.Black,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            //var horseButton = new Button { Text = "Horse Gallop", Left = 30, Top = 80, Width = 150 };
            //var arrowButton = new Button { Text = "Arrow Whizz", Left = 30, Top = 130, Width = 150 };
            //var crowdButton = new Button { Text = "Medieval Crowd", Left = 200, Top = 30, Width = 150 };
            //var trumpetButton = new Button { Text = "Trumpet Fanfare", Left = 200, Top = 80, Width = 150 };

            swordButton.Click += (s, e) => PlaySound("sounds/sword.wav");
            //horseButton.Click += (s, e) => PlaySound("sounds/horse.wav");
            //arrowButton.Click += (s, e) => PlaySound("sounds/arrow.wav");
            //crowdButton.Click += (s, e) => PlaySound("sounds/crowd.wav");
            //trumpetButton.Click += (s, e) => PlaySound("sounds/trumpet.wav");

            Controls.Add(swordButton);
            //Controls.Add(horseButton);
            //Controls.Add(arrowButton);
            //Controls.Add(crowdButton);
            //Controls.Add(trumpetButton);
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
