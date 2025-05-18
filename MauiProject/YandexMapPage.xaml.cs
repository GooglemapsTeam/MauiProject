using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Emotional_Map;

public partial class YandexMapPage : ContentPage
{
    private string _apiKey = "69f697cc-32fb-4058-8056-a615983e7e93"; // Замените на ваш API-ключ Яндекс Карт
    private bool _isLocationPermissionGranted = false;

    // Предопределенные точки маршрута (координаты достопримечательностей Екатеринбурга)
    private readonly string[] _predefinedPoints = new string[]
    {
        "56.844106, 60.645473", // Ельцин центр
        "56.837527, 60.604722", // Плотинка Екатеринбург
        "56.826389, 60.631111", // Дендропарк
        "56.821667, 60.621944"  // Зеленая роща
    };

    // Названия точек маршрута
    private readonly string[] _predefinedNames = new string[]
    {
        "Ельцин центр",
        "Плотинка",
        "Дендропарк",
        "Зеленая роща"
    };

    // Информация о маршруте
    private string _routeDistance = "";
    private string _routeDuration = "";

    public YandexMapPage()
    {
        InitializeComponent();

        // Добавляем обработчик события загрузки WebView
        MapWebView.Navigated += (sender, e) => {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        };

        // Добавляем обработчик для кнопки построения маршрута
        BuildRouteButton.Clicked += (sender, e) => {
            BuildPredefinedRoute();
        };

        // Включаем кнопку отладки в режиме разработки
#if DEBUG
        DebugButton.IsVisible = true;
        DebugButton.Clicked += async (sender, e) => {
            await DisplayAlert("Отладочная информация",
                $"Разрешение на геолокацию: {_isLocationPermissionGranted}\n" +
                $"Текущий URL: {MapWebView.Source}\n" +
                $"Расстояние: {_routeDistance}\n" +
                $"Время в пути: {_routeDuration}",
                "OK");
        };
#endif
    }

