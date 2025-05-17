using Plugin.Maui.Audio;

namespace Emotional_Map;

public partial class MainPage : ContentPage
{
	public MainPage(IAudioManager audioManager)
	{
        InitializeComponent();
        CreatePathCards();
        HeaderLabel.Text = Preferences.Get("Name", "Пользователь");
    }
    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        HeaderLabel.Text = Preferences.Get("Name", "Пользователь");
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
                new Place("Харитоновский сад", "Английский парк в Екатеринбурге, примыкает к усадьбе Расторгуевых — Харитоновых. Заложен в 1826 году, назван по имени основателя П. Я. Харитонова. В парке есть искусственное озеро с двумя насыпными островками и круглой беседкой-ротондой. В южной части парка сохранилось единственное сооружение из первоначальных садовых построек — грот. Символом парка является ротонда, построенная на искусственном острове в центре пруда. Харитоновский сад является объектом культурного наследия народов РФ федерального значения.", "a.jpg"),
                new Place("Храм-на-Крови", "православный храм в Екатеринбурге, построенный на месте дома Ипатьева, в котором содержались под арестом и были расстреляны в ночь на 17 июля 1918 года последний российский император Николай II, его семья и четверо слуг. Построенный в 2000—2003 годах, он стал главной туристической достопримечательностью Екатеринбурга, а также главным центром памяти святого Николая II и его семьи, привлекающим православных паломников не только из России, но и со всего мира.", "b.webp"),
                new Place("Парк литературного квартала", "Включает пять городских усадеб, в которых сейчас размещаются музейные экспозиции, посвященные литературной жизни Урала, а также парк, Камерный театр и летнюю эстраду.", "c.webp"),
                new Place("Плотинка", "Плотинка – это начало Екатеринбурга и его сердце. Когда-то на реку Исеть приехали двое: инженер и начальник уральских заводов В.И. де Геннин и государственный деятель В.Н. Татищев. Как и на других уральских реках, они возвели плотину из лиственницы и дали начало металлоделательному заводу, открыв его в день Святой Екатерины – вокруг него и образовался Екатеринбург. Тут всегда кипит жизнь, тусуются все от детей до старичков, играют музыканты, продается кукуруза и кофе, в теплое время по пруду, который создала плотина, катают на лодках.", "d.jpg")
            ));
    }

    public void GetNewCard(sbyte number)
    {
        // MainStack.Children[number]
        throw new NotImplementedException();
    }


}