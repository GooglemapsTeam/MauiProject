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
		Preferences.Set("Time", button.Text);
        await Shell.Current.GoToAsync("//" + nameof(MainPage), true);
    }

}