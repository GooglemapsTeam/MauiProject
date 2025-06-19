using Emotional_Map.Models;

namespace Emotional_Map.Services
{
    public static class RecommendationService
    {
        private const int MIN_SCORE_THRESHOLD = 3;
        private const int MAX_RECOMMENDATIONS = 5;
        private const int MOOD_SCORE_WEIGHT = 5;
        private const int ACTIVITY_SCORE_WEIGHT = 3;
        private const int COMPANY_SCORE_WEIGHT = 2;
        private const int TIME_SCORE_WEIGHT = 1;
        private const int BUDGET_SCORE_WEIGHT = 1;
        private const int DISTRICT_SCORE_WEIGHT = 1;

        private static readonly List<Place> _places = new List<Place>
        {
            new Place(3, "Арт-пространство \"Дом Метенкова\"", "Центр", "Друзья", "До 1000 руб.", "1–3 часа", "Культура/искусство", "Хорошее", 56.843111, 60.614152, "Культурное пространство с выставками"),
            new Place(5, "Уличные граффити на Плотинке", "Центр", "Пара", "Бесплатно", "До 1 часа", "Прогулка", "Нейтральное", 56.835333, 60.605628, "Прогулка по историческому центру с граффити"),
            new Place(12, "Библиотека Белинского", "Центр", "Один/одна", "Бесплатно", "1–3 часа", "Культура/искусство", "Усталость", 56.834342, 60.615269, "Главная библиотека города"),
            new Place(15, "ТЦ \"Гринвич\"", "Центр", "Семья", "Не важно", "Целый день", "Шопинг", "Хорошее", 56.829343, 60.599432, "Крупный торговый центр"),
            new Place(17, "Театр оперы и балета", "Центр", "Пара", "1000–3000 руб.", "1–3 часа", "Культура/искусство", "Хорошее", 56.840259, 60.617100, "Главный театр города"),
            new Place(19, "Кинотеатр \"Салют\"", "Центр", "Друзья", "До 1000 руб.", "1–3 часа", "Еда/напитки", "Нейтральное", 56.840497, 60.610211, "Современный кинотеатр"),
            new Place(21, "Музей изобразительных искусств", "Центр", "Один/одна", "До 1000 руб.", "1–3 часа", "Культура/искусство", "Нейтральное", 56.835421, 60.603392, "Художественный музей"),
            new Place(22, "Ресторан \"Демидовъ\"", "Центр", "Пара", "1000–3000 руб.", "1–3 часа", "Еда/напитки", "Хорошее", 56.846114, 60.5896711, "Ресторан русской кухни"),
            new Place(24, "Торговый центр \"Пассаж\"", "Центр", "Семья", "Не важно", "1–3 часа", "Шопинг", "Нейтральное", 56.836862, 60.595549, "Исторический торговый центр"),
            new Place(25, "Дендропарк", "Центр", "Семья", "Бесплатно", "1–3 часа", "Прогулка", "Усталость", 56.830966, 60.603877, "Парк"),
            new Place(44, "Кафе \"Высота5642\"", "Центр", "Пара", "До 1000 руб.", "1–3 часа", "Еда/напитки", "Хорошее", 56.835256, 60.601900, "Кафе с видом на город"),
            new Place(10, "Парк \"Зелёная роща\"", "Центр", "Один/одна", "Бесплатно", "До 1 часа", "Прогулка", "Усталость", 56.822485, 60.594769, "Тихий парк для отдыха"),
            new Place(35, "Парк \"Победы\"", "Железнодорожный район", "Семья", "Бесплатно", "1–3 часа", "Прогулка", "Хорошее", 56.906503, 60.565985, "Мемориальный парк с памятниками"),
            new Place(36, "Кинотеатр \"Космос\"", "Железнодорожный район", "Друзья", "До 1000 руб.", "1–3 часа", "Еда/напитки", "Нейтральное", 56.846419, 60.604831, "Районный кинотеатр"),
            new Place(2, "Боулинг \"Фортуна\"", "Ленинский район", "Друзья", "1000–3000 руб.", "1–3 часа", "Активный отдых", "Хорошее", 56.803794, 60.597690, "Современный боулинг-центр"),
            new Place(18, "Парк им. Маяковского", "Ленинский район", "Семья", "Бесплатно", "1–3 часа", "Прогулка", "Хорошее", 56.817357, 60.645133, "Центральный парк для отдыха"),
            new Place(13, "Ресторан \"Паштет\"", "Ленинский район", "Пара", "1000–3000 руб.", "1–3 часа", "Еда/напитки", "Хорошее", 56.842180, 60.609021, "Изысканная кухня для романтического ужина"),
            new Place(33, "Спортивный комплекс \"Динамо\"", "Ленинский район", "Друзья", "До 1000 руб.", "1–3 часа", "Активный отдых", "Плохое", 56.847584, 60.599027, "Спортивный комплекс с различными секциями"),
            new Place(34, "Библиотека им. Горького", "Ленинский район", "Один/одна", "Бесплатно", "1–3 часа", "Культура/искусство", "Усталость", 56.890954, 60.599686, "Районная библиотека с читальным залом"),
            new Place(46, "Кинотеатр \"Киномакс\"", "Ленинский район", "Друзья", "До 1000 руб.", "1–3 часа", "Еда/напитки", "Нейтральное", 56.810865, 60.608474, "Современный многозальный кинотеатр"),
            new Place(4, "Музей Бориса Ельцина", "Верх-Исетский район", "Один/одна", "До 1000 руб.", "1–3 часа", "Культура/искусство", "Нейтральное", 56.844647, 60.591559, "Современный музей истории России"),
            new Place(31, "ТЦ \"Алатырь\"", "Верх-Исетский район", "Семья", "Не важно", "1–3 часа", "Шопинг", "Нейтральное", 56.833656, 60.582202, "Торговый центр в Верх-Исетском районе"),
            new Place(39, "Спортивный комплекс \"Юность\"", "Верх-Исетский район", "Друзья", "До 1000 руб.", "1–3 часа", "Активный отдых", "Плохое", 56.825689, 60.598759, "Спортивный комплекс с залами"),
            new Place(45, "ТЦ \"Мега\"", "Верх-Исетский район", "Семья", "Не важно", "Целый день", "Шопинг", "Хорошее", 56.823818, 60.505507, "Крупнейший торговый центр"),
            new Place(48, "Боулинг \"Bowling Xl\"", "Верх-Исетский район", "Друзья", "1000–3000 руб.", "1–3 часа", "Активный отдых", "Хорошее", 56.839286, 60.616180, "Боулинг-центр с баром"),
            new Place(6, "Квест \"Секретная лаборатория\"", "Кировский район", "Друзья", "1000–3000 руб.", "1–3 часа", "Активный отдых", "Нейтральное", 56.833427, 60.629271, "Увлекательный квест-рум"),
            new Place(32, "Кафе \"Встреча\"", "Кировский район", "Пара", "До 1000 руб.", "1–3 часа", "Еда/напитки", "Хорошее", 56.839085, 60.656865, "Уютное кафе для свиданий"),
            new Place(28, "Ресторан \"Троекуров\"", "Кировский район", "Друзья", "1000–3000 руб.", "1–3 часа", "Еда/напитки", "Хорошее", 56.846002, 60.662735, "Ресторан европейской кухни"),
            new Place(23, "Парк \"Харитоновский\"", "Кировский район", "Пара", "Бесплатно", "До 1 часа", "Прогулка", "Усталость", 56.846857, 60.613336, "Исторический парк с прудом"),
            new Place(26, "Аквапарк \"Лимпопо\"", "Чкаловский район", "Семья", "1000–3000 руб.", "Целый день", "Активный отдых", "Хорошее", 56.792975, 60.644785, "Крупный аквапарк"),
            new Place(50, "Спортивный комплекс \"Орджо\"", "Орджоникидзевский район", "Один/одна", "До 1000 руб.", "1–3 часа", "Активный отдых", "Плохое", 56.910147, 60.627701, "Спортивный комплекс с тренажерным залом"),
            new Place(38, "Парк \"Уралмаш\"", "Орджоникидзевский район", "Семья", "Бесплатно", "1–3 часа", "Прогулка", "Усталость", 56.888375, 60.601504, "Районный парк для отдыха")
        };

