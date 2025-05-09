namespace Emotional_Map;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        NameLabel.Text = Preferences.Get("Name", "Пользователь");
    }

    public async void OnSurveyClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(ThirdSurveyPage), true);
    }

    public void OnChangeNameClicked(object sender, EventArgs e)
    {
        EditNameContainer.IsVisible = true;
        NameEntry.Focus(); 
    }

    private async void OnSaveNameClicked(object sender, EventArgs e)
    {
        NameLabel.Text = NameEntry.Text.Trim();
        Preferences.Set("Name", NameLabel.Text);
        EditNameContainer.IsVisible = false;
    }

    private async void OnMainClicked(object sender, EventArgs e)
    {
        Navigation.RemovePage(this);
        await Navigation.PushAsync(new MainPage());
    }
    private async void OnConnectionClicked(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }
}