namespace Emotional_Map
{
    public class Place
    {
        public Place(string v1, string v2, string v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageSource { get; set; }
        public string V1 { get; }
        public string V2 { get; }
        public string V3 { get; }

        public class RouteData
        {
            public List<RoutePoint> Points { get; set; }
            public string RouteType { get; set; } = "pedestrian"; // "pedestrian", "auto", "masstransit"
        }

        public class RoutePoint
        {
            public string Title { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}