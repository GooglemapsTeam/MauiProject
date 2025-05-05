namespace Emotional_Map;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    private async void ToPath(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ThirdSurveyPage());
    }

    private async void CrossClick(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ThirdSurveyPage());
    }

    private async void FavouriteClick(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new FirstSurveyPage());
    }
}