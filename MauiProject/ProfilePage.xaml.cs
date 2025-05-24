using Plugin.Maui.Audio;

namespace Emotional_Map;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        NameLabel.Text = Preferences.Get("Name", "������������");
        SoundButton.Text = AudioPlayer.DoesOn ? "��������� ����" : "�������� ����";
    }

    public async void OnSurveyClicked(object sender, EventArgs e)
    {
        NameEntry.Unfocus();
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
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
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        NameEntry.Unfocus();
    }

    private async void OnSoundClicked(object sender, EventArgs e)
    {
        AudioPlayer.DoesOn = !AudioPlayer.DoesOn;
        SoundButton.Text = AudioPlayer.DoesOn ? "��������� ����" : "�������� ����";
        Preferences.Set("DoesSoundOn", AudioPlayer.DoesOn);
        NameEntry.Unfocus();
        Navigation.RemovePage(this);
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
    }

    private async void OnMainClicked(object sender, EventArgs e)
    {
        NameEntry.Unfocus();
        Navigation.RemovePage(this);
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Navigation.PushAsync(new MainPage());
    }

    private async void OnConnectionClicked(object sender, EventArgs e)
    {
        NameEntry.Unfocus();
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        throw new NotImplementedException();
    }
}