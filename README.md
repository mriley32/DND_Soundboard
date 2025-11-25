# Medieval Soundboard

A simple Windows Forms C# application that plays medieval sounds (sword slash, horse gallop, arrow whizzing, etc.) when you click the corresponding button.

## How to Use

1. Add your own public domain .wav files to the `sounds` folder:
   - `sword.wav` (sword slash)
   - `horse.wav` (horse gallop)
   - `arrow.wav` (arrow whizz)
   - `crowd.wav` (medieval crowd)
   - `trumpet.wav` (trumpet fanfare)

2. Build and run the project in Visual Studio or with the .NET CLI.

3. Click a button to play the sound.

## Notes
- The app uses the `System.Media.SoundPlayer` class, which supports `.wav` files.
- You can find public domain medieval sound effects at sites like [freesound.org](https://freesound.org/) (filter by license) or [OpenGameArt.org](https://opengameart.org/).
- Replace the placeholder files in the `sounds` folder with your own.

## Requirements
- .NET Framework (or .NET Core/5+/6+ with Windows Forms support)
- Windows OS
