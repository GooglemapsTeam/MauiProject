using Microsoft.Maui;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace Emotional_Map;

public static class AppImageHelper
{
    public static string CurrentImagePath { get; set; }
    public static ImageSource CachedImage { get; set; }
}

public partial class ProfileImageSetPage : ContentPage
{
    private const string SavedImageKey = "saved_image_path";

    public ProfileImageSetPage()
    {
        InitializeComponent();
        LoadSavedImage();
        NextButton.IsVisible = false;
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        LoadSavedImage();
        NextButton.IsVisible = false;
    }

    private async void OnSkipClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        UpdateProfileImage("profile_button.png");
        await Shell.Current.GoToAsync("//" + nameof(ThirdSurveyPage), true);
    }

    private async void OnChooseFromGalleryClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await SelectAndSavePhoto(false);
    }

    private async void OnTakeSelfieClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await DisplayAlert("Ошибка", "Камера не поддерживается на этом устройстве", "OK");
                return;
            }

            await SelectAndSavePhoto(true);
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Ошибка", "Функция камеры не поддерживается", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Camera error: {ex}");
            await DisplayAlert("Ошибка", $"Не удалось открыть камеру: {ex.Message}", "OK");
        }
    }

    private async Task SelectAndSavePhoto(bool useCamera)
    {
        try
        {
            var status = await CheckAndRequestPermissions(useCamera);
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("Требуется разрешение", "Для продолжения предоставьте необходимые разрешения", "OK");
                return;
            }

            FileResult photo = useCamera ?
                await MediaPicker.CapturePhotoAsync() :
                await MediaPicker.PickPhotoAsync();

            if (photo == null) return;

            string fileName = $"profile_{DateTime.Now.Ticks}{Path.GetExtension(photo.FileName)}";
            string localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            using (var sourceStream = await photo.OpenReadAsync())
            using (var fileStream = File.Create(localPath))
            {
                await sourceStream.CopyToAsync(fileStream);
            }

            UpdateProfileImage(localPath);
            NextButton.IsVisible = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Photo selection error: {ex}");
            await DisplayAlert("Ошибка", $"Не удалось обработать фото: {ex.Message}", "OK");
        }
    }

    private async Task<PermissionStatus> CheckAndRequestPermissions(bool useCamera)
    {
        if (useCamera)
        {
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus != PermissionStatus.Granted)
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

            var photosStatus = await CheckPhotoPermission();

            return (cameraStatus == PermissionStatus.Granted &&
                   photosStatus == PermissionStatus.Granted) ?
                   PermissionStatus.Granted : PermissionStatus.Denied;
        }

        return await CheckPhotoPermission();
    }

    private async Task<PermissionStatus> CheckPhotoPermission()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android &&
            DeviceInfo.Version.Major >= 33)
        {
            return await Permissions.CheckStatusAsync<Permissions.Photos>();
        }

        return PermissionStatus.Granted; 
    }

    private void UpdateProfileImage(string imagePath)
    {
        AppImageHelper.CurrentImagePath = imagePath;
        AppImageHelper.CachedImage = ImageSource.FromFile(imagePath);
        Preferences.Set(SavedImageKey, imagePath);
        profileImage.Source = AppImageHelper.CachedImage;
    }

    private void LoadSavedImage()
    {
        AppImageHelper.CurrentImagePath = Preferences.Get(SavedImageKey, null);

        if (!string.IsNullOrEmpty(AppImageHelper.CurrentImagePath) &&
            File.Exists(AppImageHelper.CurrentImagePath))
        {
            UpdateProfileImage(AppImageHelper.CurrentImagePath);
        }
    }

    private async void OnNextClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(ThirdSurveyPage), true);
    }


}