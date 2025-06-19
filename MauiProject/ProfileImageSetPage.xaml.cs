namespace Emotional_Map;

public partial class ProfileImageSetPage : ContentPage
{
    private string _selectedImagePath;

    public ProfileImageSetPage()
    {
        InitializeComponent();
    }

    private async void OnChooseFromGalleryClicked(object sender, EventArgs e)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Выберите фото"
            });

            if (result != null)
            {
                _selectedImagePath = result.FullPath;
                profileImage.Source = ImageSource.FromFile(_selectedImagePath);
                NextButton.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выбрать фото: {ex.Message}", "OK");
        }
    }

    private async void OnTakeSelfieClicked(object sender, EventArgs e)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var result = await MediaPicker.CapturePhotoAsync();

            if (result != null)
            {
                _selectedImagePath = result.FullPath;
                profileImage.Source = ImageSource.FromFile(_selectedImagePath);
                NextButton.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось сделать фото: {ex.Message}", "OK");
        }
    }

    private async void OnSkipClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(FourthSurveyPage), true);
    }

    private async void OnNextClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

        if (!string.IsNullOrEmpty(_selectedImagePath))
        {
            Preferences.Set("ProfileImagePath", _selectedImagePath);
        }

        await Shell.Current.GoToAsync("//" + nameof(FourthSurveyPage), true);
    }
}
