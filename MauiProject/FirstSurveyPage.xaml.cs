
namespace Emotional_Map;
public partial class FirstSurveyPage : ContentPage
{
    public FirstSurveyPage()
    {
        InitializeComponent();
    }

    private async void OnNextClicked(object sender, EventArgs e)
    {
        Preferences.Set("Name", NameEntry.Text);
        await Shell.Current.GoToAsync("//"+nameof(SecondSurveyPage), true);
    }

    public void NameEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        var isNextButtomEnabled = NameEntry.Text != "";
        NextButton.IsEnabled = isNextButtomEnabled;
        if (isNextButtomEnabled)
            NextButton.TextColor = Color.FromRgb(0, 0, 0);
        else
            NextButton.TextColor = Color.FromHex("#757575");

    }
}