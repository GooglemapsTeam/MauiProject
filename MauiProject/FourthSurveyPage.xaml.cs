namespace Emotional_Map;

public partial class FourthSurveyPage : ContentPage
{
	public FourthSurveyPage()
	{
		InitializeComponent();
	}

	public void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
		Preferences.Set("Want", button.Text);
        Navigation.PushAsync(new FifthSurveyPage());
    }

}