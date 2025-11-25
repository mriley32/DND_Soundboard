using System;
using System.Media;
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

            var swordButton = new Button { Text = "Sword Slash", Left = 30, Top = 30, Width = 150 };
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
    }
}
