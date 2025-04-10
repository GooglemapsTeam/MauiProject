namespace MenuFormExample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnOpenMenuButtonClicked(object sender, EventArgs e)
    {
        // Создаем экземпляр страницы меню.
        var menuPage = new MenuPage();

        // Подписываемся на событие MenuItemSelected, чтобы получить результат.
        menuPage.MenuItemSelected += (s, args) =>
        {
            // Устанавливаем текст в метке на основной странице.
            ResultLabel.Text = $"Вы выбрали: {args.SelectedItem}";

            // Закрываем модальное окно меню.
            Navigation.PopModalAsync();
        };

        // Отображаем страницу меню как модальное окно.
        await Navigation.PushModalAsync(menuPage);
    }
}