namespace Emotional_Map;

public partial class ThirdSurveyPage : ContentPage
{
	public ThirdSurveyPage()
	{
		InitializeComponent();
	}

	public async void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        Preferences.Set("CurrentState", button.Text);
        await Shell.Current.GoToAsync("//" + nameof(FourthSurveyPage), true);
    }

}