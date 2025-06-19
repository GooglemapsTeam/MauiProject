using Emotional_Map.Services;

namespace Emotional_Map;

public partial class EighthSurveyPage : ContentPage
{
    public EighthSurveyPage()
    {
        InitializeComponent();
    }

    public async void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        Preferences.Set("MostSafety", button.Text);

        AppStateService.MarkSurveyCompleted();

        await Shell.Current.GoToAsync("//" + nameof(MainPage), true);
    }
}
