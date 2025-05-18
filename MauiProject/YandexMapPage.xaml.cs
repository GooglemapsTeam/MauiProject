using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Emotional_Map;

public partial class YandexMapPage : ContentPage
{
    private string _apiKey = "69f697cc-32fb-4058-8056-a615983e7e93"; // �������� �� ��� API-���� ������ ����
    private bool _isLocationPermissionGranted = false;

    // ���������������� ����� �������� (���������� ���������������������� �������������)
    private readonly string[] _predefinedPoints = new string[]
    {
        "56.844106, 60.645473", // ������ �����
        "56.837527, 60.604722", // �������� ������������
        "56.826389, 60.631111", // ����������
        "56.821667, 60.621944"  // ������� ����
    };

    // �������� ����� ��������
    private readonly string[] _predefinedNames = new string[]
    {
        "������ �����",
        "��������",
        "����������",
        "������� ����"
    };

    // ���������� � ��������
    private string _routeDistance = "";
    private string _routeDuration = "";

    public YandexMapPage()
    {
        InitializeComponent();

        // ��������� ���������� ������� �������� WebView
        MapWebView.Navigated += (sender, e) => {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        };

        // ��������� ���������� ��� ������ ���������� ��������
        BuildRouteButton.Clicked += (sender, e) => {
            BuildPredefinedRoute();
        };

        // �������� ������ ������� � ������ ����������
#if DEBUG
        DebugButton.IsVisible = true;
        DebugButton.Clicked += async (sender, e) => {
            await DisplayAlert("���������� ����������",
                $"���������� �� ����������: {_isLocationPermissionGranted}\n" +
                $"������� URL: {MapWebView.Source}\n" +
                $"����������: {_routeDistance}\n" +
                $"����� � ����: {_routeDuration}",
                "OK");
        };
#endif
    }

