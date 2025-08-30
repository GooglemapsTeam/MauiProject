using System.Diagnostics;

namespace Emotional_Map
{
    public class LocationService
    {
        // Singleton instance
        private static LocationService _instance;
        public static LocationService Instance => _instance ??= new LocationService();

        // Последнее известное местоположение
        private Location _lastKnownLocation;

        // Флаг, указывающий, запрашиваются ли в данный момент разрешения
        private bool _isRequestingPermissions;

        // Private constructor for singleton
        private LocationService() { }

        /// <summary>
        /// Проверяет и запрашивает разрешения на использование геолокации
        /// </summary>
        /// <returns>True, если разрешения предоставлены, иначе False</returns>
        public async Task<bool> CheckAndRequestLocationPermissionsAsync()
        {
            if (_isRequestingPermissions)
                return false;

            try
            {
                _isRequestingPermissions = true;

                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    Debug.WriteLine("Запрашиваем разрешение на использование геолокации...");
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                    if (status != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("Разрешение на использование геолокации не предоставлено");
                        await Application.Current.MainPage.DisplayAlert(
                            "Требуется разрешение",
                            "Для построения маршрута необходим доступ к вашему местоположению. Пожалуйста, предоставьте разрешение в настройках приложения.",
                            "OK");
                        return false;
                    }
                }

                Debug.WriteLine("Разрешение на использование геолокации предоставлено");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при запросе разрешений: {ex.Message}");
                return false;
            }
            finally
            {
                _isRequestingPermissions = false;
            }
        }

        /// <summary>
        /// Получает текущее местоположение пользователя с таймаутом и повторными попытками
        /// </summary>
        /// <param name="useLastKnownLocation">Использовать последнее известное местоположение в случае ошибки</param>
        /// <param name="maxRetries">Максимальное количество повторных попыток</param>
        /// <returns>Объект Location с координатами или null в случае ошибки</returns>
        public async Task<Location> GetCurrentLocationAsync(bool useLastKnownLocation = true, int maxRetries = 3)
        {
            // Проверяем разрешения
            if (!await CheckAndRequestLocationPermissionsAsync())
            {
                Debug.WriteLine("Нет разрешений для получения местоположения");
                return useLastKnownLocation ? _lastKnownLocation : null;
            }

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    Debug.WriteLine($"Попытка получения местоположения {retry + 1}/{maxRetries}");

                    // Настройки геолокации с таймаутом
                    GeolocationRequest request = new GeolocationRequest(
                        GeolocationAccuracy.Best,
                        TimeSpan.FromSeconds(10));

                    // Используем токен отмены для ограничения времени ожидания
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

                    // Получаем текущее местоположение
                    Location location = await Geolocation.GetLocationAsync(request, cts.Token);

                    if (location != null)
                    {
                        Debug.WriteLine($"Местоположение получено: {location.Latitude}, {location.Longitude}");
                        _lastKnownLocation = location; // Сохраняем последнее известное местоположение
                        return location;
                    }
                }
                catch (FeatureNotSupportedException)
                {
                    Debug.WriteLine("Геолокация не поддерживается на этом устройстве");
                    await Application.Current.MainPage.DisplayAlert(
                        "Ошибка",
                        "Геолокация не поддерживается на этом устройстве",
                        "OK");
                    return useLastKnownLocation ? _lastKnownLocation : null;
                }
                catch (FeatureNotEnabledException)
                {
                    Debug.WriteLine("Геолокация отключена на устройстве");
                    bool openSettings = await Application.Current.MainPage.DisplayAlert(
                        "Геолокация отключена",
                        "Для построения маршрута необходимо включить геолокацию в настройках устройства. Открыть настройки?",
                        "Да", "Нет");

                    if (openSettings)
                    {
                        // Открываем настройки геолокации
                        await OpenLocationSettings();
                    }

                    return useLastKnownLocation ? _lastKnownLocation : null;
                }
                catch (PermissionException)
                {
                    Debug.WriteLine("Нет разрешения на использование геолокации");
                    await CheckAndRequestLocationPermissionsAsync();
                    // Продолжаем цикл для повторной попытки
                }
                catch (TaskCanceledException)
                {
                    Debug.WriteLine("Превышено время ожидания получения местоположения");
                    // Продолжаем цикл для повторной попытки с увеличенным таймаутом
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка при получении местоположения: {ex.Message}");
                    // Продолжаем цикл для повторной попытки
                }

                // Небольшая задержка перед следующей попыткой
                if (retry < maxRetries - 1)
                {
                    await Task.Delay(1000);
                }
            }

            Debug.WriteLine("Не удалось получить местоположение после нескольких попыток");

            // Возвращаем последнее известное местоположение, если оно есть и если это разрешено
            if (useLastKnownLocation && _lastKnownLocation != null)
            {
                Debug.WriteLine($"Используем последнее известное местоположение: {_lastKnownLocation.Latitude}, {_lastKnownLocation.Longitude}");
                return _lastKnownLocation;
            }

            // Если нет последнего известного местоположения или его использование запрещено, используем фиксированное местоположение
            if (await Application.Current.MainPage.DisplayAlert(
                "Не удалось получить местоположение",
                "Хотите использовать стандартное местоположение для построения маршрута?",
                "Да", "Нет"))
            {
                // Возвращаем фиксированное местоположение (например, центр Москвы)
                return new Location(55.751244, 37.618423);
            }

            return null;
        }

        /// <summary>
        /// Открывает настройки геолокации на устройстве
        /// </summary>
        private async Task OpenLocationSettings()
        {
            try
            {
                // Открываем настройки геолокации
#if ANDROID
                Android.Content.Intent intent = new Android.Content.Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                Android.App.Application.Context.StartActivity(intent);
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при открытии настроек геолокации: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Ошибка",
                    "Не удалось открыть настройки геолокации. Пожалуйста, включите геолокацию вручную в настройках устройства.",
                    "OK");
            }
        }
    }
}