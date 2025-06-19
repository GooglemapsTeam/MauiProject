using Emotional_Map.Services;
using System.Text.Json;

namespace Emotional_Map.Models
{
    public class FavoriteRoute
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Place> Places { get; set; } = new List<Place>();
        public double TotalDistance { get; set; }
        public int EstimatedTime { get; set; }
        public string RouteType { get; set; } = "pedestrian";

        public FavoriteRoute()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
        }

        public FavoriteRoute(string name, string description, List<Place> places) : this()
        {
            Name = name;
            Description = description;
            Places = places ?? new List<Place>();

            var routeData = RecommendationService.GenerateRoute(Places);
            TotalDistance = routeData.TotalDistance;
            EstimatedTime = routeData.EstimatedTime;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static FavoriteRoute FromJson(string json)
        {
            return JsonSerializer.Deserialize<FavoriteRoute>(json);
        }
    }
}
