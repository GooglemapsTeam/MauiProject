namespace Emotional_Map;

public partial class FifthSurveyPage : ContentPage
{
	public FifthSurveyPage()
	{
		InitializeComponent();
	}

	public async void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        Preferences.Set("ImproveMood", button.Text);
        await Shell.Current.GoToAsync("//" + nameof(SixthSurveyPage), true);
    }

}