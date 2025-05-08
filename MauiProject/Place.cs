using Microsoft.Maui.Controls.Shapes;


namespace Emotional_Map
{
    public class Place
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string ImageSource { get; private set; }

        public Place(string title, string description, string source) 
        { 
            Title = title;
            Description = description;
            ImageSource = source;

        }
    }
}
