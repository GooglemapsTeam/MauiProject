namespace Emotional_Map.Models
{
    public class Place
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string District { get; set; }
        public string Company { get; set; }
        public string Budget { get; set; }
        public string Time { get; set; }
        public string Activity { get; set; }
        public string Mood { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }
        public string ImageSource { get; set; }
        public string Title { get; internal set; }

        public Place(int id, string name, string district, string company, string budget,
                    string time, string activity, string mood, double latitude, double longitude,
                    string description, string imageSource = "placeholder.png")
        {
            Id = id;
            Name = name;
            District = district;
            Company = company;
            Budget = budget;
            Time = time;
            Activity = activity;
            Mood = mood;
            Latitude = latitude;
            Longitude = longitude;
            Description = description;
            ImageSource = imageSource;
        }
    }

    public class RouteData
    {
        public List<Place> Places { get; set; } = new List<Place>();
        public string RouteType { get; set; } = "pedestrian";
        public double TotalDistance { get; set; }
        public int EstimatedTime { get; set; }
    }
}
