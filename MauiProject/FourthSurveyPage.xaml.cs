namespace Emotional_Map;

public partial class FourthSurveyPage : ContentPage
{
	public FourthSurveyPage()
	{
		InitializeComponent();
	}

	public async void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        Preferences.Set("Matter", button.Text);
        await Shell.Current.GoToAsync("//" + nameof(FifthSurveyPage), true);
    }

}