    // ���������� ����� BuildPredefinedRoute, ����� �������� ������������ ����������
    private async void BuildPredefinedRoute()
    {
        try
        {
            // ���������� ��������� ��������
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            // �������� ������� �������������� ������������
            var currentLocation = await GetCurrentLocationAsync();

            List<string> coordinates = new List<string>();
            List<string> names = new List<string>();

            if (currentLocation == null)
            {
                await DisplayAlert("��������", "�� ������� ���������� ���� ��������������. ������� ����� �������� ��� ����� ������ ��������������.", "OK");

                // ������ ������� ������ ����� ���������������� �����
                coordinates.AddRange(_predefinedPoints);
                names.AddRange(_predefinedNames);
            }
            else
            {
                // ��������� ������� �������������� ��� ������ �����
                coordinates.Add($"{currentLocation.Latitude.ToString(CultureInfo.InvariantCulture)}, {currentLocation.Longitude.ToString(CultureInfo.InvariantCulture)}");
                names.Add("���� ��������������");

                // ��������� ���������������� �����
                coordinates.AddRange(_predefinedPoints);
                names.AddRange(_predefinedNames);
            }

            // ���������� ���������� � �������� � ������ � ������������ |
            var routeCoordsString = string.Join("|", coordinates);
            var routeNamesString = string.Join("|", names);

            // ������ �������
            LoadMapWithRoute(routeCoordsString, routeNamesString);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"������ ��� ���������� ����������������� ��������: {ex.Message}");
            await DisplayAlert("������", "�� ������� ��������� �������", "OK");
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // ����������� ���������� �� ������������� ����������
        _isLocationPermissionGranted = await RequestLocationPermissionAsync();

        try
        {
            // �������� ��������� ����� Query
            var route = Shell.Current.CurrentState.Location.ToString();
            Debug.WriteLine($"������� �������: {route}");

            var queryStart = route.IndexOf('?');

            if (queryStart > 0)
            {
                var query = route.Substring(queryStart + 1);
                Debug.WriteLine($"��������� �������: {query}");

                var parameters = ParseQueryParameters(query);

                foreach (var param in parameters)
                {
                    Debug.WriteLine($"��������: {param.Key} = {param.Value}");
                }

                if (parameters.TryGetValue("coords", out var coords) &&
                    parameters.TryGetValue("names", out var names))
                {
                    Debug.WriteLine($"����������: {coords}");
                    Debug.WriteLine($"��������: {names}");

                    LoadMapWithRoute(coords, names);
                    return;
                }
                else
                {
                    Debug.WriteLine("�� ������� ��������� coords ��� names");
                }
            }
            else
            {
                Debug.WriteLine("�� ������ ������ '?' � URL");
            }

            // ���� �� ������� ��������� �������, ��������� ����� � ������� ���������������
            await LoadMapWithCurrentLocationAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"������ � OnAppearing: {ex.Message}");
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
            Debug.WriteLine($"������ ��� ������� ���������� �� ����������: {ex.Message}");
            return false;
        }
    }

    private async Task<Location> GetCurrentLocationAsync()
    {
        try
        {
            if (!_isLocationPermissionGranted)
            {
                Debug.WriteLine("��� ���������� �� ������������� ����������");
                return null;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location != null)
            {
                Debug.WriteLine($"������� ����������: {location.Latitude}, {location.Longitude}, ��������: {location.Accuracy} ������");
                return location;
            }

            Debug.WriteLine("�� ������� �������� ������� ��������������");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"������ ��� ��������� �������� ��������������: {ex.Message}");
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

    // ��������� ����� GenerateRouteHtml, ����� ��������� ���������� ������ �������� ��������������
    private string GenerateRouteHtml(string[] coordinates, string[] placeNames)
    {
        var sb = new StringBuilder();
        sb.Append(@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset=""utf-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
            <title>�������</title>
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
            <button id=""location-button"" class=""location-button"" title=""�� ��������������"">
                <svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                    <path d=""M12 8C9.79 8 8 9.79 8 12C8 14.21 9.79 16 12 16C14.21 16 16 14.21 16 12C16 9.79 14.21 8 12 8ZM20.94 11C20.48 6.83 17.17 3.52 13 3.06V1H11V3.06C6.83 3.52 3.52 6.83 3.06 11H1V13H3.06C3.52 17.17 6.83 20.48 11 20.94V23H13V20.94C17.17 20.48 20.48 17.17 20.94 13H23V11H20.94ZM12 19C8.13 19 5 15.87 5 12C5 8.13 8.13 5 12 5C15.87 5 19 8.13 19 12C19 15.87 15.87 19 12 19Z"" fill=""#4285F4""/>
                </svg>
            </button>
            <script>
                // ������� ��� ����������� ������
                function showError(message) {
                    var errorDiv = document.getElementById('error-message');
                    errorDiv.textContent = message;
                    errorDiv.style.display = 'block';
                    console.error(message);
                }

                // ��������� ���������� ������ JavaScript
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
                        
                        // ��������� ����������
                        var firstCoord = [");

        sb.Append(coordinates[0]);

        sb.Append(@"];
                        console.log('First coordinate:', firstCoord);
                        
                        var map = new ymaps.Map('map', {
                            center: firstCoord,
                            zoom: 16,
                            controls: ['zoomControl', 'typeSelector', 'fullscreenControl']
                        });
                        
                        // ������� ������ ����� ��������
                        var routePoints = [");

        for (int i = 0; i < coordinates.Length; i++)
        {
            if (i > 0) sb.Append(",");
            sb.Append($"'{coordinates[i]}'");
        }

        sb.Append(@"];
                        console.log('Route points:', routePoints);
                        
                        // ������� �������������
                        var multiRoute = new ymaps.multiRouter.MultiRoute({
                            referencePoints: routePoints,
                            params: {
                                // ��� ������������� - ���������� �������������
                                routingMode: 'pedestrian',
                                // �������� ����� ����������� ������ ���������� ��������
                                results: 1
                            }
                        }, {
                            // ������������� ������������� ������� ����� ���, ����� ������� ��� ����� �������
                            boundsAutoApply: true,
                            // ������� ��� ����� ��������
                            routeActiveStrokeWidth: 6,
                            routeActiveStrokeColor: '#1E90FF'
                        });
                        
                        // ��������� ������������� �� �����
                        map.geoObjects.add(multiRoute);
                        
                        // ������������� �� ������� ���������� ��������
                        multiRoute.model.events.add('requestsuccess', function() {
                            console.log('Route built successfully');
                            
                            // �������� ���������� � ��������
                            var activeRoute = multiRoute.getActiveRoute();
                            if (activeRoute) {
                                var length = activeRoute.properties.get('distance').text;
                                var duration = activeRoute.properties.get('duration').text;
                                
                                console.log('Route length:', length);
                                console.log('Route duration:', duration);
                                
                                // ���������� ���������� � �������� � ���������� ����� window.location
                                try {
                                    window.location.href = 'js://routeInfo?length=' + encodeURIComponent(length) + '&duration=' + encodeURIComponent(duration);
                                } catch (e) {
                                    console.log('Failed to send route info to app:', e);
                                }
                                
                                // ��������� ���������� � �������� �� ��������
                                var routeInfoDiv = document.createElement('div');
                                routeInfoDiv.style.position = 'absolute';
                                routeInfoDiv.style.bottom = '80px';
                                routeInfoDiv.style.left = '10px';
                                routeInfoDiv.style.backgroundColor = 'white';
                                routeInfoDiv.style.padding = '10px';
                                routeInfoDiv.style.borderRadius = '5px';
                                routeInfoDiv.style.boxShadow = '0 0 10px rgba(0,0,0,0.3)';
                                routeInfoDiv.style.zIndex = '1000';
                                routeInfoDiv.innerHTML = '<strong>����� ��������:</strong> ' + length + '<br><strong>����� � ����:</strong> ' + duration;
                                document.body.appendChild(routeInfoDiv);
                            }
                        });
                        
                        // ������������� �� ������� ������ ��� ���������� ��������
                        multiRoute.model.events.add('requestfail', function(event) {
                            var error = event.get('error');
                            showError('Error building route: ' + (error ? error.message : 'Unknown error'));
                        });
                        
                        // ��������� �����
                        var placemarks = [");

        for (int i = 0; i < coordinates.Length; i++)
        {
            if (i > 0) sb.Append(",");
            var name = i < placeNames.Length ? placeNames[i] : $"����� {i + 1}";

            // ���������� ���� �����: ������� ��� �������� ��������������, ������� ��� ��������� �����, ����� ��� ���������
            string color;
            if (i == 0)
                color = "'#00FF00'"; // ������� ��� �������� ��������������
            else if (i == coordinates.Length - 1)
                color = "'#FF0000'"; // ������� ��� ��������� �����
            else
                color = "'#1E90FF'"; // ����� ��� ������������� �����

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
                        
                        // ��������� ����� �� �����
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
                        
                        // ��������� �������� ����������
                        map.controls.add(new ymaps.control.RouteButton({
                            options: {
                                float: 'right',
                                floatIndex: 100
                            }
                        }));
                        
                        // ��������� ������ ����������� ��������������
                        var geolocationControl = new ymaps.control.GeolocationControl({
                            options: {
                                float: 'left',
                                floatIndex: 100,
                                noPlacemark: false
                            }
                        });
                        map.controls.add(geolocationControl);
                        
                        // ��������� ������ ��� ��������� ���� ��������
                        var routeTypeButton = new ymaps.control.Button({
                            data: {
                                content: '��� ��������',
                                title: '�������� ��� ��������'
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
                                        content: '������',
                                        value: 'pedestrian'
                                    }
                                },
                                {
                                    data: {
                                        content: '�� ������',
                                        value: 'auto'
                                    }
                                },
                                {
                                    data: {
                                        content: '������������ ���������',
                                        value: 'masstransit'
                                    }
                                }
                            ];
                            
                            var routeTypeMenu = new ymaps.control.ListBox({
                                data: {
                                    content: '��� ��������'
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
                                    
                                    // ������� ������ �������
                                    map.geoObjects.remove(multiRoute);
                                    
                                    // ������� ����� ������� � ��������� �����
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
                                    
                                    // ��������� ����� ������� �� �����
                                    map.geoObjects.add(multiRoute);
                                    
                                    // ������� ���� ����� ������
                                    map.controls.remove(routeTypeMenu);
                                }
                            });
                        });
                        
                        map.controls.add(routeTypeButton, {
                            float: 'right',
                            floatIndex: 100
                        });
                        
                        // ��������� ���������� ��� ������ ��������������
                        document.getElementById('location-button').addEventListener('click', function() {
                            console.log('Location button clicked');
                            if (navigator.geolocation) {
                                console.log('Geolocation API is available');
                                navigator.geolocation.getCurrentPosition(
                                    function(position) {
                                        console.log('Position received:', position.coords.latitude, position.coords.longitude);
                                        var userLocation = [position.coords.latitude, position.coords.longitude];
                                        var accuracy = position.coords.accuracy;
                                        
                                        // ������� ���������� ������� ��������������, ���� ��� ����
                                        if (window.userLocationPlacemark) {
                                            map.geoObjects.remove(window.userLocationPlacemark);
                                        }
                                        if (window.accuracyCircle) {
                                            map.geoObjects.remove(window.accuracyCircle);
                                        }
                                        
                                        // ������� ����� �������� ��������������
                                        window.userLocationPlacemark = new ymaps.Placemark(userLocation, {
                                            hintContent: '���� ��������������',
                                            balloonContent: '�� ���������� �����<br>��������: ' + Math.round(accuracy) + ' �'
                                        }, {
                                            preset: 'islands#geolocationIcon',
                                            iconColor: '#4285F4'
                                        });
                                        
                                        // ������� ����, ������������ �������� ����������� ��������������
                                        window.accuracyCircle = new ymaps.Circle([userLocation, accuracy], {
                                            hintContent: '��������: ' + Math.round(accuracy) + ' �'
                                        }, {
                                            draggable: false,
                                            fillColor: '#4285F4',
                                            fillOpacity: 0.2,
                                            strokeColor: '#4285F4',
                                            strokeOpacity: 0.6,
                                            strokeWidth: 1
                                        });
                                        
                                        // ��������� ������� �� �����
                                        map.geoObjects.add(window.userLocationPlacemark);
                                        map.geoObjects.add(window.accuracyCircle);
                                        
                                        // ���������� ����� �� �������������� ������������
                                        map.setCenter(userLocation, 16, {
                                            duration: 500
                                        });
                                    },
                                    function(error) {
                                        console.error('Geolocation error:', error.code, error.message);
                                        var errorMessage = '';
                                        switch(error.code) {
                                            case error.PERMISSION_DENIED:
                                                errorMessage = '������������ ������� � ������� � ����������';
                                                break;
                                            case error.POSITION_UNAVAILABLE:
                                                errorMessage = '���������� � �������������� ����������';
                                                break;
                                            case error.TIMEOUT:
                                                errorMessage = '������� ����� �������� ������� ��������������';
                                                break;
                                            case error.UNKNOWN_ERROR:
                                                errorMessage = '��������� ����������� ������';
                                                break;
                                        }
                                        showError('������ ����������: ' + errorMessage);
                                    },
                                    {
                                        enableHighAccuracy: true,
                                        timeout: 10000,
                                        maximumAge: 0
                                    }
                                );
                            } else {
                                showError('���������� �� �������������� ����� ���������');
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
                <title>�� ��������������</title>
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
                <button id=""location-button"" class=""location-button"" title=""�� ��������������"">
                    <svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                        <path d=""M12 8C9.79 8 8 9.79 8 12C8 14.21 9.79 16 12 16C14.21 16 16 14.21 16 12C16 9.79 14.21 8 12 8ZM20.94 11C20.48 6.83 17.17 3.52 13 3.06V1H11V3.06C6.83 3.52 3.52 6.83 3.06 11H1V13H3.06C3.52 17.17 6.83 20.48 11 20.94V23H13V20.94C17.17 20.48 20.48 17.17 20.94 13H23V11H20.94ZM12 19C8.13 19 5 15.87 5 12C5 8.13 8.13 5 12 5C15.87 5 19 8.13 19 12C19 15.87 15.87 19 12 19Z"" fill=""#4285F4""/>
                    </svg>
                </button>
                <script>
                    // ������� ��� ����������� ������
                    function showError(message) {
                        var errorDiv = document.getElementById('error-message');
                        errorDiv.textContent = message;
                        errorDiv.style.display = 'block';
                        console.error(message);
                    }

                    // ��������� ���������� ������ JavaScript
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
                            
                            // ���������� �������� ��������������
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
                            
                            // ������� ����� �������� ��������������
                            var userLocationPlacemark = new ymaps.Placemark(userLocation, {
                                hintContent: '���� ��������������',
                                balloonContent: '�� ���������� �����<br>��������: ' + Math.round(accuracy) + ' �'
                            }, {
                                preset: 'islands#geolocationIcon',
                                iconColor: '#4285F4'
                            });
                            
                            // ������� ����, ������������ �������� ����������� ��������������
                            var accuracyCircle = new ymaps.Circle([userLocation, accuracy], {
                                hintContent: '��������: ' + Math.round(accuracy) + ' �'
                            }, {
                                draggable: false,
                                fillColor: '#4285F4',
                                fillOpacity: 0.2,
                                strokeColor: '#4285F4',
                                strokeOpacity: 0.6,
                                strokeWidth: 1
                            });
                            
                            // ��������� ������� �� �����
                            map.geoObjects.add(userLocationPlacemark);
                            map.geoObjects.add(accuracyCircle);
                            
                            // ��������� ������ ����������� ��������������
                            var geolocationControl = new ymaps.control.GeolocationControl({
                                options: {
                                    float: 'left',
                                    floatIndex: 100,
                                    noPlacemark: true
                                }
                            });
                            map.controls.add(geolocationControl);
                            
                            // ��������� ���������� ��� ������ ��������������
                            document.getElementById('location-button').addEventListener('click', function() {
                                console.log('Location button clicked');
                                if (navigator.geolocation) {
                                    console.log('Geolocation API is available');
                                    navigator.geolocation.getCurrentPosition(
                                        function(position) {
                                            console.log('Position received:', position.coords.latitude, position.coords.longitude);
                                            var newUserLocation = [position.coords.latitude, position.coords.longitude];
                                            var newAccuracy = position.coords.accuracy;
                                            
                                            // ������� ���������� �������
                                            map.geoObjects.remove(userLocationPlacemark);
                                            map.geoObjects.remove(accuracyCircle);
                                            
                                            // ������� ����� �����
                                            userLocationPlacemark = new ymaps.Placemark(newUserLocation, {
                                                hintContent: '���� ��������������',
                                                balloonContent: '�� ���������� �����<br>��������: ' + Math.round(newAccuracy) + ' �'
                                            }, {
                                                preset: 'islands#geolocationIcon',
                                                iconColor: '#4285F4'
                                            });
                                            
                                            // ������� ����� ���� ��������
                                            accuracyCircle = new ymaps.Circle([newUserLocation, newAccuracy], {
                                                hintContent: '��������: ' + Math.round(newAccuracy) + ' �'
                                            }, {
                                                draggable: false,
                                                fillColor: '#4285F4',
                                                fillOpacity: 0.2,
                                                strokeColor: '#4285F4',
                                                strokeOpacity: 0.6,
                                                strokeWidth: 1
                                            });
                                            
                                            // ��������� ������� �� �����
                                            map.geoObjects.add(userLocationPlacemark);
                                            map.geoObjects.add(accuracyCircle);
                                            
                                            // ���������� ����� �� ����� ��������������
                                            map.setCenter(newUserLocation, 16, {
                                                duration: 500
                                            });
                                        },
                                        function(error) {
                                            console.error('Geolocation error:', error.code, error.message);
                                            var errorMessage = '';
                                            switch(error.code) {
                                                case error.PERMISSION_DENIED:
                                                    errorMessage = '������������ ������� � ������� � ����������';
                                                break;
                                                case error.POSITION_UNAVAILABLE:
                                                    errorMessage = '���������� � �������������� ����������';
                                                break;
                                                case error.TIMEOUT:
                                                    errorMessage = '������� ����� �������� ������� ��������������';
                                                    break;
                                                case error.UNKNOWN_ERROR:
                                                    errorMessage = '��������� ����������� ������';
                                                    break;
                                            }
                                            showError('������ ����������: ' + errorMessage);
                                        },
                                        {
                                            enableHighAccuracy: true,
                                            timeout: 10000,
                                            maximumAge: 0
                                        }
                                    );
                                } else {
                                    showError('���������� �� �������������� ����� ���������');
                                }
                            });
                            
                            // ��������� ������������ ��������������
                            startLocationTracking(map, userLocationPlacemark, accuracyCircle);
                            
                        } catch (e) {
                            showError('Error in init function: ' + e.message);
                        }
                    }
                    
                    // ������� ��� ������������ �������������� � �������� �������
                    function startLocationTracking(map, placemark, circle) {
                        if (navigator.geolocation) {
                            var watchId = navigator.geolocation.watchPosition(
                                function(position) {
                                    var newUserLocation = [position.coords.latitude, position.coords.longitude];
                                    var newAccuracy = position.coords.accuracy;
                                    
                                    // ��������� ��������� ����� ��������
                                    circle.geometry.setRadius(newAccuracy);
                                    circle.geometry.setCoordinates(newUserLocation);
                                    
                                    // ��������� ������� �����
                                    placemark.geometry.setCoordinates(newUserLocation);
                                    
                                    // ��������� ���������� ������
                                    placemark.properties.set('balloonContent', '�� ���������� �����<br>��������: ' + Math.round(newAccuracy) + ' �');
                                    
                                    // ���������� �����, ���� �������� ���������� �����������
                                    if (window.lastAccuracy && window.lastAccuracy > newAccuracy * 1.5) {
                                        map.setCenter(newUserLocation, 16, {
                                            duration: 500
                                        });
                                    }
                                    
                                    window.lastAccuracy = newAccuracy;
                                },
                                function(error) {
                                    console.error('������ ������������ ��������������:', error.message);
                                },
                                {
                                    enableHighAccuracy: true,
                                    timeout: 10000,
                                    maximumAge: 0
                                }
                            );
                            
                            // ��������� ID ��� ����������� ��������� ������������
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
                <title>������ �����</title>
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
                <button id=""location-button"" class=""location-button"" title=""�� ��������������"">
                    <svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                        <path d=""M12 8C9.79 8 8 9.79 8 12C8 14.21 9.79 16 12 16C14.21 16 16 14.21 16 12C16 9.79 14.21 8 12 8ZM20.94 11C20.48 6.83 17.17 3.52 13 3.06V1H11V3.06C6.83 3.52 3.52 6.83 3.06 11H1V13H3.06C3.52 17.17 6.83 20.48 11 20.94V23H13V20.94C17.17 20.48 20.48 17.17 20.94 13H23V11H20.94ZM12 19C8.13 19 5 15.87 5 12C5 8.13 8.13 5 12 5C15.87 5 19 8.13 19 12C19 15.87 15.87 19 12 19Z"" fill=""#4285F4""/>
                    </svg>
                </button>
                <script>
                    // ������� ��� ����������� ������
                    function showError(message) {
                        var errorDiv = document.getElementById('error-message');
                        errorDiv.textContent = message;
                        errorDiv.style.display = 'block';
                        console.error(message);
                    }

                    // ��������� ���������� ������ JavaScript
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
                            
                            // ��������� ������ ����������� ��������������
                            var geolocationControl = new ymaps.control.GeolocationControl({
                                options: {
                                    float: 'left',
                                    floatIndex: 100,
                                    noPlacemark: false
                                }
                            });
                            map.controls.add(geolocationControl);
                            
                            // ��������� ���������� ��� ������ ��������������
                            document.getElementById('location-button').addEventListener('click', function() {
                                console.log('Location button clicked');
                                if (navigator.geolocation) {
                                    console.log('Geolocation API is available');
                                    navigator.geolocation.getCurrentPosition(
                                        function(position) {
                                            console.log('Position received:', position.coords.latitude, position.coords.longitude);
                                            var userLocation = [position.coords.latitude, position.coords.longitude];
                                            var accuracy = position.coords.accuracy;
                                            
                                            // ������� ���������� ������� ��������������, ���� ��� ����
                                            if (window.userLocationPlacemark) {
                                                map.geoObjects.remove(window.userLocationPlacemark);
                                            }
                                            if (window.accuracyCircle) {
                                                map.geoObjects.remove(window.accuracyCircle);
                                            }
                                            
                                            // ������� ����� �������� ��������������
                                            window.userLocationPlacemark = new ymaps.Placemark(userLocation, {
                                                hintContent: '���� ��������������',
                                                balloonContent: '�� ���������� �����<br>��������: ' + Math.round(accuracy) + ' �'
                                            }, {
                                                preset: 'islands#geolocationIcon',
                                                iconColor: '#4285F4'
                                            });
                                            
                                            // ������� ����, ������������ �������� ����������� ��������������
                                            window.accuracyCircle = new ymaps.Circle([userLocation, accuracy], {
                                                hintContent: '��������: ' + Math.round(accuracy) + ' �'
                                            }, {
                                                draggable: false,
                                                fillColor: '#4285F4',
                                                fillOpacity: 0.2,
                                                strokeColor: '#4285F4',
                                                strokeOpacity: 0.6,
                                                strokeWidth: 1
                                            });
                                            
                                            // ��������� ������� �� �����
                                            map.geoObjects.add(window.userLocationPlacemark);
                                            map.geoObjects.add(window.accuracyCircle);
                                            
                                            // ���������� ����� �� �������������� ������������
                                            map.setCenter(userLocation, 16, {
                                                duration: 500
                                            });
                                            
                                            // ��������� ������������ ��������������
                                            startLocationTracking(map, window.userLocationPlacemark, window.accuracyCircle);
                                        },
                                        function(error) {
                                            console.error('Geolocation error:', error.code, error.message);
                                            var errorMessage = '';
                                            switch(error.code) {
                                                case error.PERMISSION_DENIED:
                                                    errorMessage = '������������ ������� � ������� � ����������';
                                                    break;
                                                case error.POSITION_UNAVAILABLE:
                                                    errorMessage = '���������� � �������������� ����������';
                                                    break;
                                                case error.TIMEOUT:
                                                    errorMessage = '������� ����� �������� ������� ��������������';
                                                    break;
                                                case error.UNKNOWN_ERROR:
                                                    errorMessage = '��������� ����������� ������';
                                                    break;
                                            }
                                            showError('������ ����������: ' + errorMessage);
                                        },
                                        {
                                            enableHighAccuracy: true,
                                            timeout: 10000,
                                            maximumAge: 0
                                        }
                                    );
                                } else {
                                    showError('���������� �� �������������� ����� ���������');
                                }
                            });
                            
                            // ������� ��� ������������ �������������� � �������� �������
                            function startLocationTracking(map, placemark, circle) {
                                if (navigator.geolocation) {
                                    var watchId = navigator.geolocation.watchPosition(
                                        function(position) {
                                            var newUserLocation = [position.coords.latitude, position.coords.longitude];
                                            var newAccuracy = position.coords.accuracy;
                                            
                                            // ��������� ��������� ����� ��������
                                            circle.geometry.setRadius(newAccuracy);
                                            circle.geometry.setCoordinates(newUserLocation);
                                            
                                            // ��������� ������� �����
                                            placemark.geometry.setCoordinates(newUserLocation);
                                            
                                            // ��������� ���������� ������
                                            placemark.properties.set('balloonContent', '�� ���������� �����<br>��������: ' + Math.round(newAccuracy) + ' �');
                                            
                                            // ���������� �����, ���� �������� ���������� �����������
                                            if (window.lastAccuracy && window.lastAccuracy > newAccuracy * 1.5) {
                                                map.setCenter(newUserLocation, 16, {
                                                    duration: 500
                                                });
                                            }
                                            
                                            window.lastAccuracy = newAccuracy;
                                        },
                                        function(error) {
                                            console.error('������ ������������ ��������������:', error.message);
                                        },
                                        {
                                            enableHighAccuracy: true,
                                            timeout: 10000,
                                            maximumAge: 0
                                        }
                                    );
                                    
                                    // ��������� ID ��� ����������� ��������� ������������
                                    window.locationWatchId = watchId;
                                }
                            }
                            
                            // ������� ����� ���������� ��������������
                            geolocationControl.events.add('click', function() {
                                document.getElementById('location-button').click();
                            });
                            
                            // ������������� ��������� ����������� ��������������
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

    // ��������� ����� LoadMapWithRoute, ����� �������� ��������� �������� ����� �������� �����
    private void LoadMapWithRoute(string coords, string names)
    {
        try
        {
            Debug.WriteLine("�������� ����� � ���������...");

            var coordinates = coords.Split('|');
            var placeNames = names.Split('|');

            Debug.WriteLine($"���������� ���������: {coordinates.Length}");
            Debug.WriteLine($"���������� ��������: {placeNames.Length}");

            if (coordinates.Length < 2)
            {
                Debug.WriteLine("������������ ����� ��� ���������� ��������");
                DisplayAlert("������", "������������ ����� ��� ���������� ��������", "OK");
                LoadMapWithCurrentLocationAsync().ConfigureAwait(false);
                return;
            }

            var html = GenerateRouteHtml(coordinates, placeNames);

            // ��������� HTML � ���� ��� �������
            SaveHtmlToFile(html);

            // ��������� ���������� ��� ��������� ��������� �� JavaScript
            MapWebView.Navigating += WebView_Navigating;

            MapWebView.Source = new HtmlWebViewSource { Html = html };

            // ��������� ���������� ������ JavaScript
            MapWebView.Navigated += (sender, e) => {
                Debug.WriteLine($"WebView ��������: {e.Url}");
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            };

            MapWebView.Navigating += (sender, e) => {
                Debug.WriteLine($"WebView �����������: {e.Url}");
            };

            // ��������� ���������� � ��������
            UpdateRouteInfo(coordinates.Length);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"������ �������� �����: {ex.Message}");
            Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            DisplayAlert("������", $"������ �������� �����: {ex.Message}", "OK");
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            LoadMapWithCurrentLocationAsync().ConfigureAwait(false);
        }
    }

    // ���������� ��� ��������� ��������� �� JavaScript
    private void WebView_Navigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("js://"))
        {
            e.Cancel = true; // �������� ��������� �� ����� URL

            // ������ URL ��� ��������� ���������� � ��������
            var uri = new Uri(e.Url);
            var query = uri.Query.TrimStart('?');
            var parameters = ParseQueryParameters(query);

            if (parameters.TryGetValue("length", out var length) &&
                parameters.TryGetValue("duration", out var duration))
            {
                // ��������� ���������� � ��������
                _routeDistance = length;
                _routeDuration = duration;

                // ��������� ���������
                UpdateRouteInfoWithDetails(length, duration);
            }
        }
    }

    // ��������� ����� UpdateRouteInfo, ����� ���������� ����� ��������� ����������
    private void UpdateRouteInfo(int pointsCount)
    {
        // ��������� ����� ����� � ����������� � ��������
        if (pointsCount > 0)
        {
            string startPoint = pointsCount > 0 ? "������ ��������������" : "����������� �����";
            RouteInfoLabel.Text = $"������� �� {startPoint} ����� {pointsCount - 1} �����";
        }
        else
        {
            RouteInfoLabel.Text = "������� �� ��������";
        }
        RouteInfoLabel.TextColor = Colors.Black;
    }

    // ����� ����� ��� ���������� ���������� � �������� � ��������
    private void UpdateRouteInfoWithDetails(string distance, string duration)
    {
        // ��������� ���������� � �������� � ����������
        Device.BeginInvokeOnMainThread(() => {
            RouteDistanceLabel.Text = $"����������: {distance}";
            RouteDurationLabel.Text = $"����� � ����: {duration}";

            // ������ ����� ��������
            RouteDistanceLabel.IsVisible = true;
            RouteDurationLabel.IsVisible = true;
        });
    }

    // ��������� ����� SaveHtmlToFile, ������� ��� ������
    private void SaveHtmlToFile(string html)
    {
        try
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(documentsPath, "map_debug.html");
            File.WriteAllText(filePath, html);
            Debug.WriteLine($"HTML �������� � ����: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"������ ��� ���������� HTML: {ex.Message}");
        }
    }

    // ��������� ����� LoadMapWithCurrentLocationAsync, ������� ��� ������
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
            Debug.WriteLine($"������ ��� �������� ����� � ������� ���������������: {ex.Message}");
            LoadDefaultMap();
        }
    }
}
