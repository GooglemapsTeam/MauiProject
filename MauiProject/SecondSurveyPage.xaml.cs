namespace Emotional_Map;

public partial class SecondSurveyPage : ContentPage
{
	public SecondSurveyPage()
	{
		InitializeComponent();
        HeaderLabel.Text = String.Format("�����! ������� �������������, {0}! �������� ���� �����", Preferences.Get("Name", "������������"));
	}

    private async void OnNextClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ThirdSurveyPage());
    }

}