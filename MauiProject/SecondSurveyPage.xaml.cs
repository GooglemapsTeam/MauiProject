namespace Emotional_Map;

public partial class SecondSurveyPage : ContentPage
{
	public SecondSurveyPage()
	{
		InitializeComponent();
        HeaderLabel.Text = String.Format("Супер! Приятно познакомиться, {0}! Осталось пару шагов", Preferences.Get("Name", "Пользователь"));
	}

    private async void OnNextClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ThirdSurveyPage());
    }

}