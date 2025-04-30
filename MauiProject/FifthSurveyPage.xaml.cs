namespace Emotional_Map;

public partial class FifthSurveyPage : ContentPage
{
	public FifthSurveyPage()
	{
		InitializeComponent();
	}

	public void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
		Preferences.Set("Matter", button.Text);
        Navigation.PushAsync(new SixthSurveyPage());
    }

}