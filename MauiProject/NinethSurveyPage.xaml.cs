namespace Emotional_Map;

public partial class NinethSurveyPage : ContentPage
{
    public NinethSurveyPage()
    {
        InitializeComponent();
    }
    public async void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        Preferences.Set("MostSafety", button.Text);
        await Shell.Current.GoToAsync("//" + nameof(TenthSurveyPage), true);
    }
}