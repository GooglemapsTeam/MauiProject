using Plugin.Maui.Audio;

namespace Emotional_Map
{
    public static class AudioPlayer
    {
        private static IAudioManager _audioManager;
        internal static string SlideSound;

        public static string ButtonClickSound => "button_click.wav";

        public static string ToPathButtonClickSound { get; internal set; }

        static AudioPlayer()
        {
            try
            {
                _audioManager = AudioManager.Current;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации AudioManager: {ex.Message}");
            }
        }

        public static async void PlaySound(string soundFile)
        {
            try
            {
                var soundEnabled = Preferences.Get("SoundEnabled", true);
                if (!soundEnabled) return;

                if (_audioManager != null)
                {
                    var audioPlayer = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(soundFile));
                    audioPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения звука: {ex.Message}");
            }
        }

        internal static void PlaySound(object slideSound)
        {
            throw new NotImplementedException();
        }
    }
}
