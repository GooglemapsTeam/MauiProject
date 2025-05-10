namespace Emotional_Map;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
        InitializeComponent();
        CreatePathCards();
        HeaderLabel.Text = Preferences.Get("Name", "Пользователь");
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        HeaderLabel.Text = Preferences.Get("Name", "Пользователь");
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
                new Place("Радик", "Родился радистом", "a.png"),
                new Place("Новокольцово", "Молись на 56 автобус", "b.png"),
                new Place("Дурка", "Психиатрическая клиническая больница имени Н. М. Кащенко", "c.png"),
                new Place("Могила", "умру программистом", "d.png")
            ));
    }

    public void GetNewCard(sbyte number)
    {
       // MainStack.Children[number]
    }


}