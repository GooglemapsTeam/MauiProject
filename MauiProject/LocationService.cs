namespace Emotional_Map.Services
{
    public class LocationService
    {
        private static LocationService _instance;
        public static LocationService Instance => _instance ??= new LocationService();

        private LocationService() { }

        public async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        System.Diagnostics.Debug.WriteLine("Разрешение на геолокацию не предоставлено");
                        return GetDefaultLocation();
                    }
                }

                var request = new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(15)
                };

                var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken.Token);

                if (location != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Получено местоположение: {location.Latitude}, {location.Longitude}");
                    return location;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Местоположение не получено, используем по умолчанию");
                    return GetDefaultLocation();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка получения местоположения: {ex.Message}");
                return GetDefaultLocation();
            }
        }

        private Location GetDefaultLocation()
        {
            return new Location(56.8431, 60.6454);
        }

        public async Task<bool> IsLocationAvailableAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                return status == PermissionStatus.Granted;
            }
            catch
            {
                return false;
            }
        }
    }
}
