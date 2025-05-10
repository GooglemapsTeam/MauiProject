namespace Emotional_Map;

public partial class SeventhSurveyPage : ContentPage
{
	public SeventhSurveyPage()
	{
		InitializeComponent();
	}
    public async void OnAnswerSelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        Preferences.Set("MostSafety", button.Text);
        await Shell.Current.GoToAsync("//" + nameof(MainPage), true);
    }
}