
using System.Xml;

namespace Emotional_Map
{
    public partial class FirstSurveyPage : ContentPage
    {

        public FirstSurveyPage()
        {
            if (Preferences.ContainsKey("Name"))
                Navigation.PushAsync(new MenuPage());
            InitializeComponent();
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            Preferences.Set("Name", NameEntry.Text);
            await Navigation.PushAsync(new MenuPage());
        }

        public void NameEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            NextButton.IsEnabled = NameEntry.Text != "";
        }

    }
}