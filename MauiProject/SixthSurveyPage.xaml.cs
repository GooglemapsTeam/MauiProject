namespace Emotional_Map;

public partial class SixthSurveyPage : ContentPage
{
	public SixthSurveyPage()
	{
		InitializeComponent();
	}

	public void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
		Preferences.Set("Time", button.Text);
        Navigation.PushAsync(new MainPage());
    }

}