using Microsoft.Maui.Controls.Shapes;


namespace Emotional_Map
{
    public class Place
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageSource { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Place(string title, string description, string source) 
        { 
            Title = title;
            Description = description;
            ImageSource = source;

        }
    }
}
