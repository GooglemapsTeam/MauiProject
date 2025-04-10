namespace MauiProject
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent(); // Вызов в конструкторе
                                   // Дополнительный код инициализации, если необходимо (например, настройка ViewModel)
        }

        public object CounterBtn { get; private set; }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
