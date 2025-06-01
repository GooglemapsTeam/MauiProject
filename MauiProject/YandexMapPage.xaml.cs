using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Emotional_Map;

public partial class YandexMapPage : ContentPage
{
    private string _apiKey = "69f697cc-32fb-4058-8056-a615983e7e93";
    private bool _isLocationPermissionGranted = false;
    private Random _random = new Random();
    private string _selectedDistrict = null;

    private class PlaceInfo
    {
        public string Name { get; set; }
        public string Coordinates { get; set; }
        public string District { get; set; }
    }

    private readonly Dictionary<string, string> _districts = new Dictionary<string, string>
    {
        { "Верх-Исетский", "Верх-Исетский район" },
        { "Железнодорожный", "Железнодорожный район" },
        { "Кировский", "Кировский район" },
        { "Ленинский", "Ленинский район" },
        { "Октябрьский", "Октябрьский район" },
        { "Орджоникидзевский", "Орджоникидзевский район" },
        { "Чкаловский", "Чкаловский район" }
    };

    private readonly List<PlaceInfo> _allPlacesInfo = new List<PlaceInfo>
    {
        // Верх-Исетский район
        new PlaceInfo { Name = "Верх-Исетский пруд", Coordinates = "56.836106, 60.545473", District = "Верх-Исетский" },
        new PlaceInfo { Name = "Парк «Зеленая роща»", Coordinates = "56.826660, 60.605514", District = "Верх-Исетский" },
        new PlaceInfo { Name = "ВИЗ-центр", Coordinates = "56.824687, 60.555051", District = "Верх-Исетский" },
        new PlaceInfo { Name = "Смотровая площадка у ВИЗа", Coordinates = "56.836815, 60.550605", District = "Верх-Исетский" },
        new PlaceInfo { Name = "Набережная Верх-Исетского пруда", Coordinates = "56.835904, 60.547989", District = "Верх-Исетский" },
        new PlaceInfo { Name = "Парк Победы", Coordinates = "56.820863, 60.550324", District = "Верх-Исетский" },
        new PlaceInfo { Name = "Сквер у ДК ВИЗ", Coordinates = "56.824687, 60.555051", District = "Верх-Исетский" },
        new PlaceInfo { Name = "Пляж на Верх-Исетском пруду", Coordinates = "56.830904, 60.542989", District = "Верх-Исетский" },
        
        // Железнодорожный район
        new PlaceInfo { Name = "Железнодорожный вокзал", Coordinates = "56.857527, 60.604722", District = "Железнодорожный" },
        new PlaceInfo { Name = "Набережная Исети у ЖД вокзала", Coordinates = "56.855527, 60.604722", District = "Железнодорожный" },
        new PlaceInfo { Name = "Сквер у ТЮЗа", Coordinates = "56.848389, 60.611111", District = "Железнодорожный" },
        new PlaceInfo { Name = "Привокзальная площадь", Coordinates = "56.856660, 60.605514", District = "Железнодорожный" },
        new PlaceInfo { Name = "Парк им. Павлика Морозова", Coordinates = "56.842106, 60.615473", District = "Железнодорожный" },
        new PlaceInfo { Name = "ДК Железнодорожников", Coordinates = "56.852904, 60.602989", District = "Железнодорожный" },
        new PlaceInfo { Name = "Сквер Строителей", Coordinates = "56.852687, 60.595051", District = "Железнодорожный" },
        new PlaceInfo { Name = "Площадь Первой Пятилетки", Coordinates = "56.858863, 60.610324", District = "Железнодорожный" },
        
        // Кировский район
        new PlaceInfo { Name = "ЦПКиО им. Маяковского", Coordinates = "56.878106, 60.585473", District = "Кировский" },
        new PlaceInfo { Name = "Театр драмы", Coordinates = "56.866687, 60.575051", District = "Кировский" },
        new PlaceInfo { Name = "Сквер Кировский", Coordinates = "56.870389, 60.591111", District = "Кировский" },
        new PlaceInfo { Name = "Набережная Исети около ЦПКиО", Coordinates = "56.876431, 60.587524", District = "Кировский" },
        new PlaceInfo { Name = "Парк Липовый", Coordinates = "56.878815, 60.580605", District = "Кировский" },
        new PlaceInfo { Name = "Площадь Кирова", Coordinates = "56.866904, 60.582989", District = "Кировский" },
        new PlaceInfo { Name = "Дом Маклецкого", Coordinates = "56.872863, 60.590324", District = "Кировский" },
        new PlaceInfo { Name = "Усадьба Тарасова", Coordinates = "56.869527, 60.594722", District = "Кировский" },
        
        // Ленинский район
        new PlaceInfo { Name = "Плотинка", Coordinates = "56.837527, 60.614722", District = "Ленинский" },
        new PlaceInfo { Name = "Оперный театр", Coordinates = "56.838660, 60.615514", District = "Ленинский" },
        new PlaceInfo { Name = "Литературный квартал", Coordinates = "56.836389, 60.611111", District = "Ленинский" },
        new PlaceInfo { Name = "Дом Севастьянова", Coordinates = "56.834687, 60.615051", District = "Ленинский" },
        new PlaceInfo { Name = "Исторический сквер", Coordinates = "56.837527, 60.614722", District = "Ленинский" },
        new PlaceInfo { Name = "Площадь Труда", Coordinates = "56.837527, 60.614722", District = "Ленинский" },
        new PlaceInfo { Name = "Театральный сквер", Coordinates = "56.838660, 60.615514", District = "Ленинский" },
        new PlaceInfo { Name = "Набережная Исети от плотинки", Coordinates = "56.837527, 60.614722", District = "Ленинский" },
        
        // Октябрьский район
        new PlaceInfo { Name = "Харитоновский парк", Coordinates = "56.841527, 60.634722", District = "Октябрьский" },
        new PlaceInfo { Name = "Филармония", Coordinates = "56.832389, 60.651111", District = "Октябрьский" },
        new PlaceInfo { Name = "Площадь 1905 года", Coordinates = "56.838904, 60.612989", District = "Октябрьский" },
        new PlaceInfo { Name = "Набережная Рабочей Молодёжи", Coordinates = "56.842660, 60.635514", District = "Октябрьский" },
        new PlaceInfo { Name = "Парк Энгельса", Coordinates = "56.816106, 60.635473", District = "Октябрьский" },
        new PlaceInfo { Name = "Сквер Октябрьский", Coordinates = "56.828687, 60.635051", District = "Октябрьский" },
        new PlaceInfo { Name = "Парк семейных традиций", Coordinates = "56.830815, 60.640605", District = "Октябрьский" },
        new PlaceInfo { Name = "Двор с Космонавтами", Coordinates = "56.824863, 60.650324", District = "Октябрьский" },
        
        // Орджоникидзевский район
        new PlaceInfo { Name = "Парк Уралмаш", Coordinates = "56.890106, 60.615473", District = "Орджоникидзевский" },
        new PlaceInfo { Name = "ДК Уралмаш", Coordinates = "56.890687, 60.615051", District = "Орджоникидзевский" },
        new PlaceInfo { Name = "Сквер Энергетиков", Coordinates = "56.893527, 60.614722", District = "Орджоникидзевский" },
        new PlaceInfo { Name = "Набережная Исети у УЗТМ", Coordinates = "56.894389, 60.611111", District = "Орджоникидзевский" },
        new PlaceInfo { Name = "Парк Калиновский", Coordinates = "56.900431, 60.617524", District = "Орджоникидзевский" },
        new PlaceInfo { Name = "Сквер Строителей", Coordinates = "56.892815, 60.610605", District = "Орджоникидзевский" },
        new PlaceInfo { Name = "Площадь Первой Пятилетки", Coordinates = "56.896863, 60.610324", District = "Орджоникидзевский" },
        new PlaceInfo { Name = "Проспект Космонавтов", Coordinates = "56.894660, 60.615514", District = "Орджоникидзевский" },
        
        // Чкаловский район
        new PlaceInfo { Name = "Лесопарк", Coordinates = "56.792106, 60.635473", District = "Чкаловский" },
        new PlaceInfo { Name = "Набережная Исети (Ботаника)", Coordinates = "56.795527, 60.634722", District = "Чкаловский" },
        new PlaceInfo { Name = "Сквер Чкаловский", Coordinates = "56.796389, 60.631111", District = "Чкаловский" },
        new PlaceInfo { Name = "Парк Зелёный остров", Coordinates = "56.792431, 60.627524", District = "Чкаловский" },
        new PlaceInfo { Name = "ТЦ Ботаника Молл", Coordinates = "56.794815, 60.630605", District = "Чкаловский" },
        new PlaceInfo { Name = "Пляж Широкая речка", Coordinates = "56.798863, 60.630324", District = "Чкаловский" },
        new PlaceInfo { Name = "ДК Елизаветинский", Coordinates = "56.792904, 60.622989", District = "Чкаловский" },
        new PlaceInfo { Name = "Улица 8 Марта", Coordinates = "56.792687, 60.625051", District = "Чкаловский" }
    };

