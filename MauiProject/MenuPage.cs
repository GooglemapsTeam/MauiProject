namespace Emotional_Map;

public partial class MenuPage : ContentPage
{

    public MenuPage()
    {
        InitializeComponent();
        if (!Preferences.ContainsKey("q1") || Preferences.Get("q1", "_") == "_")
        {
            Navigation.RemovePage(this);
            Navigation.PushAsync(new SurveyPage());
        }
        UpdateText();
    }

    public void UpdateText()
    {
        q1.Text = Preferences.Get("q1", "_");
        q2.Text = Preferences.Get("q2", "_");
        q3.Text = Preferences.Get("q3", "_");
        q4.Text = Preferences.Get("q4", "_");
        q5.Text = Preferences.Get("q5", "_");
    }

    private async void OnSurveyClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SurveyPage());

    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        DisplayAlert("Настройки", "Открытие раздела настроек", "OK");
    }


    private async void OnExitClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Выход", "Вы действительно хотите выйти?", "Да", "Нет");
        if (result)
        {
            await Shell.Current.GoToAsync("//login");
        }
    }


}