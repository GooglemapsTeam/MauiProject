using Emotional_Map.Models;
using System.Text.Json;

namespace Emotional_Map.Services
{
    public static class FavoriteRoutesService
    {
        private const string FAVORITES_KEY = "FavoriteRoutes";

        public static List<FavoriteRoute> GetFavoriteRoutes()
        {
            try
            {
                var favoritesJson = Preferences.Get(FAVORITES_KEY, "");
                if (string.IsNullOrEmpty(favoritesJson))
                    return new List<FavoriteRoute>();

                var favoritesList = JsonSerializer.Deserialize<List<FavoriteRoute>>(favoritesJson);
                return favoritesList ?? new List<FavoriteRoute>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки избранных маршрутов: {ex.Message}");
                return new List<FavoriteRoute>();
            }
        }

        public static bool AddFavoriteRoute(FavoriteRoute route)
        {
            try
            {
                var favorites = GetFavoriteRoutes();

                if (favorites.Any(f => f.Name == route.Name))
                    return false;

                favorites.Add(route);
                SaveFavoriteRoutes(favorites);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка добавления маршрута в избранное: {ex.Message}");
                return false;
            }
        }

        public static bool RemoveFavoriteRoute(string routeId)
        {
            try
            {
                var favorites = GetFavoriteRoutes();
                var routeToRemove = favorites.FirstOrDefault(f => f.Id == routeId);

                if (routeToRemove != null)
                {
                    favorites.Remove(routeToRemove);
                    SaveFavoriteRoutes(favorites);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления маршрута из избранного: {ex.Message}");
                return false;
            }
        }

        public static bool IsRouteFavorite(List<Place> places)
        {
            try
            {
                var favorites = GetFavoriteRoutes();
                return favorites.Any(f => AreRoutesEqual(f.Places, places));
            }
            catch
            {
                return false;
            }
        }

        public static FavoriteRoute GetFavoriteRouteByPlaces(List<Place> places)
        {
            try
            {
                var favorites = GetFavoriteRoutes();
                return favorites.FirstOrDefault(f => AreRoutesEqual(f.Places, places));
            }
            catch
            {
                return null;
            }
        }

        private static void SaveFavoriteRoutes(List<FavoriteRoute> favorites)
        {
            try
            {
                var json = JsonSerializer.Serialize(favorites);
                Preferences.Set(FAVORITES_KEY, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения избранных маршрутов: {ex.Message}");
            }
        }

        private static bool AreRoutesEqual(List<Place> route1, List<Place> route2)
        {
            if (route1.Count != route2.Count)
                return false;

            for (int i = 0; i < route1.Count; i++)
            {
                if (route1[i].Id != route2[i].Id)
                    return false;
            }

            return true;
        }

        public static void ClearAllFavorites()
        {
            try
            {
                Preferences.Remove(FAVORITES_KEY);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка очистки избранных маршрутов: {ex.Message}");
            }
        }
    }
}
