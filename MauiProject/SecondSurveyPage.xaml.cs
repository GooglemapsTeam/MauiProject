namespace Emotional_Map;

public partial class SecondSurveyPage : ContentPage
{
    public SecondSurveyPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var userName = Preferences.Get("Name", "");
        if (!string.IsNullOrEmpty(userName))
        {
            HeaderLabel.Text = $"Привет, {userName}! 👋";
        }
    }

    private async void OnNextClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(ThirdSurveyPage), true);
    }
}