    private string[] _predefinedPoints;
    private string[] _predefinedNames;
    private string _routeDistance = "";
    private string _routeDuration = "";

    public YandexMapPage()
    {
        InitializeComponent();

        MapWebView.Navigated += (sender, e) => {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        };

        // МНОЖЕСТВЕННЫЕ ОБРАБОТЧИКИ для кнопки назад
        SetupBackButtonHandlers();

        BuildRouteButton.Clicked += (sender, e) => {
            BuildPredefinedRoute();
        };

        RandomRouteButton.Clicked += (sender, e) => {
            GenerateRandomRoute();
            BuildPredefinedRoute();
        };

        SelectDistrictButton.Clicked += async (sender, e) => {
            await ShowDistrictSelectionDialog();
        };

#if DEBUG
        DebugButton.IsVisible = true;
        DebugButton.Clicked += async (sender, e) => {
            await ShowDebugInfo();
        };
#endif

        UpdateButtonsState();
    }

    // УПРОЩЕННЫЙ метод для настройки обработчиков кнопки назад
    private void SetupBackButtonHandlers()
    {
        try
        {
            // Используем только обычную кнопку
            BackButton.Clicked += async (sender, e) => {
                await HandleBackNavigation("Button.Clicked");
            };

            Debug.WriteLine("Обработчик кнопки назад настроен");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при настройке обработчика кнопки назад: {ex.Message}");
        }
    }

