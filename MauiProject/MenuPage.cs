//namespace MenuFormExample;

//public partial class MenuPage : ContentPage
//{
//    // Объявляем событие, которое будет возникать при выборе пункта меню.
//    public event EventHandler<MenuItemEventArgs> MenuItemSelected;

//    public MenuPage()
//    {
//        InitializeComponent();
//    }

//    private void OnOption1Clicked(object sender, EventArgs e)
//    {
//        // Вызываем событие MenuItemSelected с выбранным значением.
//        MenuItemSelected?.Invoke(this, new MenuItemEventArgs { SelectedItem = "Option 1" });
//    }

//    private void OnOption2Clicked(object sender, EventArgs e)
//    {
//        // Вызываем событие MenuItemSelected с выбранным значением.
//        MenuItemSelected?.Invoke(this, new MenuItemEventArgs { SelectedItem = "Option 2" });
//    }

//    private void OnOption3Clicked(object sender, EventArgs e)
//    {
//        // Вызываем событие MenuItemSelected с выбранным значением.
//        MenuItemSelected?.Invoke(this, new MenuItemEventArgs { SelectedItem = "Option 3" });
//    }
//}

//// Класс для передачи данных о выбранном пункте меню.
//public class MenuItemEventArgs : EventArgs
//{
//    public string SelectedItem { get; set; }
//}