        private static readonly Dictionary<string, string> MoodMapping = new Dictionary<string, string>
        {
            { "Чувствую усталость и упадок сил 😴", "Усталость" },
            { "Чувствую тревогу и беспокойство 😟", "Плохое" },
            { "Чувствую скуку и апатию 🥱", "Нейтральное" },
            { "Чувствую раздражение и злость 😠", "Плохое" },
            { "Хорошее", "Хорошее" },
            { "Нейтральное", "Нейтральное" },
            { "Плохое", "Плохое" },
            { "Усталость", "Усталость" }
        };

        public static List<Place> GetAllPlaces()
        {
            return _places.ToList();
        }

        public static List<Place> GetRecommendations()
        {
            var userPreferences = GetUserPreferences();
            var mappedMood = GetMappedMood(userPreferences.Mood);

            var scoredPlaces = _places
                .Select(place => new { Place = place, Score = CalculatePlaceScore(place, userPreferences, mappedMood) })
                .Where(item => item.Score >= MIN_SCORE_THRESHOLD)
                .OrderByDescending(item => item.Score)
                .Take(MAX_RECOMMENDATIONS)
                .Select(item => item.Place)
                .ToList();

            return scoredPlaces;
        }

        public static RouteData GenerateRoute(List<Place> places)
        {
            if (places == null || !places.Any())
                return new RouteData();

            return new RouteData
            {
                Places = places,
                RouteType = "pedestrian",
                TotalDistance = CalculateTotalDistance(places),
                EstimatedTime = CalculateEstimatedTime(places)
            };
        }

