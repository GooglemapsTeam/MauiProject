using Microsoft.Maui.Controls;
using System.Text.Json;

namespace Emotional_Map
{
    public partial class SurveyPage : ContentPage
    {
        private bool q1Answered = false;
        private bool q2Answered = false;
        private bool q3Answered = false;
        private bool q5Answered = false;
        private bool q4Answered = false;

        public static string q1Answer;
        public static string q2Answer;
        public static string q3Answer;
        public static string q4Answer;
        public static string q5Answer;


        public class SurveyResults
        {
            public string Q1 { get; set; }
            public string Q2 { get; set; }
            public string Q3 { get; set; }
            public List<string> Q4 { get; set; }
            public string Q5 { get; set; }
        }

        public SurveyPage()
        {
            InitializeComponent();
        }

        private void OnAnswerSelected(object sender, CheckedChangedEventArgs e)
        {
            var radioButton = (RadioButton)sender;

            switch (radioButton.GroupName)
            {
                case "q1": { q1Answered = e.Value; break; }
                case "q2": q2Answered = e.Value; break;
                case "q3": q3Answered = e.Value; break;
                case "q5": q5Answered = e.Value; break;
            }

            UpdateSubmitButtonState();
        }

        private void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            q4Answered = q4a.IsChecked || q4b.IsChecked ||q4c.IsChecked || q4d.IsChecked || q4e.IsChecked;
            UpdateSubmitButtonState();
        }

        private void UpdateSubmitButtonState()
        {
            SubmitButton.IsEnabled = q1Answered && q2Answered && q3Answered && q4Answered && q5Answered;
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            var results = new
            {
                Q1 = GetSelectedRadio("q1"),
                Q2 = GetSelectedRadio("q2"),
                Q3 = GetSelectedRadio("q3"),
                Q4 = GetSelectedCheckBoxes(),
                Q5 = GetSelectedRadio("q5")
            };
            SubmitButton.IsEnabled = false;
            Navigation.RemovePage(this);

            q1Answer = results.Q1;
            q2Answer = results.Q2;
            q3Answer = results.Q3;
            q4Answer = String.Join(" ", results.Q4);
            q5Answer = results.Q5;

            Preferences.Set("q1", q1Answer);
            Preferences.Set("q2", q2Answer);
            Preferences.Set("q3", q3Answer);
            Preferences.Set("q4", q4Answer);
            Preferences.Set("q5", q5Answer);
            var menuPage = new MenuPage();
            menuPage.UpdateText();
            await Navigation.PushAsync(menuPage);

        }

        private string GetSelectedRadio(string groupName)
        {
            foreach (var view in ((VerticalStackLayout)((ScrollView)Content).Content).Children)
            {
                if (view is RadioButton rb && rb.GroupName == groupName && rb.IsChecked)
                    return rb.Content.ToString();
            }
            return null;
        }

        private List<string> GetSelectedCheckBoxes()
        {
            var selected = new List<string>();
            if (q4a.IsChecked) selected.Add("A. Солнечная погода");
            if (q4b.IsChecked) selected.Add("B. Вкусная еда/напитки");
            if (q4c.IsChecked) selected.Add("C. Музыка/развлечения");
            if (q4d.IsChecked) selected.Add("D. Животные");
            if (q4e.IsChecked) selected.Add("E. Новые впечатления");
            return selected;
        }
    }
}