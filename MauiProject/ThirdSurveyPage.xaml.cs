namespace Emotional_Map;

public partial class ThirdSurveyPage : ContentPage
{
	public ThirdSurveyPage()
	{
		InitializeComponent();
	}

	public void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
		Preferences.Set("Wish", button.Text);
        Navigation.PushAsync(new FourthSurveyPage());
    }

}