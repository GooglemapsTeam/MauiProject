using Emotional_Map.Models;
using Emotional_Map.Services;
using System.Text;

namespace Emotional_Map;

[QueryProperty(nameof(SelectedPlace), "SelectedPlace")]
[QueryProperty(nameof(AllRecommendations), "AllRecommendations")]
public partial class YandexMapPage : ContentPage
{
    public Place SelectedPlace { get; set; }
    public List<Place> AllRecommendations { get; set; }

    private Location _userLocation;
    private bool _routeBuilt = false;
    private List<NavigationStep> _navigationSteps = new List<NavigationStep>();
    private int _currentStepIndex = 0;

    private const string YANDEX_API_KEY = "69f697cc-32fb-4058-8056-a615983e7e93";

    public YandexMapPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMap();
    }

    private async Task LoadMap()
    {
        try
        {
            _userLocation = await LocationService.Instance.GetCurrentLocationAsync();

            if (_userLocation != null)
            {
                System.Diagnostics.Debug.WriteLine($"Местоположение пользователя: {_userLocation.Latitude}, {_userLocation.Longitude}");
            }

            if (AllRecommendations?.Any() == true)
            {
                var route = RecommendationService.GenerateRoute(AllRecommendations);
                RouteTitle.Text = $"Маршрут из {route.Places.Count} мест";
                RouteInfo.Text = $"Расстояние: {route.TotalDistance} км • Время: {route.EstimatedTime} мин";
                BuildRouteButton.Text = "Проложить маршрут по всем местам";

                PrepareNavigationSteps(route.Places);

                await LoadYandexMap(route.Places, false);
            }
            else if (SelectedPlace != null)
            {
                RouteTitle.Text = SelectedPlace.Name;
                RouteInfo.Text = $"{SelectedPlace.District} • {SelectedPlace.Activity}";
                BuildRouteButton.Text = $"Проложить маршрут до места";

                PrepareNavigationSteps(new List<Place> { SelectedPlace });

                await LoadYandexMap(new List<Place> { SelectedPlace }, false);
            }
            else
            {
                RouteInfo.Text = "Нет данных для отображения";
                RouteButtonFrame.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить карту: {ex.Message}", "OK");
        }
    }

    private void PrepareNavigationSteps(List<Place> places)
    {
        _navigationSteps.Clear();
        _currentStepIndex = 0;

        if (_userLocation != null && places.Any())
        {
            _navigationSteps.Add(new NavigationStep
            {
                StepNumber = 0,
                Title = "Начальная точка",
                Description = "Ваше текущее местоположение",
                Latitude = _userLocation.Latitude,
                Longitude = _userLocation.Longitude,
                DistanceToNext = places.Any() ? CalculateDistance(_userLocation.Latitude, _userLocation.Longitude, places[0].Latitude, places[0].Longitude) : 0
            });

            for (int i = 0; i < places.Count; i++)
            {
                var place = places[i];
                var distanceToNext = i < places.Count - 1 ?
                    CalculateDistance(place.Latitude, place.Longitude, places[i + 1].Latitude, places[i + 1].Longitude) : 0;

                _navigationSteps.Add(new NavigationStep
                {
                    StepNumber = i + 1,
                    Title = place.Name,
                    Description = $"{place.District} • {place.Activity}",
                    Latitude = place.Latitude,
                    Longitude = place.Longitude,
                    DistanceToNext = distanceToNext
                });
            }
        }

        UpdateNavigationDisplay();
    }

    private void UpdateNavigationDisplay()
    {
        if (!_navigationSteps.Any())
        {
            NavigationFrame.IsVisible = false;
            return;
        }

        NavigationFrame.IsVisible = _routeBuilt;

        UpdateTotalRouteInfo();
    }

    private void UpdateTotalRouteInfo()
    {
        if (_navigationSteps.Any())
        {
            var totalDistance = _navigationSteps.Sum(step => step.DistanceToNext);
            var placesCount = _navigationSteps.Count - 1;
            var walkTime = (int)(totalDistance * 12);
            var visitTime = placesCount * 45;
            var totalTime = walkTime + visitTime;

            TotalDistanceLabel.Text = $"Общее расстояние: {FormatDistance(totalDistance)}";
            TotalTimeLabel.Text = $"Время: {totalTime} мин";
            PlacesCountLabel.Text = $"Мест: {placesCount}";
            WalkTimeLabel.Text = $"Ходьба: {walkTime} мин";
            VisitTimeLabel.Text = $"Посещение: {visitTime} мин";
        }
    }

    private string FormatDistance(double distanceKm)
    {
        if (distanceKm < 1)
        {
            return $"{(int)(distanceKm * 1000)} м";
        }
        else
        {
            return $"{distanceKm:F1} км";
        }
    }

    private int CalculateEstimatedTime(double totalDistanceKm, int placesCount)
    {
        const int MINUTES_PER_PLACE = 45;
        const int MINUTES_PER_KM = 12;

        int visitTime = placesCount * MINUTES_PER_PLACE;
        int walkTime = (int)(totalDistanceKm * MINUTES_PER_KM);
        return visitTime + walkTime;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
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

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    private async Task LoadYandexMap(List<Place> places, bool buildRoute = false)
    {
        try
        {
            var html = GenerateYandexMapHtml(places, buildRoute);
            var htmlSource = new HtmlWebViewSource { Html = html };
            MapWebView.Source = htmlSource;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка создания карты: {ex.Message}", "OK");
        }
    }

    private string GenerateYandexMapHtml(List<Place> places, bool buildRoute = false)
    {
        var html = new StringBuilder();

        var centerLat = places.Average(p => p.Latitude);
        var centerLon = places.Average(p => p.Longitude);

        if (_userLocation != null && buildRoute)
        {
            var allLats = places.Select(p => p.Latitude).Concat(new[] { _userLocation.Latitude });
            var allLons = places.Select(p => p.Longitude).Concat(new[] { _userLocation.Longitude });
            centerLat = allLats.Average();
            centerLon = allLons.Average();
        }

        var zoomLevel = buildRoute ? 10 : (places.Count == 1 ? 14 : 12);

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset='utf-8'>");
        html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        html.AppendLine("<title>Карта маршрута</title>");

        var apiUrl = string.IsNullOrEmpty(YANDEX_API_KEY)
            ? "https://api-maps.yandex.ru/2.1/?lang=ru_RU"
            : $"https://api-maps.yandex.ru/2.1/?apikey={YANDEX_API_KEY}&lang=ru_RU";

        html.AppendLine($"<script src='{apiUrl}' type='text/javascript'></script>");
        html.AppendLine($"<script src='https://api-maps.yandex.ru/2.1/?apikey={YANDEX_API_KEY}&lang=ru_RU&load=package.full' type='text/javascript'></script>");
        html.AppendLine("<style>");
        html.AppendLine("body, html { margin: 0; padding: 0; width: 100%; height: 100%; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif; }");
        html.AppendLine("#map { width: 100%; height: 100vh; }");
        html.AppendLine(".route-info { position: absolute; top: 10px; left: 10px; background: rgba(255,255,255,0.95); padding: 15px; border-radius: 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); z-index: 1000; max-width: 280px; font-size: 14px; }");
        html.AppendLine(".route-info.success { border-left: 4px solid #4CAF50; }");
        html.AppendLine(".route-info.error { border-left: 4px solid #f44336; background: rgba(255,235,238,0.95); }");
        html.AppendLine(".route-info.loading { border-left: 4px solid #2196F3; }");
        html.AppendLine(".loading-spinner { display: inline-block; width: 16px; height: 16px; border: 2px solid #f3f3f3; border-top: 2px solid #2196F3; border-radius: 50%; animation: spin 1s linear infinite; margin-right: 8px; }");
        html.AppendLine("@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }");
        html.AppendLine(".route-options { position: absolute; top: 10px; right: 10px; background: rgba(255,255,255,0.95); padding: 10px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.3); z-index: 1000; max-width: 200px; font-size: 12px; }");
        html.AppendLine(".route-option { margin: 5px 0; padding: 8px; background: #f0f0f0; border-radius: 5px; cursor: pointer; }");
        html.AppendLine(".route-option.active { background: #14D0FF; color: white; }");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<div id='map'></div>");
        html.AppendLine("<div id='routeInfo' class='route-info' style='display: none;'></div>");
        html.AppendLine("<script type='text/javascript'>");

        html.AppendLine("ymaps.ready(function() {");
        html.AppendLine("    try {");
        html.AppendLine("        console.log('Инициализация Яндекс.Карт...');");

        html.AppendLine($"        var myMap = new ymaps.Map('map', {{");
        html.AppendLine($"            center: [{centerLat.ToString().Replace(',', '.')}, {centerLon.ToString().Replace(',', '.')}],");
        html.AppendLine($"            zoom: {zoomLevel},");
        html.AppendLine($"            controls: ['zoomControl', 'fullscreenControl', 'geolocationControl', 'typeSelector']");
        html.AppendLine($"        }}, {{");
        html.AppendLine($"            searchControlProvider: 'yandex#search'");
        html.AppendLine($"        }});");

        if (_userLocation != null)
        {
            html.AppendLine($"        var userPlacemark = new ymaps.Placemark([{_userLocation.Latitude.ToString().Replace(',', '.')}, {_userLocation.Longitude.ToString().Replace(',', '.')}], {{");
            html.AppendLine($"            balloonContent: '<div style=\"padding: 10px; min-width: 200px;\"><strong>Ваше местоположение</strong><br><small>Отсюда начнется маршрут</small></div>',");
            html.AppendLine($"            hintContent: 'Ваше местоположение'");
            html.AppendLine($"        }}, {{");
            html.AppendLine($"            preset: 'islands#blueCircleDotIcon',");
            html.AppendLine($"            iconColor: '#0066FF'");
            html.AppendLine($"        }});");
            html.AppendLine($"        myMap.geoObjects.add(userPlacemark);");
            html.AppendLine($"        console.log('Добавлено местоположение пользователя');");
        }

        for (int i = 0; i < places.Count; i++)
        {
            var place = places[i];
            var lat = place.Latitude.ToString().Replace(',', '.');
            var lon = place.Longitude.ToString().Replace(',', '.');

            var iconPreset = places.Count == 1 ? "islands#redIcon" :
                           (i < 9 ? $"islands#red{(i + 1)}Icon" : "islands#redIcon");

            html.AppendLine($"        var placemark{i} = new ymaps.Placemark([{lat}, {lon}], {{");
            html.AppendLine($"            balloonContent: '<div style=\"padding: 10px; min-width: 250px;\"><strong>{place.Name}</strong><br><p style=\"margin: 8px 0;\">{place.Description}</p><div style=\"color: #666; font-size: 12px;\">{place.District} • {place.Activity}<br>{place.Budget} • {place.Time}</div></div>',");
            html.AppendLine($"            hintContent: '{place.Name}'");
            html.AppendLine($"        }}, {{");
            html.AppendLine($"            preset: '{iconPreset}'");
            html.AppendLine($"        }});");
            html.AppendLine($"        myMap.geoObjects.add(placemark{i});");
        }

        html.AppendLine($"        console.log('Добавлено {places.Count} меток на карту');");

        if (buildRoute && _userLocation != null)
        {
            html.AppendLine("        showRouteInfo('<div class=\"loading-spinner\"></div>Строим маршрут...', 'loading');");

            html.AppendLine("        try {");
            html.AppendLine("            var routePoints = [");

            html.AppendLine($"                [{_userLocation.Latitude.ToString().Replace(',', '.')}, {_userLocation.Longitude.ToString().Replace(',', '.')}],");

            for (int i = 0; i < places.Count; i++)
            {
                var lat = places[i].Latitude.ToString().Replace(',', '.');
                var lon = places[i].Longitude.ToString().Replace(',', '.');
                html.AppendLine($"                [{lat}, {lon}]{(i < places.Count - 1 ? "," : "")}");
            }

            html.AppendLine("            ];");
            html.AppendLine("            console.log('Точки маршрута:', routePoints);");

            html.AppendLine("            var multiRoute = new ymaps.multiRouter.MultiRoute({");
            html.AppendLine("                referencePoints: routePoints,");
            html.AppendLine("                params: { ");
            html.AppendLine("                    routingMode: 'pedestrian',");
            html.AppendLine("                    avoidTrafficJams: false,");
            html.AppendLine("                    strictBounds: false,");
            html.AppendLine("                    results: 3");
            html.AppendLine("                }");
            html.AppendLine("            }, {");
            html.AppendLine("                boundsAutoApply: true,");
            html.AppendLine("                wayPointVisible: true,");
            html.AppendLine("                wayPointIconFillColor: '#14D0FF',");
            html.AppendLine("                routeActiveStrokeWidth: 6,");
            html.AppendLine("                routeActiveStrokeColor: '#14D0FF',");
            html.AppendLine("                routeActiveStrokeOpacity: 0.9,");
            html.AppendLine("                routeInactiveStrokeColor: '#cccccc',");
            html.AppendLine("                routeInactiveStrokeOpacity: 0.6,");
            html.AppendLine("                pinIconFillColor: '#14D0FF',");
            html.AppendLine("                pinIconStrokeColor: '#ffffff'");
            html.AppendLine("            });");

            html.AppendLine("            myMap.geoObjects.add(multiRoute);");
            html.AppendLine("            console.log('Маршрут добавлен на карту');");

            html.AppendLine("            multiRoute.model.events.add('requestsuccess', function () {");
            html.AppendLine("                console.log('Маршрут успешно построен');");
            html.AppendLine("                try {");
            html.AppendLine("                    var activeRoute = multiRoute.getActiveRoute();");
            html.AppendLine("                    if (activeRoute) {");
            html.AppendLine("                        var distance = activeRoute.properties.get('distance');");
            html.AppendLine("                        var duration = activeRoute.properties.get('duration');");
            html.AppendLine("                        var routeLength = activeRoute.properties.get('length');");
            html.AppendLine("                        ");
            html.AppendLine("                        var distanceText = distance ? distance.text : 'неизвестно';");
            html.AppendLine("                        var durationText = duration ? duration.text : 'неизвестно';");
            html.AppendLine("                        ");
            html.AppendLine("                        showRouteInfo('<strong>Маршрут построен успешно!</strong><br>Расстояние: ' + distanceText + '<br>Время в пути: ' + durationText + '<br><small>Пешеходный маршрут</small>', 'success');");
            html.AppendLine("                        ");
            html.AppendLine("                        setTimeout(function() {");
            html.AppendLine("                            var routeInfo = document.getElementById('routeInfo');");
            html.AppendLine("                            if (routeInfo) routeInfo.style.display = 'none';");
            html.AppendLine("                        }, 10000);");
            html.AppendLine("                    } else {");
            html.AppendLine("                        showRouteInfo('<strong>Маршрут построен</strong><br>Маршрут отображен на карте', 'success');");
            html.AppendLine("                    }");
            html.AppendLine("                } catch(e) {");
            html.AppendLine("                    console.error('Ошибка обработки данных маршрута:', e);");
            html.AppendLine("                    showRouteInfo('<strong>Маршрут построен</strong><br>Маршрут отображен на карте', 'success');");
            html.AppendLine("                }");
            html.AppendLine("                        var routes = multiRoute.getRoutes();");
            html.AppendLine("                        if (routes.getLength() > 1) {");
            html.AppendLine("                            showRouteOptions(routes);");
            html.AppendLine("                        }");
            html.AppendLine("            });");

            html.AppendLine("            multiRoute.model.events.add('requestfail', function (e) {");
            html.AppendLine("                console.error('Ошибка построения маршрута:', e);");
            html.AppendLine("                showRouteInfo('<strong>Не удалось построить маршрут</strong><br>Возможные причины:<br>• Нет пешеходных дорог между точками<br>• Проблемы с интернет-соединением<br>• Ограничения API<br><br>Попробуйте обновить карту или используйте внешний навигатор', 'error');");
            html.AppendLine("            });");

            html.AppendLine("            multiRoute.model.events.add('requeststart', function () {");
            html.AppendLine("                console.log('Начало запроса маршрута');");
            html.AppendLine("                showRouteInfo('<div class=\"loading-spinner\"></div>Строим маршрут...', 'loading');");
            html.AppendLine("            });");

            html.AppendLine("        } catch(routeError) {");
            html.AppendLine("            console.error('Ошибка создания маршрута:', routeError);");
            html.AppendLine("            showRouteInfo('<strong>Ошибка создания маршрута</strong><br>' + routeError.message, 'error');");
            html.AppendLine("        }");
        }
        else if (buildRoute && _userLocation == null)
        {
            html.AppendLine("        showRouteInfo('<strong>Местоположение недоступно</strong><br>Для построения маршрута необходимо определить ваше местоположение.<br><br>Нажмите кнопку \"GPS\" для обновления местоположения', 'error');");
        }

        html.AppendLine("        function showRouteInfo(message, type) {");
        html.AppendLine("            var routeInfo = document.getElementById('routeInfo');");
        html.AppendLine("            routeInfo.innerHTML = message;");
        html.AppendLine("            routeInfo.className = 'route-info ' + (type || '');");
        html.AppendLine("            routeInfo.style.display = 'block';");
        html.AppendLine("            console.log('Показано сообщение:', type, message);");
        html.AppendLine("        }");

        html.AppendLine("        window.showRouteInfo = showRouteInfo;");

        html.AppendLine("        function showRouteOptions(routes) {");
        html.AppendLine("            var optionsHtml = '<div class=\"route-options\"><strong>Варианты маршрута:</strong><br>';");
        html.AppendLine("            for (var i = 0; i < Math.min(routes.getLength(), 3); i++) {");
        html.AppendLine("                var route = routes.get(i);");
        html.AppendLine("                var distance = route.properties.get('distance');");
        html.AppendLine("                var duration = route.properties.get('duration');");
        html.AppendLine("                var isActive = i === multiRoute.getActiveRouteIndex();");
        html.AppendLine("                optionsHtml += '<div class=\"route-option' + (isActive ? ' active' : '') + '\" onclick=\"selectRoute(' + i + ')\">';");
        html.AppendLine("                optionsHtml += 'Маршрут ' + (i + 1) + '<br>';");
        html.AppendLine("                optionsHtml += (distance ? distance.text : 'неизвестно') + '<br>';");
        html.AppendLine("                optionsHtml += (duration ? duration.text : 'неизвестно');");
        html.AppendLine("                optionsHtml += '</div>';");
        html.AppendLine("            }");
        html.AppendLine("            optionsHtml += '</div>';");
        html.AppendLine("            document.body.insertAdjacentHTML('beforeend', optionsHtml);");
        html.AppendLine("        }");
        html.AppendLine("");
        html.AppendLine("        function selectRoute(index) {");
        html.AppendLine("            multiRoute.setActiveRouteIndex(index);");
        html.AppendLine("            var activeRoute = multiRoute.getActiveRoute();");
        html.AppendLine("            if (activeRoute) {");
        html.AppendLine("                var distance = activeRoute.properties.get('distance');");
        html.AppendLine("                var duration = activeRoute.properties.get('duration');");
        html.AppendLine("                showRouteInfo('<strong>Выбран маршрут ' + (index + 1) + '</strong><br>' + (distance ? distance.text : 'неизвестно') + '<br>' + (duration ? duration.text : 'неизвестно'), 'success');");
        html.AppendLine("            }");
        html.AppendLine("            var options = document.querySelectorAll('.route-option');");
        html.AppendLine("            options.forEach(function(option, i) {");
        html.AppendLine("                option.className = i === index ? 'route-option active' : 'route-option';");
        html.AppendLine("            });");
        html.AppendLine("        }");

        html.AppendLine("    } catch(error) {");
        html.AppendLine("        console.error('Критическая ошибка инициализации карты:', error);");
        html.AppendLine("        document.body.innerHTML = '<div style=\"padding: 20px; text-align: center; font-family: Arial; background: #f5f5f5; height: 100vh; display: flex; flex-direction: column; justify-content: center;\"><div style=\"background: white; padding: 30px; border-radius: 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.1);\"><h3 style=\"color: #f44336; margin-top: 0;\">Ошибка загрузки карты</h3><p style=\"color: #666; margin-bottom: 20px;\">Не удалось инициализировать Яндекс.Карты</p><p style=\"font-size: 14px; color: #999;\">Проверьте подключение к интернету и попробуйте обновить страницу</p></div></div>';");
        html.AppendLine("    }");
        html.AppendLine("});");
        html.AppendLine("</script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private async void OnBuildRouteClicked(object sender, EventArgs e)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            BuildRouteButton.Text = "Строим маршрут...";
            BuildRouteButton.IsEnabled = false;

            if (_userLocation == null)
            {
                var getLocation = await DisplayAlert("Местоположение",
                    "Для построения маршрута нужно определить ваше местоположение.\n\nРазрешить доступ к геолокации?",
                    "Да, определить", "Использовать центр города");

                if (getLocation)
                {
                    BuildRouteButton.Text = "Определяем местоположение...";
                    _userLocation = await LocationService.Instance.GetCurrentLocationAsync();

                    if (_userLocation == null)
                    {
                        var useCenter = await DisplayAlert("Местоположение",
                            "Не удалось определить точное местоположение.\n\nИспользовать центр Екатеринбурга как начальную точку?",
                            "Да", "Отмена");

                        if (useCenter)
                        {
                            _userLocation = new Location(56.8431, 60.6454);
                        }
                        else
                        {
                            BuildRouteButton.Text = "Проложить маршрут";
                            BuildRouteButton.IsEnabled = true;
                            return;
                        }
                    }
                }
                else
                {
                    _userLocation = new Location(56.8431, 60.6454);
                    await DisplayAlert("Информация", "Маршрут будет построен от центра Екатеринбурга", "OK");
                }
            }

            List<Place> placesToRoute;
            if (AllRecommendations?.Any() == true)
            {
                placesToRoute = AllRecommendations;
            }
            else if (SelectedPlace != null)
            {
                placesToRoute = new List<Place> { SelectedPlace };
            }
            else
            {
                await DisplayAlert("Ошибка", "Нет мест для построения маршрута", "OK");
                BuildRouteButton.IsEnabled = true;
                BuildRouteButton.Text = "Проложить маршрут";
                return;
            }

            BuildRouteButton.Text = "Загружаем карту...";

            PrepareNavigationSteps(placesToRoute);

            await LoadYandexMap(placesToRoute, true);

            _routeBuilt = true;

            NavigationFrame.IsVisible = true;
            UpdateNavigationDisplay();

            await Task.Delay(3000);

            BuildRouteButton.Text = "Маршрут строится";
            BuildRouteButton.BackgroundColor = Color.FromHex("#28A745");

            await Task.Delay(5000);

            BuildRouteButton.Text = "Перестроить маршрут";
            BuildRouteButton.BackgroundColor = Color.FromHex("#14D0FF");
            BuildRouteButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            BuildRouteButton.Text = "Ошибка построения";
            BuildRouteButton.BackgroundColor = Color.FromHex("#DC3545");
            await DisplayAlert("Ошибка", $"Не удалось построить маршрут: {ex.Message}", "OK");

            await Task.Delay(3000);
            BuildRouteButton.Text = "Попробовать снова";
            BuildRouteButton.BackgroundColor = Color.FromHex("#14D0FF");
            BuildRouteButton.IsEnabled = true;
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(MainPage));
    }

    private async void OnMyLocationClicked(object sender, EventArgs e)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var location = await LocationService.Instance.GetCurrentLocationAsync();
            if (location != null)
            {
                _userLocation = location;
                await CenterMapOnUserLocation();

                await DisplayAlert("Местоположение",
                    $"Ваше текущее местоположение:\nШирота: {location.Latitude:F6}\nДолгота: {location.Longitude:F6}",
                    "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось определить ваше местоположение", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка получения местоположения: {ex.Message}", "OK");
        }
    }

    private async Task CenterMapOnUserLocation()
    {
        if (_userLocation == null) return;

        try
        {
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Текущее местоположение</title>
                    <script src='https://api-maps.yandex.ru/2.1/?apikey={YANDEX_API_KEY}&lang=ru_RU' type='text/javascript'></script>
                    <style>
                        body, html {{ margin: 0; padding: 0; width: 100%; height: 100%; }}
                        #map {{ width: 100%; height: 100%; }}
                    </style>
                </head>
                <body>
                    <div id='map'></div>
                    <script type='text/javascript'>
                        ymaps.ready(function() {{
                            var myMap = new ymaps.Map('map', {{
                                center: [{_userLocation.Latitude.ToString().Replace(',', '.')}, {_userLocation.Longitude.ToString().Replace(',', '.')}],
                                zoom: 15,
                                controls: ['zoomControl', 'fullscreenControl']
                            }});
                            
                            var userPlacemark = new ymaps.Placemark([{_userLocation.Latitude.ToString().Replace(',', '.')}, {_userLocation.Longitude.ToString().Replace(',', '.')}], {{
                                balloonContent: 'Ваше текущее местоположение',
                                hintContent: 'Вы здесь'
                            }}, {{
                                preset: 'islands#blueCircleDotIcon',
                                iconColor: '#0066FF'
                            }});
                            
                            myMap.geoObjects.add(userPlacemark);
                        }});
                    </script>
                </body>
                </html>";

            var htmlSource = new HtmlWebViewSource { Html = html };
            MapWebView.Source = htmlSource;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка обновления карты: {ex.Message}", "OK");
        }
    }

    private class NavigationStep
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DistanceToNext { get; set; }
    }
}