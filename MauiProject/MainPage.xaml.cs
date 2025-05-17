using Plugin.Maui.Audio;

namespace Emotional_Map;

public partial class MainPage : ContentPage
{
	public MainPage(IAudioManager audioManager)
	{
        InitializeComponent();
        CreatePathCards();
        HeaderLabel.Text = Preferences.Get("Name", "������������");
    }
    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        HeaderLabel.Text = Preferences.Get("Name", "������������");
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ProfileButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(ProfilePage), true);
    }

    private async void OnFavouriteClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(FirstSurveyPage), true);
    }

    private async void OnChangeMoodClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(FirstSurveyPage), true);
    }

    private async void OnUpdatePathesClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(FirstSurveyPage), true);
    }

    public void CreatePathCards()
    {
        MainStack.Children.Clear();
        for (var i = 0; i < 4; i++)
            MainStack.Children.Add(new PathCard(MainStack,
                new Place("������������� ���", "���������� ���� � �������������, ��������� � ������� ������������ � �����������. ������� � 1826 ����, ������ �� ����� ���������� �. �. ����������. � ����� ���� ������������� ����� � ����� ��������� ���������� � ������� ��������-��������. � ����� ����� ����� ����������� ������������ ���������� �� �������������� ������� �������� � ����. �������� ����� �������� �������, ����������� �� ������������� ������� � ������ �����. ������������� ��� �������� �������� ����������� �������� ������� �� ������������ ��������.", "a.jpg"),
                new Place("����-��-�����", "������������ ���� � �������������, ����������� �� ����� ���� ��������, � ������� ����������� ��� ������� � ���� ����������� � ���� �� 17 ���� 1918 ���� ��������� ���������� ��������� ������� II, ��� ����� � ������� ����. ����������� � 2000�2003 �����, �� ���� ������� ������������� ���������������������� �������������, � ����� ������� ������� ������ ������� ������� II � ��� �����, ������������ ������������ ���������� �� ������ �� ������, �� � �� ����� ����.", "b.webp"),
                new Place("���� ������������� ��������", "�������� ���� ��������� ������, � ������� ������ ����������� �������� ����������, ����������� ������������ ����� �����, � ����� ����, �������� ����� � ������ �������.", "c.webp"),
                new Place("��������", "�������� � ��� ������ ������������� � ��� ������. �����-�� �� ���� ����� �������� ����: ������� � ��������� ��������� ������� �.�. �� ������ � ��������������� ������� �.�. �������. ��� � �� ������ ��������� �����, ��� ������� ������� �� ����������� � ���� ������ ������������������� ������, ������ ��� � ���� ������ ��������� � ������ ���� � ����������� ������������. ��� ������ ����� �����, �������� ��� �� ����� �� ���������, ������ ���������, ��������� �������� � ����, � ������ ����� �� �����, ������� ������� �������, ������ �� ������.", "d.jpg")
            ));
    }

    public void GetNewCard(sbyte number)
    {
        // MainStack.Children[number]
        throw new NotImplementedException();
    }


}