    // Исправляем метод BuildPredefinedRoute, чтобы избежать дублирования переменных
    private async void BuildPredefinedRoute()
    {
        try
        {
            // Показываем индикатор загрузки
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            // Получаем текущее местоположение пользователя
            var currentLocation = await GetCurrentLocationAsync();

            List<string> coordinates = new List<string>();
            List<string> names = new List<string>();

            if (currentLocation == null)
            {
                await DisplayAlert("Внимание", "Не удалось определить ваше местоположение. Маршрут будет построен без учета вашего местоположения.", "OK");

                // Строим маршрут только через предопределенные точки
                coordinates.AddRange(_predefinedPoints);
                names.AddRange(_predefinedNames);
            }
            else
            {
                // Добавляем текущее местоположение как первую точку
                coordinates.Add($"{currentLocation.Latitude.ToString(CultureInfo.InvariantCulture)}, {currentLocation.Longitude.ToString(CultureInfo.InvariantCulture)}");
                names.Add("Ваше местоположение");

                // Добавляем предопределенные точки
                coordinates.AddRange(_predefinedPoints);
                names.AddRange(_predefinedNames);
            }

            // Объединяем координаты и названия в строки с разделителем |
            var routeCoordsString = string.Join("|", coordinates);
            var routeNamesString = string.Join("|", names);

            // Строим маршрут
            LoadMapWithRoute(routeCoordsString, routeNamesString);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при построении предопределенного маршрута: {ex.Message}");
            await DisplayAlert("Ошибка", "Не удалось построить маршрут", "OK");
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Запрашиваем разрешение на использование геолокации
        _isLocationPermissionGranted = await RequestLocationPermissionAsync();

        try
        {
            // Получаем параметры через Query
            var route = Shell.Current.CurrentState.Location.ToString();
            Debug.WriteLine($"Текущий маршрут: {route}");

            var queryStart = route.IndexOf('?');

            if (queryStart > 0)
            {
                var query = route.Substring(queryStart + 1);
                Debug.WriteLine($"Параметры запроса: {query}");

                var parameters = ParseQueryParameters(query);

                foreach (var param in parameters)
                {
                    Debug.WriteLine($"Параметр: {param.Key} = {param.Value}");
                }

                if (parameters.TryGetValue("coords", out var coords) &&
                    parameters.TryGetValue("names", out var names))
                {
                    Debug.WriteLine($"Координаты: {coords}");
                    Debug.WriteLine($"Названия: {names}");

                    LoadMapWithRoute(coords, names);
                    return;
                }
                else
                {
                    Debug.WriteLine("Не найдены параметры coords или names");
                }
            }
            else
            {
                Debug.WriteLine("Не найден символ '?' в URL");
            }

            // Если не удалось загрузить маршрут, загружаем карту с текущим местоположением
            await LoadMapWithCurrentLocationAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в OnAppearing: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await LoadMapWithCurrentLocationAsync();
        }
    }

    private async Task<bool> RequestLocationPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при запросе разрешения на геолокацию: {ex.Message}");
            return false;
        }
    }

    private async Task<Location> GetCurrentLocationAsync()
    {
        try
        {
            if (!_isLocationPermissionGranted)
            {
                Debug.WriteLine("Нет разрешения на использование геолокации");
                return null;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                Debug.WriteLine($"Текущие координаты: {location.Latitude}, {location.Longitude}, Точность: {location.Accuracy} метров");
                return location;
            }

            Debug.WriteLine("Не удалось получить текущее местоположение");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при получении текущего местоположения: {ex.Message}");
            return null;
        }
    }

    private Dictionary<string, string> ParseQueryParameters(string query)
    {
        var parameters = new Dictionary<string, string>();
        var pairs = query.Split('&');

        foreach (var pair in pairs)
        {
            var items = pair.Split('=');
            if (items.Length == 2)
            {
                parameters[items[0]] = Uri.UnescapeDataString(items[1]);
            }
        }

        return parameters;
    }

    // Обновляем метод GenerateRouteHtml, чтобы правильно отображать маркер текущего местоположения
    private string GenerateRouteHtml(string[] coordinates, string[] placeNames)
    {
        var sb = new StringBuilder();
        sb.Append(@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset=""utf-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
            <title>Маршрут</title>
            <script src=""https://api-maps.yandex.ru/2.1/?apikey=");

        sb.Append(_apiKey);

        sb.Append(@"&lang=ru_RU"" type=""text/javascript""></script>
            <style>
                html, body, #map {
                    width: 100%; 
                    height: 100%; 
                    padding: 0; 
                    margin: 0;
                }
                .error-message {
                    position: absolute;
                    top: 10px;
                    left: 10px;
                    background: white;
                    padding: 10px;
                    border-radius: 5px;
                    box-shadow: 0 0 10px rgba(0,0,0,0.3);
                    z-index: 1000;
                    display: none;
                }
                .accuracy-circle {
                    stroke: #4285F4;
                    stroke-opacity: 0.6;
                    stroke-width: 1;
                    fill: #4285F4;
                    fill-opacity: 0.2;
                }
                .location-button {
                    position: absolute;
                    bottom: 20px;
                    right: 20px;
                    background: white;
                    border: none;
                    border-radius: 50%;
                    width: 50px;
                    height: 50px;
                    box-shadow: 0 2px 6px rgba(0,0,0,0.3);
                    cursor: pointer;
                    z-index: 1000;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }
                .location-button:focus {
                    outline: none;
                }
            </style>
        </head>
        <body>
            <div id=""map""></div>
            <div id=""error-message"" class=""error-message""></div>
            <button id=""location-button"" class=""location-button"" title=""Моё местоположение"">
                <svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                    <path d=""M12 8C9.79 8 8 9.79 8 12C8 14.21 9.79 16 12 16C14.21 16 16 14.21 16 12C16 9.79 14.21 8 12 8ZM20.94 11C20.48 6.83 17.17 3.52 13 3.06V1H11V3.06C6.83 3.52 3.52 6.83 3.06 11H1V13H3.06C3.52 17.17 6.83 20.48 11 20.94V23H13V20.94C17.17 20.48 20.48 17.17 20.94 13H23V11H20.94ZM12 19C8.13 19 5 15.87 5 12C5 8.13 8.13 5 12 5C15.87 5 19 8.13 19 12C19 15.87 15.87 19 12 19Z"" fill=""#4285F4""/>
                </svg>
            </button>
            <script>
                // Функция для отображения ошибок
                function showError(message) {
                    var errorDiv = document.getElementById('error-message');
                    errorDiv.textContent = message;
                    errorDiv.style.display = 'block';
                    console.error(message);
                }

                // Обработка глобальных ошибок JavaScript
                window.onerror = function(message, source, lineno, colno, error) {
                    showError('JavaScript error: ' + message);
                    return true;
                };

                try {
                    ymaps.ready(init);
                } catch (e) {
                    showError('Error loading Yandex Maps: ' + e.message);
                }
                
                function init() {
                    try {
                        console.log('Initializing map...');
                        
                        // Проверяем координаты
                        var firstCoord = [");

        sb.Append(coordinates[0]);

        sb.Append(@"];
                        console.log('First coordinate:', firstCoord);
                        
                        var map = new ymaps.Map('map', {
                            center: firstCoord,
                            zoom: 16,
                            controls: ['zoomControl', 'typeSelector', 'fullscreenControl']
                        });
                        
                        // Создаем массив точек маршрута
                        var routePoints = [");

        for (int i = 0; i < coordinates.Length; i++)
        {
            if (i > 0) sb.Append(",");
            sb.Append($"'{coordinates[i]}'");
        }

        sb.Append(@"];
                        console.log('Route points:', routePoints);
                        
                        // Создаем мультимаршрут
                        var multiRoute = new ymaps.multiRouter.MultiRoute({
                            referencePoints: routePoints,
                            params: {
                                // Тип маршрутизации - пешеходная маршрутизация
                                routingMode: 'pedestrian',
                                // Включаем режим отображения только выбранного маршрута
                                results: 1
                            }
                        }, {
                            // Автоматически устанавливать границы карты так, чтобы маршрут был виден целиком
                            boundsAutoApply: true,
                            // Внешний вид линий маршрута
                            routeActiveStrokeWidth: 6,
                            routeActiveStrokeColor: '#1E90FF'
                        });
                        
                        // Добавляем мультимаршрут на карту
                        map.geoObjects.add(multiRoute);
                        
                        // Подписываемся на событие готовности маршрута
                        multiRoute.model.events.add('requestsuccess', function() {
                            console.log('Route built successfully');
                            
                            // Получаем информацию о маршруте
                            var activeRoute = multiRoute.getActiveRoute();
                            if (activeRoute) {
                                var length = activeRoute.properties.get('distance').text;
                                var duration = activeRoute.properties.get('duration').text;
                                
                                console.log('Route length:', length);
                                console.log('Route duration:', duration);
                                
                                // Отправляем информацию о маршруте в приложение через window.location
                                try {
                                    window.location.href = 'js://routeInfo?length=' + encodeURIComponent(length) + '&duration=' + encodeURIComponent(duration);
                                } catch (e) {
                                    console.log('Failed to send route info to app:', e);
                                }
                                
                                // Обновляем информацию о маршруте на странице
                                var routeInfoDiv = document.createElement('div');
                                routeInfoDiv.style.position = 'absolute';
                                routeInfoDiv.style.bottom = '80px';
                                routeInfoDiv.style.left = '10px';
                                routeInfoDiv.style.backgroundColor = 'white';
                                routeInfoDiv.style.padding = '10px';
                                routeInfoDiv.style.borderRadius = '5px';
                                routeInfoDiv.style.boxShadow = '0 0 10px rgba(0,0,0,0.3)';
                                routeInfoDiv.style.zIndex = '1000';
                                routeInfoDiv.innerHTML = '<strong>Длина маршрута:</strong> ' + length + '<br><strong>Время в пути:</strong> ' + duration;
                                document.body.appendChild(routeInfoDiv);
                            }
                        });
                        
                        // Подписываемся на событие ошибки при построении маршрута
                        multiRoute.model.events.add('requestfail', function(event) {
                            var error = event.get('error');
                            showError('Error building route: ' + (error ? error.message : 'Unknown error'));
                        });
                        
                        // Добавляем метки
                        var placemarks = [");

        for (int i = 0; i < coordinates.Length; i++)
        {
            if (i > 0) sb.Append(",");
            var name = i < placeNames.Length ? placeNames[i] : $"Точка {i + 1}";

            // Определяем цвет метки: зеленый для текущего местоположения, красный для последней точки, синий для остальных
            string color;
            if (i == 0)
                color = "'#00FF00'"; // Зеленый для текущего местоположения
            else if (i == coordinates.Length - 1)
                color = "'#FF0000'"; // Красный для последней точки
            else
                color = "'#1E90FF'"; // Синий для промежуточных точек

            sb.Append($@"
                            {{
                                coords: [{coordinates[i]}],
                                title: '{name.Replace("'", "\\'")}',
                                number: {i + 1},
                                color: {color}
                            }}");
        }

        sb.Append(@"
                        ];
                        
                        // Добавляем метки на карту
                        placemarks.forEach(function(point) {
                            try {
                                var placemark = new ymaps.Placemark(point.coords, {
                                    hintContent: point.title,
                                    balloonContent: point.number + '. ' + point.title
                                }, {
                                    preset: 'islands#' + point.color + 'CircleDotIconWithCaption',
                                    iconColor: point.color
                                });
                                
                                map.geoObjects.add(placemark);
                            } catch (e) {
                                showError('Error adding placemark: ' + e.message);
                            }
                        });
                        
                        // Добавляем элементы управления
                        map.controls.add(new ymaps.control.RouteButton({
                            options: {
                                float: 'right',
                                floatIndex: 100
                            }
                        }));
                        
                        // Добавляем кнопку определения местоположения
                        var geolocationControl = new ymaps.control.GeolocationControl({
                            options: {
                                float: 'left',
                                floatIndex: 100,
                                noPlacemark: false
                            }
                        });
                        map.controls.add(geolocationControl);
                        
                        // Добавляем кнопку для изменения типа маршрута
                        var routeTypeButton = new ymaps.control.Button({
                            data: {
                                content: 'Тип маршрута',
                                title: 'Изменить тип маршрута'
                            },
                            options: {
                                selectOnClick: false,
                                maxWidth: 150
                            }
                        });
                        
                        routeTypeButton.events.add('click', function() {
                            var items = [
                                {
                                    data: {
                                        content: 'Пешком',
                                        value: 'pedestrian'
                                    }
                                },
                                {
                                    data: {
                                        content: 'На машине',
                                        value: 'auto'
                                    }
                                },
                                {
                                    data: {
                                        content: 'Общественный транспорт',
                                        value: 'masstransit'
                                    }
                                }
                            ];
                            
                            var routeTypeMenu = new ymaps.control.ListBox({
                                data: {
                                    content: 'Тип маршрута'
                                },
                                items: items,
                                options: {
                                    position: {
                                        top: 60,
                                        right: 10
                                    }
                                }
                            });
                            
                            map.controls.add(routeTypeMenu);
                            
                            routeTypeMenu.events.add('click', function(e) {
                                var item = e.get('target');
                                if (item) {
                                    var routingMode = item.data.get('value');
                                    
                                    // Удаляем старый маршрут
                                    map.geoObjects.remove(multiRoute);
                                    
                                    // Создаем новый маршрут с выбранным типом
                                    multiRoute = new ymaps.multiRouter.MultiRoute({
                                        referencePoints: routePoints,
                                        params: {
                                            routingMode: routingMode,
                                            results: 1
                                        }
                                    }, {
                                        boundsAutoApply: true,
                                        routeActiveStrokeWidth: 6,
                                        routeActiveStrokeColor: '#1E90FF'
                                    });
                                    
                                    // Добавляем новый маршрут на карту
                                    map.geoObjects.add(multiRoute);
                                    
                                    // Удаляем меню после выбора
                                    map.controls.remove(routeTypeMenu);
                                }
                            });
                        });
                        
                        map.controls.add(routeTypeButton, {
                            float: 'right',
                            floatIndex: 100
                        });
                        
                        // Добавляем обработчик для кнопки местоположения
                        document.getElementById('location-button').addEventListener('click', function() {
                            console.log('Location button clicked');
                            if (navigator.geolocation) {
                                console.log('Geolocation API is available');
                                navigator.geolocation.getCurrentPosition(
                                    function(position) {
                                        console.log('Position received:', position.coords.latitude, position.coords.longitude);
                                        var userLocation = [position.coords.latitude, position.coords.longitude];
                                        var accuracy = position.coords.accuracy;
                                        
                                        // Удаляем предыдущие объекты местоположения, если они есть
                                        if (window.userLocationPlacemark) {
                                            map.geoObjects.remove(window.userLocationPlacemark);
                                        }
                                        if (window.accuracyCircle) {
                                            map.geoObjects.remove(window.accuracyCircle);
                                        }
                                        
                                        // Создаем метку текущего местоположения
                                        window.userLocationPlacemark = new ymaps.Placemark(userLocation, {
                                            hintContent: 'Ваше местоположение',
                                            balloonContent: 'Вы находитесь здесь<br>Точность: ' + Math.round(accuracy) + ' м'
                                        }, {
                                            preset: 'islands#geolocationIcon',
                                            iconColor: '#4285F4'
                                        });
                                        
                                        // Создаем круг, показывающий точность определения местоположения
                                        window.accuracyCircle = new ymaps.Circle([userLocation, accuracy], {
                                            hintContent: 'Точность: ' + Math.round(accuracy) + ' м'
                                        }, {
                                            draggable: false,
                                            fillColor: '#4285F4',
                                            fillOpacity: 0.2,
                                            strokeColor: '#4285F4',
                                            strokeOpacity: 0.6,
                                            strokeWidth: 1
                                        });
                                        
                                        // Добавляем объекты на карту
                                        map.geoObjects.add(window.userLocationPlacemark);
                                        map.geoObjects.add(window.accuracyCircle);
                                        
                                        // Центрируем карту на местоположении пользователя
                                        map.setCenter(userLocation, 16, {
                                            duration: 500
                                        });
                                    },
                                    function(error) {
                                        console.error('Geolocation error:', error.code, error.message);
                                        var errorMessage = '';
                                        switch(error.code) {
                                            case error.PERMISSION_DENIED:
                                                errorMessage = 'Пользователь отказал в доступе к геолокации';
                                                break;
                                            case error.POSITION_UNAVAILABLE:
                                                errorMessage = 'Информация о местоположении недоступна';
                                                break;
                                            case error.TIMEOUT:
                                                errorMessage = 'Истекло время ожидания запроса местоположения';
                                                break;
                                            case error.UNKNOWN_ERROR:
                                                errorMessage = 'Произошла неизвестная ошибка';
                                                break;
                                        }
                                        showError('Ошибка геолокации: ' + errorMessage);
                                    },
                                    {
                                        enableHighAccuracy: true,
                                        timeout: 10000,
                                        maximumAge: 0
                                    }
                                );
                            } else {
                                showError('Геолокация не поддерживается вашим браузером');
                            }
                        });
                        
                    } catch (e) {
                        showError('Error in init function: ' + e.message);
                    }
                }
            </script>
        </body>
        </html>");

        return sb.ToString();
    }

    private string GenerateCurrentLocationHtml(double latitude, double longitude, double accuracy)
    {
        var sb = new StringBuilder();
        sb.Append(@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
                <title>Моё местоположение</title>
                <script src=""https://api-maps.yandex.ru/2.1/?apikey=");

        sb.Append(_apiKey);

        sb.Append(@"&lang=ru_RU"" type=""text/javascript""></script>
                <style>
                    html, body, #map {
                        width: 100%; 
                        height: 100%; 
                        padding: 0; 
                        margin: 0;
                    }
                    .error-message {
                        position: absolute;
                        top: 10px;
                        left: 10px;
                        background: white;
                        padding: 10px;
                        border-radius: 5px;
                        box-shadow: 0 0 10px rgba(0,0,0,0.3);
                        z-index: 1000;
                        display: none;
                    }
                    .accuracy-circle {
                        stroke: #4285F4;
                        stroke-opacity: 0.6;
                        stroke-width: 1;
                        fill: #4285F4;
                        fill-opacity: 0.2;
                    }
                    .location-button {
                        position: absolute;
                        bottom: 20px;
                        right: 20px;
                        background: white;
                        border: none;
                        border-radius: 50%;
                        width: 50px;
                        height: 50px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.3);
                        cursor: pointer;
                        z-index: 1000;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                    }
                    .location-button:focus {
                        outline: none;
                    }
                </style>
            </head>
            <body>
                <div id=""map""></div>
                <div id=""error-message"" class=""error-message""></div>
                <button id=""location-button"" class=""location-button"" title=""Моё местоположение"">
                    <svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                        <path d=""M12 8C9.79 8 8 9.79 8 12C8 14.21 9.79 16 12 16C14.21 16 16 14.21 16 12C16 9.79 14.21 8 12 8ZM20.94 11C20.48 6.83 17.17 3.52 13 3.06V1H11V3.06C6.83 3.52 3.52 6.83 3.06 11H1V13H3.06C3.52 17.17 6.83 20.48 11 20.94V23H13V20.94C17.17 20.48 20.48 17.17 20.94 13H23V11H20.94ZM12 19C8.13 19 5 15.87 5 12C5 8.13 8.13 5 12 5C15.87 5 19 8.13 19 12C19 15.87 15.87 19 12 19Z"" fill=""#4285F4""/>
                    </svg>
                </button>
                <script>
                    // Функция для отображения ошибок
                    function showError(message) {
                        var errorDiv = document.getElementById('error-message');
                        errorDiv.textContent = message;
                        errorDiv.style.display = 'block';
                        console.error(message);
                    }

                    // Обработка глобальных ошибок JavaScript
                    window.onerror = function(message, source, lineno, colno, error) {
                        showError('JavaScript error: ' + message);
                        return true;
                    };

                    try {
                        ymaps.ready(init);
                    } catch (e) {
                        showError('Error loading Yandex Maps: ' + e.message);
                    }
                    
                    function init() {
                        try {
                            console.log('Initializing map with current location...');
                            
                            // Координаты текущего местоположения
                            var userLocation = [");

        sb.Append($"{latitude.ToString(CultureInfo.InvariantCulture)}, {longitude.ToString(CultureInfo.InvariantCulture)}");

        sb.Append(@"];
                            var accuracy = ");

        sb.Append(accuracy.ToString(CultureInfo.InvariantCulture));

        sb.Append(@";
                            
                            console.log('Initial location:', userLocation, 'Accuracy:', accuracy);
                            
                            var map = new ymaps.Map('map', {
                                center: userLocation,
                                zoom: 16,
                                controls: ['zoomControl', 'typeSelector', 'fullscreenControl']
                            });
                            
                            // Создаем метку текущего местоположения
                            var userLocationPlacemark = new ymaps.Placemark(userLocation, {
                                hintContent: 'Ваше местоположение',
                                balloonContent: 'Вы находитесь здесь<br>Точность: ' + Math.round(accuracy) + ' м'
                            }, {
                                preset: 'islands#geolocationIcon',
                                iconColor: '#4285F4'
                            });
                            
                            // Создаем круг, показывающий точность определения местоположения
                            var accuracyCircle = new ymaps.Circle([userLocation, accuracy], {
                                hintContent: 'Точность: ' + Math.round(accuracy) + ' м'
                            }, {
                                draggable: false,
                                fillColor: '#4285F4',
                                fillOpacity: 0.2,
                                strokeColor: '#4285F4',
                                strokeOpacity: 0.6,
                                strokeWidth: 1
                            });
                            
                            // Добавляем объекты на карту
                            map.geoObjects.add(userLocationPlacemark);
                            map.geoObjects.add(accuracyCircle);
                            
                            // Добавляем кнопку определения местоположения
                            var geolocationControl = new ymaps.control.GeolocationControl({
                                options: {
                                    float: 'left',
                                    floatIndex: 100,
                                    noPlacemark: true
                                }
                            });
                            map.controls.add(geolocationControl);
                            
                            // Добавляем обработчик для кнопки местоположения
                            document.getElementById('location-button').addEventListener('click', function() {
                                console.log('Location button clicked');
                                if (navigator.geolocation) {
                                    console.log('Geolocation API is available');
                                    navigator.geolocation.getCurrentPosition(
                                        function(position) {
                                            console.log('Position received:', position.coords.latitude, position.coords.longitude);
                                            var newUserLocation = [position.coords.latitude, position.coords.longitude];
                                            var newAccuracy = position.coords.accuracy;
                                            
                                            // Удаляем предыдущие объекты
                                            map.geoObjects.remove(userLocationPlacemark);
                                            map.geoObjects.remove(accuracyCircle);
                                            
                                            // Создаем новую метку
                                            userLocationPlacemark = new ymaps.Placemark(newUserLocation, {
                                                hintContent: 'Ваше местоположение',
                                                balloonContent: 'Вы находитесь здесь<br>Точность: ' + Math.round(newAccuracy) + ' м'
                                            }, {
                                                preset: 'islands#geolocationIcon',
                                                iconColor: '#4285F4'
                                            });
                                            
                                            // Создаем новый круг точности
                                            accuracyCircle = new ymaps.Circle([newUserLocation, newAccuracy], {
                                                hintContent: 'Точность: ' + Math.round(newAccuracy) + ' м'
                                            }, {
                                                draggable: false,
                                                fillColor: '#4285F4',
                                                fillOpacity: 0.2,
                                                strokeColor: '#4285F4',
                                                strokeOpacity: 0.6,
                                                strokeWidth: 1
                                            });
                                            
                                            // Добавляем объекты на карту
                                            map.geoObjects.add(userLocationPlacemark);
                                            map.geoObjects.add(accuracyCircle);
                                            
                                            // Центрируем карту на новом местоположении
                                            map.setCenter(newUserLocation, 16, {
                                                duration: 500
                                            });
                                        },
                                        function(error) {
                                            console.error('Geolocation error:', error.code, error.message);
                                            var errorMessage = '';
                                            switch(error.code) {
                                                case error.PERMISSION_DENIED:
                                                    errorMessage = 'Пользователь отказал в доступе к геолокации';
                                                break;
                                                case error.POSITION_UNAVAILABLE:
                                                    errorMessage = 'Информация о местоположении недоступна';
                                                break;
                                                case error.TIMEOUT:
                                                    errorMessage = 'Истекло время ожидания запроса местоположения';
                                                    break;
                                                case error.UNKNOWN_ERROR:
                                                    errorMessage = 'Произошла неизвестная ошибка';
                                                    break;
                                            }
                                            showError('Ошибка геолокации: ' + errorMessage);
                                        },
                                        {
                                            enableHighAccuracy: true,
                                            timeout: 10000,
                                            maximumAge: 0
                                        }
                                    );
                                } else {
                                    showError('Геолокация не поддерживается вашим браузером');
                                }
                            });
                            
                            // Запускаем отслеживание местоположения
                            startLocationTracking(map, userLocationPlacemark, accuracyCircle);
                            
                        } catch (e) {
                            showError('Error in init function: ' + e.message);
                        }
                    }
                    
                    // Функция для отслеживания местоположения в реальном времени
                    function startLocationTracking(map, placemark, circle) {
                        if (navigator.geolocation) {
                            var watchId = navigator.geolocation.watchPosition(
                                function(position) {
                                    var newUserLocation = [position.coords.latitude, position.coords.longitude];
                                    var newAccuracy = position.coords.accuracy;
                                    
                                    // Обновляем геометрию круга точности
                                    circle.geometry.setRadius(newAccuracy);
                                    circle.geometry.setCoordinates(newUserLocation);
                                    
                                    // Обновляем позицию метки
                                    placemark.geometry.setCoordinates(newUserLocation);
                                    
                                    // Обновляем содержимое балуна
                                    placemark.properties.set('balloonContent', 'Вы находитесь здесь<br>Точность: ' + Math.round(newAccuracy) + ' м');
                                    
                                    // Центрируем карту, если точность улучшилась значительно
                                    if (window.lastAccuracy && window.lastAccuracy > newAccuracy * 1.5) {
                                        map.setCenter(newUserLocation, 16, {
                                            duration: 500
                                        });
                                    }
                                    
                                    window.lastAccuracy = newAccuracy;
                                },
                                function(error) {
                                    console.error('Ошибка отслеживания местоположения:', error.message);
                                },
                                {
                                    enableHighAccuracy: true,
                                    timeout: 10000,
                                    maximumAge: 0
                                }
                            );
                            
                            // Сохраняем ID для возможности остановки отслеживания
                            window.locationWatchId = watchId;
                        }
                    }
                </script>
            </body>
            </html>");

        return sb.ToString();
    }

    private void LoadDefaultMap()
    {
        var html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
                <title>Яндекс Карты</title>
                <script src=""https://api-maps.yandex.ru/2.1/?apikey=" + _apiKey + @"&lang=ru_RU"" type=""text/javascript""></script>
                <style>
                    html, body, #map {
                        width: 100%; 
                        height: 100%; 
                        padding: 0; 
                        margin: 0;
                    }
                    .error-message {
                        position: absolute;
                        top: 10px;
                        left: 10px;
                        background: white;
                        padding: 10px;
                        border-radius: 5px;
                        box-shadow: 0 0 10px rgba(0,0,0,0.3);
                        z-index: 1000;
                        display: none;
                    }
                    .location-button {
                        position: absolute;
                        bottom: 20px;
                        right: 20px;
                        background: white;
                        border: none;
                        border-radius: 50%;
                        width: 50px;
                        height: 50px;
                        box-shadow: 0 2px 6px rgba(0,0,0,0.3);
                        cursor: pointer;
                        z-index: 1000;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                    }
                    .location-button:focus {
                        outline: none;
                    }
                </style>
            </head>
            <body>
                <div id=""map""></div>
                <div id=""error-message"" class=""error-message""></div>
                <button id=""location-button"" class=""location-button"" title=""Моё местоположение"">
                    <svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                        <path d=""M12 8C9.79 8 8 9.79 8 12C8 14.21 9.79 16 12 16C14.21 16 16 14.21 16 12C16 9.79 14.21 8 12 8ZM20.94 11C20.48 6.83 17.17 3.52 13 3.06V1H11V3.06C6.83 3.52 3.52 6.83 3.06 11H1V13H3.06C3.52 17.17 6.83 20.48 11 20.94V23H13V20.94C17.17 20.48 20.48 17.17 20.94 13H23V11H20.94ZM12 19C8.13 19 5 15.87 5 12C5 8.13 8.13 5 12 5C15.87 5 19 8.13 19 12C19 15.87 15.87 19 12 19Z"" fill=""#4285F4""/>
                    </svg>
                </button>
                <script>
                    // Функция для отображения ошибок
                    function showError(message) {
                        var errorDiv = document.getElementById('error-message');
                        errorDiv.textContent = message;
                        errorDiv.style.display = 'block';
                        console.error(message);
                    }

                    // Обработка глобальных ошибок JavaScript
                    window.onerror = function(message, source, lineno, colno, error) {
                        showError('JavaScript error: ' + message);
                        return true;
                    };

                    try {
                        ymaps.ready(init);
                    } catch (e) {
                        showError('Error loading Yandex Maps: ' + e.message);
                    }
                    
                    function init() {
                        try {
                            console.log('Initializing default map...');
                            
                            var map = new ymaps.Map('map', {
                                center: [55.751574, 37.573856],
                                zoom: 10,
                                controls: ['zoomControl', 'typeSelector', 'fullscreenControl']
                            });
                            
                            // Добавляем кнопку определения местоположения
                            var geolocationControl = new ymaps.control.GeolocationControl({
                                options: {
                                    float: 'left',
                                    floatIndex: 100,
                                    noPlacemark: false
                                }
                            });
                            map.controls.add(geolocationControl);
                            
                            // Добавляем обработчик для кнопки местоположения
                            document.getElementById('location-button').addEventListener('click', function() {
                                console.log('Location button clicked');
                                if (navigator.geolocation) {
                                    console.log('Geolocation API is available');
                                    navigator.geolocation.getCurrentPosition(
                                        function(position) {
                                            console.log('Position received:', position.coords.latitude, position.coords.longitude);
                                            var userLocation = [position.coords.latitude, position.coords.longitude];
                                            var accuracy = position.coords.accuracy;
                                            
                                            // Удаляем предыдущие объекты местоположения, если они есть
                                            if (window.userLocationPlacemark) {
                                                map.geoObjects.remove(window.userLocationPlacemark);
                                            }
                                            if (window.accuracyCircle) {
                                                map.geoObjects.remove(window.accuracyCircle);
                                            }
                                            
                                            // Создаем метку текущего местоположения
                                            window.userLocationPlacemark = new ymaps.Placemark(userLocation, {
                                                hintContent: 'Ваше местоположение',
                                                balloonContent: 'Вы находитесь здесь<br>Точность: ' + Math.round(accuracy) + ' м'
                                            }, {
                                                preset: 'islands#geolocationIcon',
                                                iconColor: '#4285F4'
                                            });
                                            
                                            // Создаем круг, показывающий точность определения местоположения
                                            window.accuracyCircle = new ymaps.Circle([userLocation, accuracy], {
                                                hintContent: 'Точность: ' + Math.round(accuracy) + ' м'
                                            }, {
                                                draggable: false,
                                                fillColor: '#4285F4',
                                                fillOpacity: 0.2,
                                                strokeColor: '#4285F4',
                                                strokeOpacity: 0.6,
                                                strokeWidth: 1
                                            });
                                            
                                            // Добавляем объекты на карту
                                            map.geoObjects.add(window.userLocationPlacemark);
                                            map.geoObjects.add(window.accuracyCircle);
                                            
                                            // Центрируем карту на местоположении пользователя
                                            map.setCenter(userLocation, 16, {
                                                duration: 500
                                            });
                                            
                                            // Запускаем отслеживание местоположения
                                            startLocationTracking(map, window.userLocationPlacemark, window.accuracyCircle);
                                        },
                                        function(error) {
                                            console.error('Geolocation error:', error.code, error.message);
                                            var errorMessage = '';
                                            switch(error.code) {
                                                case error.PERMISSION_DENIED:
                                                    errorMessage = 'Пользователь отказал в доступе к геолокации';
                                                    break;
                                                case error.POSITION_UNAVAILABLE:
                                                    errorMessage = 'Информация о местоположении недоступна';
                                                    break;
                                                case error.TIMEOUT:
                                                    errorMessage = 'Истекло время ожидания запроса местоположения';
                                                    break;
                                                case error.UNKNOWN_ERROR:
                                                    errorMessage = 'Произошла неизвестная ошибка';
                                                    break;
                                            }
                                            showError('Ошибка геолокации: ' + errorMessage);
                                        },
                                        {
                                            enableHighAccuracy: true,
                                            timeout: 10000,
                                            maximumAge: 0
                                        }
                                    );
                                } else {
                                    showError('Геолокация не поддерживается вашим браузером');
                                }
                            });
                            
                            // Функция для отслеживания местоположения в реальном времени
                            function startLocationTracking(map, placemark, circle) {
                                if (navigator.geolocation) {
                                    var watchId = navigator.geolocation.watchPosition(
                                        function(position) {
                                            var newUserLocation = [position.coords.latitude, position.coords.longitude];
                                            var newAccuracy = position.coords.accuracy;
                                            
                                            // Обновляем геометрию круга точности
                                            circle.geometry.setRadius(newAccuracy);
                                            circle.geometry.setCoordinates(newUserLocation);
                                            
                                            // Обновляем позицию метки
                                            placemark.geometry.setCoordinates(newUserLocation);
                                            
                                            // Обновляем содержимое балуна
                                            placemark.properties.set('balloonContent', 'Вы находитесь здесь<br>Точность: ' + Math.round(newAccuracy) + ' м');
                                            
                                            // Центрируем карту, если точность улучшилась значительно
                                            if (window.lastAccuracy && window.lastAccuracy > newAccuracy * 1.5) {
                                                map.setCenter(newUserLocation, 16, {
                                                    duration: 500
                                                });
                                            }
                                            
                                            window.lastAccuracy = newAccuracy;
                                        },
                                        function(error) {
                                            console.error('Ошибка отслеживания местоположения:', error.message);
                                        },
                                        {
                                            enableHighAccuracy: true,
                                            timeout: 10000,
                                            maximumAge: 0
                                        }
                                    );
                                    
                                    // Сохраняем ID для возможности остановки отслеживания
                                    window.locationWatchId = watchId;
                                }
                            }
                            
                            // Пробуем сразу определить местоположение
                            geolocationControl.events.add('click', function() {
                                document.getElementById('location-button').click();
                            });
                            
                            // Автоматически запускаем определение местоположения
                            setTimeout(function() {
                                document.getElementById('location-button').click();
                            }, 1000);
                            
                        } catch (e) {
                            showError('Error in init function: ' + e.message);
                        }
                    }
                </script>
            </body>
            </html>";

        MapWebView.Source = new HtmlWebViewSource { Html = html };
    }

    // Обновляем метод LoadMapWithRoute, чтобы скрывать индикатор загрузки после загрузки карты
    private void LoadMapWithRoute(string coords, string names)
    {
        try
        {
            Debug.WriteLine("Загрузка карты с маршрутом...");

            var coordinates = coords.Split('|');
            var placeNames = names.Split('|');

            Debug.WriteLine($"Количество координат: {coordinates.Length}");
            Debug.WriteLine($"Количество названий: {placeNames.Length}");

            if (coordinates.Length < 2)
            {
                Debug.WriteLine("Недостаточно точек для построения маршрута");
                DisplayAlert("Ошибка", "Недостаточно точек для построения маршрута", "OK");
                LoadMapWithCurrentLocationAsync().ConfigureAwait(false);
                return;
            }

            var html = GenerateRouteHtml(coordinates, placeNames);

            // Сохраняем HTML в файл для отладки
            SaveHtmlToFile(html);

            // Добавляем обработчик для перехвата сообщений от JavaScript
            MapWebView.Navigating += WebView_Navigating;

            MapWebView.Source = new HtmlWebViewSource { Html = html };

            // Добавляем обработчик ошибок JavaScript
            MapWebView.Navigated += (sender, e) => {
                Debug.WriteLine($"WebView загружен: {e.Url}");
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            };

            MapWebView.Navigating += (sender, e) => {
                Debug.WriteLine($"WebView загружается: {e.Url}");
            };

            // Обновляем информацию о маршруте
            UpdateRouteInfo(coordinates.Length);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки карты: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            DisplayAlert("Ошибка", $"Ошибка загрузки карты: {ex.Message}", "OK");
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            LoadMapWithCurrentLocationAsync().ConfigureAwait(false);
        }
    }

    // Обработчик для перехвата сообщений от JavaScript
    private void WebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("js://"))
        {
            e.Cancel = true; // Отменяем навигацию по этому URL

            // Парсим URL для получения информации о маршруте
            var uri = new Uri(e.Url);
            var query = uri.Query.TrimStart('?');
            var parameters = ParseQueryParameters(query);

            if (parameters.TryGetValue("length", out var length) &&
                parameters.TryGetValue("duration", out var duration))
            {
                // Сохраняем информацию о маршруте
                _routeDistance = length;
                _routeDuration = duration;

                // Обновляем интерфейс
                UpdateRouteInfoWithDetails(length, duration);
            }
        }
    }

    // Обновляем метод UpdateRouteInfo, чтобы показывать более подробную информацию
    private void UpdateRouteInfo(int pointsCount)
    {
        // Обновляем текст метки с информацией о маршруте
        if (pointsCount > 0)
        {
            string startPoint = pointsCount > 0 ? "вашего местоположения" : "неизвестной точки";
            RouteInfoLabel.Text = $"Маршрут от {startPoint} через {pointsCount - 1} точек";
        }
        else
        {
            RouteInfoLabel.Text = "Маршрут не построен";
        }
        RouteInfoLabel.TextColor = Colors.Black;
    }

    // Новый метод для обновления информации о маршруте с деталями
    private void UpdateRouteInfoWithDetails(string distance, string duration)
    {
        // Обновляем информацию о маршруте в интерфейсе
        Device.BeginInvokeOnMainThread(() => {
            RouteDistanceLabel.Text = $"Расстояние: {distance}";
            RouteDurationLabel.Text = $"Время в пути: {duration}";

            // Делаем метки видимыми
            RouteDistanceLabel.IsVisible = true;
            RouteDurationLabel.IsVisible = true;
        });
    }

    // Добавляем метод SaveHtmlToFile, который был удален
    private void SaveHtmlToFile(string html)
    {
        try
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, "map_debug.html");
            File.WriteAllText(filePath, html);
            Debug.WriteLine($"HTML сохранен в файл: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при сохранении HTML: {ex.Message}");
        }
    }

    // Добавляем метод LoadMapWithCurrentLocationAsync, который был удален
    private async Task LoadMapWithCurrentLocationAsync()
    {
        try
        {
            var location = await GetCurrentLocationAsync();

            if (location != null)
            {
                var html = GenerateCurrentLocationHtml(location.Latitude, location.Longitude, location.Accuracy ?? 0);
                MapWebView.Source = new HtmlWebViewSource { Html = html };
            }
            else
            {
                LoadDefaultMap();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при загрузке карты с текущим местоположением: {ex.Message}");
            LoadDefaultMap();
        }
    }
}