        private static UserPreferences GetUserPreferences()
        {
            return new UserPreferences
            {
                Mood = Preferences.Get("CurrentMood", ""),
                Company = Preferences.Get("ImproveMood", ""),
                Activity = Preferences.Get("Matter", ""),
                Time = Preferences.Get("TimeAvailable", ""),
                Budget = Preferences.Get("Budget", ""),
                District = Preferences.Get("MostSafety", "")
            };
        }

        private static string GetMappedMood(string userMood)
        {
            return MoodMapping.ContainsKey(userMood) ? MoodMapping[userMood] : userMood;
        }

        private static int CalculatePlaceScore(Place place, UserPreferences preferences, string mappedMood)
        {
            int score = 0;

            if (place.Mood == mappedMood)
                score += MOOD_SCORE_WEIGHT;

            if (place.Activity == preferences.Activity)
                score += ACTIVITY_SCORE_WEIGHT;

            if (place.Company == preferences.Company || preferences.Company == "Не важно")
                score += COMPANY_SCORE_WEIGHT;

            if (place.Time == preferences.Time || preferences.Time == "Не важно")
                score += TIME_SCORE_WEIGHT;

            if (place.Budget == preferences.Budget || preferences.Budget == "Не важно")
                score += BUDGET_SCORE_WEIGHT;

            if (place.District == preferences.District || preferences.District == "Не важно")
                score += DISTRICT_SCORE_WEIGHT;

            return score;
        }

        private static double CalculateTotalDistance(List<Place> places)
        {
            if (places.Count < 2) return 0;

            double totalDistance = 0;
            for (int i = 0; i < places.Count - 1; i++)
            {
                totalDistance += GetDistanceBetweenPoints(
                    places[i].Latitude, places[i].Longitude,
                    places[i + 1].Latitude, places[i + 1].Longitude);
            }
            return Math.Round(totalDistance, 2);
        }

        private static int CalculateEstimatedTime(List<Place> places)
        {
            const int MINUTES_PER_PLACE = 60;
            const int MINUTES_PER_KM = 12;

            int baseTime = places.Count * MINUTES_PER_PLACE;
            int travelTime = (int)(CalculateTotalDistance(places) * MINUTES_PER_KM);
            return baseTime + travelTime;
        }

        private static double GetDistanceBetweenPoints(double lat1, double lon1, double lat2, double lon2)
        {
            const double EARTH_RADIUS_KM = 6371;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EARTH_RADIUS_KM * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        private class UserPreferences
        {
            public string Mood { get; set; } = "";
            public string Company { get; set; } = "";
            public string Activity { get; set; } = "";
            public string Time { get; set; } = "";
            public string Budget { get; set; } = "";
            public string District { get; set; } = "";
        }
    }
}