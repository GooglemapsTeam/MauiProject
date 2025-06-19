using Emotional_Map.Models;
using Emotional_Map.Services;

namespace Emotional_Map;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProfile();
        LoadStatistics();
    }

    private void LoadProfile()
    {
        var userName = Preferences.Get("Name", "Пользователь");
        NameLabel.Text = userName;

        var profileImagePath = Preferences.Get("ProfileImagePath", "");
        if (!string.IsNullOrEmpty(profileImagePath))
        {
            ProfileImage.Source = profileImagePath;
        }
        else
        {
            ProfileImage.Source = "profile_button.png";
        }
        var soundEnabled = Preferences.Get("SoundEnabled", true);
        SoundButton.Text = soundEnabled ? "Звук включен" : "Звук выключен";
    }

    private void LoadStatistics()
    {
        try
        {
            var favoritePlaces = Preferences.Get("FavoritePlaces", "");
            var favoritePlacesCount = string.IsNullOrEmpty(favoritePlaces) ? 0 :
                                    favoritePlaces.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
            FavoritePlacesCountLabel.Text = favoritePlacesCount.ToString();
            var firstUseDate = Preferences.Get("FirstUseDate", DateTime.Now.ToString());
            if (DateTime.TryParse(firstUseDate, out var firstUse))
            {
                var daysUsing = (DateTime.Now - firstUse).Days + 1;
                DaysCountLabel.Text = daysUsing.ToString();
            }
            else
            {
                Preferences.Set("FirstUseDate", DateTime.Now.ToString());
                DaysCountLabel.Text = "1";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки статистики: {ex.Message}");
        }
    }

    private void OnChangeNameClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        EditNameContainer.IsVisible = true;
        NameEntry.Text = NameLabel.Text;
        NameEntry.Focus();
    }

    private void OnSaveNameClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

        if (!string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            Preferences.Set("Name", NameEntry.Text);
            NameLabel.Text = NameEntry.Text;
        }

        EditNameContainer.IsVisible = false;
    }

    private async void OnSurveyClicked(object sender, EventArgs e)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var result = await DisplayAlert("Пройти опрос заново",
                "Вы хотите пройти опрос заново? Это поможет получить новые персонализированные рекомендации.",
                "Да, пройти опрос", "Отмена");

            if (result)
            {
                var clearAnswers = await DisplayAlert("Очистить ответы",
                    "Хотите начать опрос с чистого листа или сохранить предыдущие ответы?",
                    "Начать заново", "Сохранить ответы");

                if (clearAnswers)
                {
                    AppStateService.ResetSurvey();
                }

                await Shell.Current.GoToAsync("//" + nameof(FirstSurveyPage), true);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось перейти к опросу: {ex.Message}", "OK");
        }
    }

    private void OnSoundClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

        var soundEnabled = Preferences.Get("SoundEnabled", true);
        soundEnabled = !soundEnabled;
        Preferences.Set("SoundEnabled", soundEnabled);

        SoundButton.Text = soundEnabled ? "Звук включен" : "Звук выключен";
    }

    private async void OnImageClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(ProfileImageSetPage), true);
    }

    private async void OnMainClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(MainPage), true);
    }
}
