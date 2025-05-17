namespace Emotional_Map;

public partial class SixthSurveyPage : ContentPage
{
	public SixthSurveyPage()
	{
		InitializeComponent();
	}

	public async void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        Preferences.Set("MostImpact", button.Text);
        await Shell.Current.GoToAsync("//" + nameof(SeventhSurveyPage), true);
    }

}