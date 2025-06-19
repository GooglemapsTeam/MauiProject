using Emotional_Map.Models;

namespace Emotional_Map.Services
{
    public static class RouteOptimizationService
    {
        public static List<Place> OptimizeRoute(List<Place> places, Location startLocation = null)
        {
            if (places == null || places.Count <= 2)
                return places?.ToList() ?? new List<Place>();

            try
            {
                var start = startLocation ?? new Location(places[0].Latitude, places[0].Longitude);
                var optimizedRoute = new List<Place>();
                var remainingPlaces = places.ToList();
                var currentLocation = start;

                while (remainingPlaces.Any())
                {
                    var nearestPlace = FindNearestPlace(currentLocation, remainingPlaces);
                    optimizedRoute.Add(nearestPlace);
                    remainingPlaces.Remove(nearestPlace);
                    currentLocation = new Location(nearestPlace.Latitude, nearestPlace.Longitude);
                }

                return optimizedRoute;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка оптимизации маршрута: {ex.Message}");
                return places.ToList();
            }
        }

        private static Place FindNearestPlace(Location currentLocation, List<Place> places)
        {
            Place nearestPlace = places[0];
            double minDistance = GetDistance(currentLocation.Latitude, currentLocation.Longitude,
                                           nearestPlace.Latitude, nearestPlace.Longitude);

            foreach (var place in places.Skip(1))
            {
                double distance = GetDistance(currentLocation.Latitude, currentLocation.Longitude,
                                            place.Latitude, place.Longitude);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPlace = place;
                }
            }

            return nearestPlace;
        }

        private static double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static RouteAnalysis AnalyzeRoute(List<Place> places, Location startLocation = null)
        {
            var analysis = new RouteAnalysis();

            if (!places.Any())
                return analysis;

            try
            {
                var totalDistance = 0.0;
                var currentLat = startLocation?.Latitude ?? places[0].Latitude;
                var currentLon = startLocation?.Longitude ?? places[0].Longitude;

                foreach (var place in places)
                {
                    var distance = GetDistance(currentLat, currentLon, place.Latitude, place.Longitude);
                    totalDistance += distance;
                    currentLat = place.Latitude;
                    currentLon = place.Longitude;
                }

                analysis.TotalDistance = Math.Round(totalDistance, 2);
                analysis.EstimatedWalkingTime = (int)(totalDistance * 12);
                analysis.EstimatedVisitTime = places.Count * 60;
                analysis.TotalTime = analysis.EstimatedWalkingTime + analysis.EstimatedVisitTime;

                var districts = places.GroupBy(p => p.District).ToList();
                analysis.DistrictsCount = districts.Count;
                analysis.MostVisitedDistrict = districts.OrderByDescending(g => g.Count()).First().Key;

                var activities = places.GroupBy(p => p.Activity).ToList();
                analysis.ActivitiesCount = activities.Count;
                analysis.MainActivity = activities.OrderByDescending(g => g.Count()).First().Key;

                if (analysis.TotalDistance > 10)
                {
                    analysis.Recommendations.Add("🚌 Рассмотрите использование общественного транспорта для дальних переходов");
                }

                if (analysis.TotalTime > 480)
                {
                    analysis.Recommendations.Add("⏰ Маршрут займет целый день - планируйте перерывы на отдых");
                }

                if (districts.Count > 3)
                {
                    analysis.Recommendations.Add("🗺️ Маршрут охватывает много районов - возможно стоит разделить на несколько дней");
                }

                if (places.Any(p => p.Budget == "Бесплатно"))
                {
                    analysis.Recommendations.Add("💰 В маршруте есть бесплатные места - отличная экономия!");
                }

                return analysis;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка анализа маршрута: {ex.Message}");
                return analysis;
            }
        }
    }

    public class RouteAnalysis
    {
        public double TotalDistance { get; set; }
        public int EstimatedWalkingTime { get; set; }
        public int EstimatedVisitTime { get; set; }
        public int TotalTime { get; set; }
        public int DistrictsCount { get; set; }
        public string MostVisitedDistrict { get; set; } = "";
        public int ActivitiesCount { get; set; }
        public string MainActivity { get; set; } = "";
        public List<string> Recommendations { get; set; } = new List<string>();

        public string GetFormattedTime()
        {
            var hours = TotalTime / 60;
            var minutes = TotalTime % 60;
            return hours > 0 ? $"{hours}ч {minutes}мин" : $"{minutes}мин";
        }

        public string GetFormattedWalkingTime()
        {
            var hours = EstimatedWalkingTime / 60;
            var minutes = EstimatedWalkingTime % 60;
            return hours > 0 ? $"{hours}ч {minutes}мин" : $"{minutes}мин";
        }
    }
}