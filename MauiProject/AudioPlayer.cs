using Plugin.Maui.Audio;
using System.Collections.Concurrent;

public static class AudioPlayer
{
    private static readonly ConcurrentDictionary<string, IAudioPlayer> _activePlayers = new();
    private static bool _isDisposed;
    public static bool DoesOn { get; set; }


    public const string ButtonClickSound = "button_click.wav";
    public const string ToPathButtonClickSound = "to_path_button_click.wav";
    public const string SlideSound = "slide.mp3";

    static AudioPlayer()
    {
        DoesOn = Preferences.Get("DoesSoundOn", true);
    }

    public static void PlaySound(string sound)
    {
        if (!DoesOn) return;
        if (_isDisposed)
            throw new InvalidOperationException("AudioPlayer has been disposed");

        Task.Run(async () =>
        {
            try
            {
                if (_activePlayers.TryRemove(sound, out var oldPlayer))
                {
                    oldPlayer.Dispose();
                }

                var audioPlayer = AudioManager.Current.CreatePlayer(
                    await FileSystem.OpenAppPackageFileAsync(sound));

                if (!_activePlayers.TryAdd(sound, audioPlayer))
                {
                    audioPlayer.Dispose();
                    return;
                }

                audioPlayer.PlaybackEnded += OnPlaybackEnded;
                audioPlayer.Play();
            }
            catch (Exception)
            {

            }
        });
    }

    private static void OnPlaybackEnded(object sender, EventArgs e)
    {
        if (sender is IAudioPlayer player)
        {
            var item = _activePlayers.FirstOrDefault(x => x.Value == player);
            if (!string.IsNullOrEmpty(item.Key))
            {
                _activePlayers.TryRemove(item.Key, out _);
                player.Dispose();
            }
        }
    }

    public static void Cleanup()
    {
        if (_isDisposed) return;

        _isDisposed = true;

        foreach (var player in _activePlayers.Values)
        {
            player.Stop();
            player.Dispose();
        }

        _activePlayers.Clear();
    }
}