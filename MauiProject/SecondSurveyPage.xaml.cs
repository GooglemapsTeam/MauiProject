namespace Emotional_Map;

public partial class SecondSurveyPage : ContentPage
{
	public SecondSurveyPage()
	{
		InitializeComponent();
	}

    private async void OnNextClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ThirdSurveyPage());
    }

}