    // НОВЫЙ универсальный метод обработки навигации назад
    private async Task HandleBackNavigation(string source)
    {
        try
        {
            Debug.WriteLine($"Навигация назад вызвана из: {source}");

            // Показываем диалог подтверждения
            bool shouldGoBack = await DisplayAlert("Подтверждение", "Вы хотите вернуться в главное меню?", "Да", "Нет");

            if (!shouldGoBack)
            {
                Debug.WriteLine("Пользователь отменил навигацию назад");
                return;
            }

            // Получаем отладочную информацию о текущем состоянии навигации
            var currentRoute = Shell.Current?.CurrentState?.Location?.ToString() ?? "Неизвестно";
            Debug.WriteLine($"Текущий маршрут: {currentRoute}");

            bool navigationSuccessful = false;

            // Способ 1: Навигация к MainPage через правильный маршрут из AppShell
            try
            {
                Debug.WriteLine("Попытка навигации к //MainPage");
                await Shell.Current.GoToAsync("//MainPage");
                navigationSuccessful = true;
                Debug.WriteLine("Навигация к //MainPage успешна");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Навигация к //MainPage не удалась: {ex.Message}");
            }

            // Способ 2: Попробуем навигацию через маршрут MainPage
            try
            {
                Debug.WriteLine("Попытка навигации к MainPage");
                await Shell.Current.GoToAsync("MainPage");
                navigationSuccessful = true;
                Debug.WriteLine("Навигация к MainPage успешна");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Навигация к MainPage не удалась: {ex.Message}");
            }

            // Способ 3: Попробуем относительную навигацию назад
            try
            {
                Debug.WriteLine("Попытка относительной навигации ..");
                await Shell.Current.GoToAsync("..");
                navigationSuccessful = true;
                Debug.WriteLine("Относительная навигация успешна");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Относительная навигация не удалась: {ex.Message}");
            }

            // Способ 4: Navigation.PopAsync (несколько раз если нужно)
            try
            {
                if (Navigation != null && Navigation.NavigationStack.Count > 1)
                {
                    Debug.WriteLine($"Попытка Navigation.PopAsync(), стек содержит {Navigation.NavigationStack.Count} страниц");

                    // Попробуем вернуться на несколько страниц назад до MainPage
                    while (Navigation.NavigationStack.Count > 1)
                    {
                        var currentPage = Navigation.NavigationStack.LastOrDefault();
                        Debug.WriteLine($"Текущая страница в стеке: {currentPage?.GetType().Name}");

                        await Navigation.PopAsync();

                        // Проверяем, достигли ли мы MainPage
                        var newCurrentPage = Navigation.NavigationStack.LastOrDefault();
                        if (newCurrentPage?.GetType().Name == "MainPage")
                        {
                            Debug.WriteLine("Достигли MainPage через PopAsync");
                            navigationSuccessful = true;
                            return;
                        }
                    }

                    if (!navigationSuccessful)
                    {
                        Debug.WriteLine("PopAsync навигация завершена, но MainPage не найдена");
                        navigationSuccessful = true; // Считаем успешной, так как мы вернулись назад
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PopAsync навигация не удалась: {ex.Message}");
            }

            // Способ 5: Попробуем установить MainPage как корневую страницу
            try
            {
                Debug.WriteLine("Попытка создания новой MainPage");
                var mainPage = new MainPage();
                Application.Current.MainPage = new AppShell();
                await Shell.Current.GoToAsync("//MainPage");
                navigationSuccessful = true;
                Debug.WriteLine("Создание новой MainPage успешно");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Создание новой MainPage не удалось: {ex.Message}");
            }

            // Если ничего не сработало, показываем подробную информацию
            if (!navigationSuccessful)
            {
                Debug.WriteLine("Все способы навигации не удались");

                string debugInfo = $"Отладочная информация навигации:\n" +
                                  $"Текущий маршрут: {currentRoute}\n" +
                                  $"Navigation Stack: {Navigation?.NavigationStack?.Count ?? 0} страниц\n" +
                                  $"Modal Stack: {Navigation?.ModalStack?.Count ?? 0} страниц\n" +
                                  $"Shell.Current: {(Shell.Current != null ? "Доступен" : "Недоступен")}\n" +
                                  $"Application.Current.MainPage: {(Application.Current?.MainPage != null ? Application.Current.MainPage.GetType().Name : "Недоступен")}";

                // Предлагаем пользователю варианты
                var action = await DisplayActionSheet("Не удалось вернуться в главное меню", "Отмена", null,
                    "Перезапустить приложение", "Использовать системную кнопку назад", "Показать отладочную информацию");

                switch (action)
                {
                    case "Перезапустить приложение":
                        // Попробуем перезапустить приложение
                        try
                        {
                            Application.Current.MainPage = new AppShell();
                            await Shell.Current.GoToAsync("//MainPage");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Перезапуск приложения не удался: {ex.Message}");
                            await DisplayAlert("Ошибка", "Не удалось перезапустить приложение. Пожалуйста, закройте и откройте приложение вручную.", "OK");
                        }
                        break;
                    case "Показать отладочную информацию":
                        await DisplayAlert("Отладочная информация", debugInfo, "OK");
                        break;
                    default:
                        await DisplayAlert("Информация",
                            "Используйте системную кнопку назад или перезапустите приложение.",
                            "OK");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Критическая ошибка в HandleBackNavigation: {ex.Message}");
            await DisplayAlert("Ошибка", $"Ошибка навигации: {ex.Message}", "OK");
        }
    }

    // Переопределяем системную кнопку назад
    protected override bool OnBackButtonPressed()
    {
        try
        {
            Debug.WriteLine("Системная кнопка назад нажата");

            // Запускаем обработку навигации асинхронно
            Device.BeginInvokeOnMainThread(async () => {
                await HandleBackNavigation("System Back Button");
            });

            // Возвращаем true, чтобы предотвратить стандартное поведение
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при обработке системной кнопки назад: {ex.Message}");
            return base.OnBackButtonPressed();
        }
    }

    private async Task ShowNavigationStructure()
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== СТРУКТУРА НАВИГАЦИИ ===");

            // Информация о Shell
            if (Shell.Current != null)
            {
                sb.AppendLine($"Shell.Current: {Shell.Current.GetType().Name}");
                sb.AppendLine($"Current Route: {Shell.Current.CurrentState?.Location}");
                sb.AppendLine($"Current Page: {Shell.Current.CurrentPage?.GetType().Name}");
            }
            else
            {
                sb.AppendLine("Shell.Current: НЕ ДОСТУПЕН");
            }

            // Информация о Navigation Stack
            if (Navigation?.NavigationStack != null)
            {
                sb.AppendLine($"\nNavigation Stack ({Navigation.NavigationStack.Count} страниц):");
                for (int i = 0; i < Navigation.NavigationStack.Count; i++)
                {
                    var page = Navigation.NavigationStack[i];
                    sb.AppendLine($"  [{i}] {page.GetType().Name} - {page.Title}");
                }
            }

            // Информация о Modal Stack
            if (Navigation?.ModalStack != null && Navigation.ModalStack.Count > 0)
            {
                sb.AppendLine($"\nModal Stack ({Navigation.ModalStack.Count} страниц):");
                for (int i = 0; i < Navigation.ModalStack.Count; i++)
                {
                    var page = Navigation.ModalStack[i];
                    sb.AppendLine($"  [{i}] {page.GetType().Name} - {page.Title}");
                }
            }

            // Информация о Application.Current.MainPage
            if (Application.Current?.MainPage != null)
            {
                sb.AppendLine($"\nApplication.Current.MainPage: {Application.Current.MainPage.GetType().Name}");
            }

            await DisplayAlert("Структура навигации", sb.ToString(), "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка получения структуры навигации: {ex.Message}", "OK");
        }
    }

    private async Task ShowDebugInfo()
    {
        var action = await DisplayActionSheet("Отладочная информация", "Отмена", null,
            "Показать общую информацию", "Показать структуру навигации", "Тест навигации");

        switch (action)
        {
            case "Показать общую информацию":
                await ShowGeneralDebugInfo();
                break;
            case "Показать структуру навигации":
                await ShowNavigationStructure();
                break;
            case "Тест навигации":
                await TestNavigation();
                break;
        }
    }

    private async Task ShowGeneralDebugInfo()
    {
        var shellInfo = Shell.Current != null ? "Доступен" : "Недоступен";
        var navigationStackCount = Navigation?.NavigationStack?.Count ?? 0;
        var modalStackCount = Navigation?.ModalStack?.Count ?? 0;
        var currentPage = Shell.Current?.CurrentPage?.GetType().Name ?? "Неизвестно";
        var currentRoute = Shell.Current?.CurrentState?.Location?.ToString() ?? "Неизвестно";

        await DisplayAlert("Общая отладочная информация",
            $"Разрешение на геолокацию: {_isLocationPermissionGranted}\n" +
            $"Выбранный район: {_selectedDistrict ?? "Не выбран"}\n" +
            $"Shell.Current: {shellInfo}\n" +
            $"Navigation Stack Count: {navigationStackCount}\n" +
            $"Modal Stack Count: {modalStackCount}\n" +
            $"Current Page: {currentPage}\n" +
            $"Current Route: {currentRoute}",
            "OK");
    }

    private async Task TestNavigation()
    {
        var testRoutes = new[] { "..", "//MainPage", "//main", "/MainPage" };

        foreach (var route in testRoutes)
        {
            try
            {
                var result = await DisplayAlert("Тест навигации",
                    $"Попробовать навигацию к: {route}?", "Да", "Пропустить");

                if (result)
                {
                    await Shell.Current.GoToAsync(route);
                    await DisplayAlert("Успех", $"Навигация к {route} успешна!", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Навигация к {route} не удалась: {ex.Message}", "OK");
            }
        }
    }

    private async Task ShowDistrictSelectionDialog()
    {
        var districts = _districts.Values.ToArray();
        var selectedDistrict = await DisplayActionSheet("Выберите район", "Отмена", null, districts);

        if (selectedDistrict != null && selectedDistrict != "Отмена")
        {
            _selectedDistrict = _districts.FirstOrDefault(x => x.Value == selectedDistrict).Key;
            Debug.WriteLine($"Выбран район: {selectedDistrict}, ключ: {_selectedDistrict}");

            var placesInDistrict = _allPlacesInfo.Where(p => p.District == _selectedDistrict).ToList();
            Debug.WriteLine($"В районе {_selectedDistrict} найдено {placesInDistrict.Count} мест");

            if (placesInDistrict.Count < 4)
            {
                await DisplayAlert("Внимание", $"В районе {selectedDistrict} недостаточно мест для построения маршрута. Выберите другой район.", "OK");
                return;
            }

            if (_selectedDistrict != null)
            {
                _predefinedPoints = null;
                _predefinedNames = null;

                Device.BeginInvokeOnMainThread(() => {
                    RouteInfoLabel.Text = $"Выбран район: {selectedDistrict}";
                    RouteInfoLabel.TextColor = Colors.Black;
                    RouteDistanceLabel.IsVisible = false;
                    RouteDurationLabel.IsVisible = false;
                });

                UpdateButtonsState();
                GenerateRandomRoute();
            }
        }
    }

    private void UpdateButtonsState()
    {
        BuildRouteButton.IsVisible = _selectedDistrict != null;

        if (_selectedDistrict != null)
        {
            RandomRouteButton.Text = $"Случайный маршрут по району";
        }
        else
        {
            RandomRouteButton.Text = "Случайный маршрут";
        }
    }

    private void GenerateRandomRoute()
    {
        if (_selectedDistrict == null)
        {
            Device.BeginInvokeOnMainThread(async () => {
                await DisplayAlert("Внимание", "Пожалуйста, сначала выберите район", "OK");
            });
            return;
        }

        Debug.WriteLine($"Генерация маршрута для района: {_selectedDistrict}");

        var availablePlaces = _allPlacesInfo.Where(p => p.District == _selectedDistrict).ToList();
        Debug.WriteLine($"Найдено {availablePlaces.Count} мест в районе {_selectedDistrict}");

        if (availablePlaces.Count < 4)
        {
            Device.BeginInvokeOnMainThread(async () => {
                await DisplayAlert("Внимание", "В выбранном районе недостаточно мест для построения маршрута. Выберите другой район.", "OK");
            });
            return;
        }

        var selectedPlaces = GetRandomPlaces(availablePlaces, 4);
        _predefinedPoints = selectedPlaces.Select(p => p.Coordinates).ToArray();
        _predefinedNames = selectedPlaces.Select(p => p.Name).ToArray();

        Debug.WriteLine($"Сгенерирован новый случайный маршрут в районе {_selectedDistrict}: {string.Join(" → ", _predefinedNames)}");

        Device.BeginInvokeOnMainThread(() => {
            RouteInfoLabel.Text = $"Маршрут по району: {_districts[_selectedDistrict]}\n{string.Join(" → ", _predefinedNames)}";
            RouteInfoLabel.TextColor = Colors.Black;
        });
    }

    private List<PlaceInfo> GetRandomPlaces(List<PlaceInfo> places, int count)
    {
        var result = new List<PlaceInfo>();
        var availablePlaces = new List<PlaceInfo>(places);

        if (availablePlaces.Count < count)
        {
            count = availablePlaces.Count;
        }

        for (int i = 0; i < count && availablePlaces.Count > 0; i++)
        {
            int index = _random.Next(0, availablePlaces.Count);
            var selectedPlace = availablePlaces[index];
            result.Add(selectedPlace);
            availablePlaces.RemoveAt(index);
        }

        return result;
    }

    private async void BuildPredefinedRoute()
    {
        try
        {
            if (_selectedDistrict == null)
            {
                await DisplayAlert("Ошибка", "Пожалуйста, сначала выберите район", "OK");
                return;
            }

            if (_predefinedPoints == null || _predefinedPoints.Length == 0)
            {
                await DisplayAlert("Ошибка", "Маршрут не сгенерирован. Пожалуйста, нажмите 'Случайный маршрут'.", "OK");
                return;
            }

            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var currentLocation = await GetCurrentLocationAsync();
            List<string> coordinates = new List<string>();
            List<string> names = new List<string>();

            if (currentLocation == null)
            {
                await DisplayAlert("Внимание", "Не удалось определить ваше местоположение. Маршрут будет построен без учета вашего местоположения.", "OK");
                coordinates.AddRange(_predefinedPoints);
                names.AddRange(_predefinedNames);
            }
            else
            {
                coordinates.Add($"{currentLocation.Latitude.ToString(CultureInfo.InvariantCulture)}, {currentLocation.Longitude.ToString(CultureInfo.InvariantCulture)}");
                names.Add("Ваше местоположение");
                coordinates.AddRange(_predefinedPoints);
                names.AddRange(_predefinedNames);
            }

            var routeCoordsString = string.Join("|", coordinates);
            var routeNamesString = string.Join("|", names);

            LoadMapWithRoute(routeCoordsString, routeNamesString);
            UpdateRouteDestinations(_predefinedNames);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при построении предопределенного маршрута: {ex.Message}");
            await DisplayAlert("Ошибка", "Не удалось построить маршрут", "OK");
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private void UpdateRouteDestinations(string[] destinations)
    {
        Device.BeginInvokeOnMainThread(() => {
            RouteInfoLabel.Text = $"Маршрут: {string.Join(" → ", destinations)}";
            RouteInfoLabel.TextColor = Colors.Black;
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _isLocationPermissionGranted = await RequestLocationPermissionAsync();

        try
        {
            var route = Shell.Current.CurrentState.Location.ToString();
            Debug.WriteLine($"Текущий маршрут: {route}");

            var queryStart = route.IndexOf('?');
            if (queryStart > 0)
            {
                var query = route.Substring(queryStart + 1);
                var parameters = ParseQueryParameters(query);

                if (parameters.TryGetValue("coords", out var coords) &&
                    parameters.TryGetValue("names", out var names))
                {
                    LoadMapWithRoute(coords, names);
                    return;
                }
            }

            await LoadMapWithCurrentLocationAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка в OnAppearing: {ex.Message}");
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
            </style>
        </head>
        <body>
            <div id=""map""></div>
            <script>
                ymaps.ready(function() {
                    var coordinates = [");

        for (int i = 0; i < coordinates.Length; i++)
        {
            var coords = coordinates[i].Split(',');
            if (coords.Length == 2 &&
                double.TryParse(coords[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(coords[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
            {
                sb.Append($"[{lat.ToString(CultureInfo.InvariantCulture)}, {lng.ToString(CultureInfo.InvariantCulture)}]");
                if (i < coordinates.Length - 1) sb.Append(", ");
            }
        }

        sb.Append(@"];
                    var placeNames = [");

        for (int i = 0; i < placeNames.Length; i++)
        {
            sb.Append($"'{placeNames[i].Replace("'", "\\'")}' ");
            if (i < placeNames.Length - 1) sb.Append(", ");
        }

        sb.Append(@"];
                    
                    var map = new ymaps.Map('map', {
                        center: coordinates[0],
                        zoom: 12,
                        controls: ['zoomControl', 'typeSelector', 'fullscreenControl']
                    });
                    
                    for (var i = 0; i < coordinates.length; i++) {
                        var placemark = new ymaps.Placemark(coordinates[i], {
                            hintContent: placeNames[i],
                            balloonContent: placeNames[i]
                        }, {
                            preset: i === 0 ? 'islands#redIcon' : 'islands#blueIcon'
                        });
                        map.geoObjects.add(placemark);
                    }
                    
                    if (coordinates.length > 1) {
                        var multiRoute = new ymaps.multiRouter.MultiRoute({
                            referencePoints: coordinates,
                            params: {
                                routingMode: 'pedestrian'
                            }
                        }, {
                            boundsAutoApply: true,
                            routeActiveStrokeWidth: 6,
                            routeActiveStrokeColor: '#fa6600'
                        });
                        
                        map.geoObjects.add(multiRoute);
                    }
                });
            </script>
        </body>
        </html>");

        return sb.ToString();
    }

    private void LoadDefaultMap()
    {
        try
        {
            var html = GenerateDefaultMapHtml();
            MapWebView.Source = new HtmlWebViewSource { Html = html };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при загрузке карты по умолчанию: {ex.Message}");
        }
    }

    private string GenerateDefaultMapHtml()
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset=""utf-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
            <title>Карта</title>
            <script src=""https://api-maps.yandex.ru/2.1/?apikey={_apiKey}&lang=ru_RU"" type=""text/javascript""></script>
            <style>
                html, body, #map {{
                    width: 100%; 
                    height: 100%; 
                    padding: 0; 
                    margin: 0;
                }}
            </style>
        </head>
        <body>
            <div id=""map""></div>
            <script>
                ymaps.ready(function() {{
                    var map = new ymaps.Map('map', {{
                        center: [56.8431, 60.6454],
                        zoom: 10,
                        controls: ['zoomControl', 'typeSelector', 'fullscreenControl']
                    }});
                }});
            </script>
        </body>
        </html>";
    }

    private string GenerateCurrentLocationHtml(double latitude, double longitude, double accuracy)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset=""utf-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
            <title>Ваше местоположение</title>
            <script src=""https://api-maps.yandex.ru/2.1/?apikey={_apiKey}&lang=ru_RU"" type=""text/javascript""></script>
            <style>
                html, body, #map {{
                    width: 100%; 
                    height: 100%; 
                    padding: 0; 
                    margin: 0;
                }}
            </style>
        </head>
        <body>
            <div id=""map""></div>
            <script>
                ymaps.ready(function() {{
                    var map = new ymaps.Map('map', {{
                        center: [{latitude.ToString(CultureInfo.InvariantCulture)}, {longitude.ToString(CultureInfo.InvariantCulture)}],
                        zoom: 16,
                        controls: ['zoomControl', 'typeSelector', 'fullscreenControl']
                    }});
                    
                    var placemark = new ymaps.Placemark([{latitude.ToString(CultureInfo.InvariantCulture)}, {longitude.ToString(CultureInfo.InvariantCulture)}], {{
                        hintContent: 'Ваше местоположение',
                        balloonContent: 'Вы находитесь здесь<br>Точность: {Math.Round(accuracy)} м'
                    }}, {{
                        preset: 'islands#geolocationIcon',
                        iconColor: '#4285F4'
                    }});
                    
                    map.geoObjects.add(placemark);
                    
                    var circle = new ymaps.Circle([[{latitude.ToString(CultureInfo.InvariantCulture)}, {longitude.ToString(CultureInfo.InvariantCulture)}], {{{accuracy.ToString(CultureInfo.InvariantCulture)}}}], {{}}, {{
                        fillColor: '#4285F4',
                        fillOpacity: 0.2,
                        strokeColor: '#4285F4',
                        strokeOpacity: 0.6,
                        strokeWidth: 1
                    }});
                    
                    map.geoObjects.add(circle);
                }});
            </script>
        </body>
        </html>";
    }

    private void LoadMapWithRoute(string coords, string names)
    {
        try
        {
            Debug.WriteLine("Загрузка карты с маршрутом...");

            var coordinates = coords.Split('|');
            var placeNames = names.Split('|');

            if (coordinates.Length < 2)
            {
                Debug.WriteLine("Недостаточно точек для построения маршрута");
                DisplayAlert("Ошибка", "Недостаточно точек для построения маршрута", "OK");
                LoadMapWithCurrentLocationAsync().ConfigureAwait(false);
                return;
            }

            var html = GenerateRouteHtml(coordinates, placeNames);
            MapWebView.Source = new HtmlWebViewSource { Html = html };

            MapWebView.Navigated += (sender, e) => {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка загрузки карты: {ex.Message}");
            DisplayAlert("Ошибка", $"Ошибка загрузки карты: {ex.Message}", "OK");
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            LoadMapWithCurrentLocationAsync().ConfigureAwait(false);
        }
    }

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