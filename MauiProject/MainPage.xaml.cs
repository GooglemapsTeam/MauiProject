
namespace Emotional_Map;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    public void ToPath(object sender, EventArgs e)
    {
        var button = (Button)sender;
        Navigation.PushAsync(new FirstSurveyPage());
    }
}