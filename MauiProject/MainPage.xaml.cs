namespace Emotional_Map;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
        InitializeComponent();
        CreatePathCards();
        HeaderLabel.Text = Preferences.Get("Name", "������������");
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        HeaderLabel.Text = Preferences.Get("Name", "������������");
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(ProfilePage), true);
    }

    private async void OnFavouriteClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(FirstSurveyPage), true);
    }

    private async void OnChangeMoodClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(FirstSurveyPage), true);
    }

    private async void OnUpdatePathesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//" + nameof(FirstSurveyPage), true);
    }

    public void CreatePathCards()
    {
        MainStack.Children.Clear();
        for (var i = 0; i < 4; i++)
            MainStack.Children.Add(new PathCard(MainStack,
                new Place("�����", "������� ��������", "a.png"),
                new Place("������������", "������ �� 56 �������", "b.png"),
                new Place("�����", "��������������� ����������� �������� ����� �. �. �������", "c.png"),
                new Place("������", "���� �������������", "d.png")
            ));
    }

    public void GetNewCard(sbyte number)
    {
       // MainStack.Children[number]
    }


}