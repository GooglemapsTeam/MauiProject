namespace Emotional_Map.Services
{
    public static class AppStateService
    {
        private const string SURVEY_COMPLETED_KEY = "IsSurveyCompleted";
        private const string LAST_SURVEY_DATE_KEY = "LastSurveyDate";
        private const string SURVEY_VERSION_KEY = "SurveyVersion";

        private const int CURRENT_SURVEY_VERSION = 1;

        public static bool IsSurveyCompleted()
        {
            try
            {
                var isCompleted = Preferences.Get(SURVEY_COMPLETED_KEY, false);
                var surveyVersion = Preferences.Get(SURVEY_VERSION_KEY, 0);
                var userName = Preferences.Get("Name", "");

                return isCompleted && !string.IsNullOrEmpty(userName) && surveyVersion >= CURRENT_SURVEY_VERSION;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка проверки состояния опроса: {ex.Message}");
                return false;
            }
        }

        public static void MarkSurveyCompleted()
        {
            try
            {
                Preferences.Set(SURVEY_COMPLETED_KEY, true);
                Preferences.Set(LAST_SURVEY_DATE_KEY, DateTime.Now.ToString());
                Preferences.Set(SURVEY_VERSION_KEY, CURRENT_SURVEY_VERSION);

                var surveysCount = Preferences.Get("SurveysCompleted", 0);
                Preferences.Set("SurveysCompleted", surveysCount + 1);

                if (!Preferences.ContainsKey("FirstUseDate"))
                {
                    Preferences.Set("FirstUseDate", DateTime.Now.ToString());
                }

                System.Diagnostics.Debug.WriteLine("Опрос отмечен как завершенный");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения состояния опроса: {ex.Message}");
            }
        }

        public static void ResetSurvey()
        {
            try
            {
                var userName = Preferences.Get("Name", "");
                var profileImagePath = Preferences.Get("ProfileImagePath", "");
                var soundEnabled = Preferences.Get("SoundEnabled", true);
                var favoritePlaces = Preferences.Get("FavoritePlaces", "");
                var favoriteRoutes = Preferences.Get("FavoriteRoutes", "");
                var surveysCompleted = Preferences.Get("SurveysCompleted", 0);
                var firstUseDate = Preferences.Get("FirstUseDate", DateTime.Now.ToString());

                Preferences.Remove("CurrentMood");
                Preferences.Remove("Matter");
                Preferences.Remove("ImproveMood");
                Preferences.Remove("MoodInfluence");
                Preferences.Remove("TimeAvailable");
                Preferences.Remove("MostSafety");
                Preferences.Remove("Budget");
                Preferences.Remove(SURVEY_COMPLETED_KEY);
                Preferences.Remove(LAST_SURVEY_DATE_KEY);

                if (!string.IsNullOrEmpty(userName))
                    Preferences.Set("Name", userName);
                if (!string.IsNullOrEmpty(profileImagePath))
                    Preferences.Set("ProfileImagePath", profileImagePath);
                Preferences.Set("SoundEnabled", soundEnabled);
                if (!string.IsNullOrEmpty(favoritePlaces))
                    Preferences.Set("FavoritePlaces", favoritePlaces);
                if (!string.IsNullOrEmpty(favoriteRoutes))
                    Preferences.Set("FavoriteRoutes", favoriteRoutes);
                Preferences.Set("SurveysCompleted", surveysCompleted);
                Preferences.Set("FirstUseDate", firstUseDate);

                System.Diagnostics.Debug.WriteLine("Состояние опроса сброшено");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сброса опроса: {ex.Message}");
            }
        }

        public static DateTime? GetLastSurveyDate()
        {
            try
            {
                var dateString = Preferences.Get(LAST_SURVEY_DATE_KEY, "");
                if (DateTime.TryParse(dateString, out var date))
                {
                    return date;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool ShouldSuggestRetakeSurvey()
        {
            try
            {
                var lastSurveyDate = GetLastSurveyDate();
                if (lastSurveyDate == null) return true;

                return (DateTime.Now - lastSurveyDate.Value).TotalDays > 7;
            }
            catch
            {
                return false;
            }
        }

        public static int GetSurveyProgress()
        {
            try
            {
                var fields = new[]
                {
                    "Name",
                    "CurrentMood",
                    "Matter",
                    "ImproveMood",
                    "MoodInfluence",
                    "TimeAvailable",
                    "MostSafety"
                };

                var filledFields = fields.Count(field => !string.IsNullOrEmpty(Preferences.Get(field, "")));
                return (int)((double)filledFields / fields.Length * 100);
            }
            catch
            {
                return 0;
            }
        }
    }
}
