namespace Emotional_Map;

public partial class FirstSurveyPage : ContentPage
{
    public FirstSurveyPage()
    {
        InitializeComponent(); // Этот метод генерируется автоматически
    }

    private async void OnNextClicked(object sender, EventArgs e)
    {
        Preferences.Set("Name", NameEntry.Text);
        await Shell.Current.GoToAsync("//" + nameof(SecondSurveyPage), true);
    }

    private void NameEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        var isNextButtonEnabled = !string.IsNullOrEmpty(NameEntry.Text);
        NextButton.IsEnabled = isNextButtonEnabled;
        NextButton.TextColor = isNextButtonEnabled ? Colors.Black : Color.FromHex("#757575");
    }
}