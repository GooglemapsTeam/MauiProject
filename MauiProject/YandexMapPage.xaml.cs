using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Emotional_Map;

public partial class YandexMapPage : ContentPage
{
    private string _apiKey = "69f697cc-32fb-4058-8056-a615983e7e93"; // Замените на ваш API-ключ Яндекс Карт

    public YandexMapPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

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

            LoadDefaultMap();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в OnAppearing: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            LoadDefaultMap();
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
                LoadDefaultMap();
                return;
            }

            var html = GenerateRouteHtml(coordinates, placeNames);

            // Сохраняем HTML в файл для отладки
            SaveHtmlToFile(html);

            MapWebView.Source = new HtmlWebViewSource { Html = html };

            // Добавляем обработчик ошибок JavaScript
            MapWebView.Navigated += (sender, e) => {
                Debug.WriteLine($"WebView загружен: {e.Url}");
            };

            MapWebView.Navigating += (sender, e) => {
                Debug.WriteLine($"WebView загружается: {e.Url}");
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки карты: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            DisplayAlert("Ошибка", $"Ошибка загрузки карты: {ex.Message}", "OK");
            LoadDefaultMap();
        }
    }

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
                </style>
            </head>
            <body>
                <div id=""map""></div>
                <div id=""error-message"" class=""error-message""></div>
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
                                zoom: 12,
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
            sb.Append($@"
                                {{
                                    coords: [{coordinates[i]}],
                                    title: '{name.Replace("'", "\\'")}',
                                    number: {i + 1},
                                    color: {(i == 0 ? "'#00FF00'" : i == coordinates.Length - 1 ? "'#FF0000'" : "'#1E90FF'")}
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
                                    floatIndex: 100
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
                            
                        } catch (e) {
                            showError('Error in init function: ' + e.message);
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
                </style>
            </head>
            <body>
                <div id=""map""></div>
                <div id=""error-message"" class=""error-message""></div>
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
                            var map = new ymaps.Map('map', {
                                center: [55.751574, 37.573856],
                                zoom: 10,
                                controls: ['zoomControl', 'typeSelector', 'fullscreenControl']
                            });
                            
                            // Добавляем кнопку определения местоположения
                            var geolocationControl = new ymaps.control.GeolocationControl({
                                options: {
                                    float: 'left',
                                    floatIndex: 100
                                }
                            });
                            map.controls.add(geolocationControl);
                            
                        } catch (e) {
                            showError('Error in init function: ' + e.message);
                        }
                    }
                </script>
            </body>
            </html>";

        MapWebView.Source = new HtmlWebViewSource { Html = html };
    }
}