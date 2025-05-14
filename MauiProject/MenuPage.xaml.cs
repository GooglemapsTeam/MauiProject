namespace Emotional_Map;

public partial class MenuPage : ContentPage
{

    public MenuPage()
    {
        InitializeComponent();
    }


    private async void OnSurveyClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new FirstSurveyPage());

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