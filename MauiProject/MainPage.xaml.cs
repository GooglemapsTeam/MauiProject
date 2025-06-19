using Emotional_Map.Models;
using Emotional_Map.Services;

namespace Emotional_Map;

public partial class MainPage : ContentPage
{
    private const string PLACE_CARD_BORDER_COLOR = "#E0E0E0";
    private const string PRIMARY_COLOR = "#14D0FF";
    private const string FAVORITE_COLOR = "#FF6B6B";
    private const string DISABLED_COLOR = "#CCC";
    private const string TEXT_SECONDARY_COLOR = "#666";
    private const string BACKGROUND_COLOR = "#E6F9FF";

    private List<Place> _currentRecommendations;
    private Location _userLocation;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!AppStateService.IsSurveyCompleted())
        {
            await NavigateToSurvey();
            return;
        }

        await InitializePageAsync();
    }

    private async Task InitializePageAsync()
    {
        LoadUserProfile();
        await LoadRecommendationsAsync();
        UpdateUIElements();
        CheckSurveyRetakeReminder();
    }

    private void LoadUserProfile()
    {
        var userName = Preferences.Get("Name", "Пользователь");
        WelcomeLabel.Text = $"Привет, {userName}! 👋";
        HeaderLabel.Text = userName;

        var profileImagePath = Preferences.Get("ProfileImagePath", "");
        if (!string.IsNullOrEmpty(profileImagePath))
        {
            ProfileImage.Source = profileImagePath;
        }
    }

    private async Task LoadRecommendationsAsync()
    {
        try
        {
            _currentRecommendations = RecommendationService.GetRecommendations();
            RecommendationsContainer.Children.Clear();

            if (_currentRecommendations.Any())
            {
                await DisplayRecommendations();
                NoRecommendationsFrame.IsVisible = false;
            }
            else
            {
                NoRecommendationsFrame.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось загрузить рекомендации: {ex.Message}", "OK");
        }
    }

    private async Task DisplayRecommendations()
    {
        for (int i = 0; i < _currentRecommendations.Count; i++)
        {
            var place = _currentRecommendations[i];
            var placeCard = CreatePlaceCard(place, i + 1);
            RecommendationsContainer.Children.Add(placeCard);
        }
    }

    private void UpdateUIElements()
    {
        UpdateRetakeSurveyVisibility();
    }

    private async void CheckSurveyRetakeReminder()
    {
        try
        {
            if (!AppStateService.ShouldSuggestRetakeSurvey()) return;

            var lastSurveyDate = AppStateService.GetLastSurveyDate();
            var daysSinceLastSurvey = lastSurveyDate.HasValue ?
                (int)(DateTime.Now - lastSurveyDate.Value).TotalDays : 0;

            if (daysSinceLastSurvey > 7)
            {
                var shouldRetake = await ShowSurveyRetakeDialog(daysSinceLastSurvey);
                if (shouldRetake)
                {
                    RetakeSurveyAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка проверки напоминания об опросе: {ex.Message}");
        }
    }

    private async Task<bool> ShowSurveyRetakeDialog(int daysSinceLastSurvey)
    {
        return await DisplayAlert("Обновить рекомендации",
            $"Прошло {daysSinceLastSurvey} дней с последнего опроса.\n\n" +
            "Хотите пройти опрос заново для получения актуальных рекомендаций?",
            "Да, обновить", "Позже");
    }

    private Frame CreatePlaceCard(Place place, int index)
    {
        var frame = CreateCardFrame();
        var mainLayout = new VerticalStackLayout { Spacing = 10 };

        mainLayout.Children.Add(CreateCardHeader(place, index));
        mainLayout.Children.Add(CreateDescriptionLabel(place.Description));
        mainLayout.Children.Add(CreateDetailsGrid(place));
        mainLayout.Children.Add(CreateActionButtons(place));

        frame.Content = mainLayout;
        return frame;
    }

    private Frame CreateCardFrame()
    {
        return new Frame
        {
            WidthRequest = 400,
            BackgroundColor = Colors.White,
            BorderColor = Color.FromHex(PLACE_CARD_BORDER_COLOR),
            CornerRadius = 20,
            Padding = 15,
            Margin = new Thickness(0, 5),
            HasShadow = true
        };
    }

    private HorizontalStackLayout CreateCardHeader(Place place, int index)
    {
        var headerLayout = new HorizontalStackLayout { Spacing = 10 };

        var numberLabel = new Label
        {
            Text = $"#{index}",
            FontSize = 12,
            TextColor = Colors.White,
            BackgroundColor = Color.FromHex(PRIMARY_COLOR),
            WidthRequest = 30,
            HeightRequest = 30,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        var titleLabel = new Label
        {
            Text = place.Name,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.FillAndExpand
        };

        headerLayout.Children.Add(numberLabel);
        headerLayout.Children.Add(titleLabel);

        return headerLayout;
    }

    private Label CreateDescriptionLabel(string description)
    {
        return new Label
        {
            Text = description,
            FontSize = 14,
            TextColor = Color.FromHex(TEXT_SECONDARY_COLOR),
            Margin = new Thickness(0, 5, 0, 10)
        };
    }

    private Grid CreateDetailsGrid(Place place)
    {
        var detailsGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            RowSpacing = 5
        };

        AddDetailToGrid(detailsGrid, "📍", place.District, 0, 0);
        AddDetailToGrid(detailsGrid, "👥", place.Company, 0, 1);
        AddDetailToGrid(detailsGrid, "⏰", place.Time, 1, 0);
        AddDetailToGrid(detailsGrid, "💰", place.Budget, 1, 1);

        var activityLabel = CreateDetailLabel("🎯", place.Activity);
        Grid.SetRow(activityLabel, 2);
        Grid.SetColumn(activityLabel, 0);
        Grid.SetColumnSpan(activityLabel, 2);
        detailsGrid.Children.Add(activityLabel);

        return detailsGrid;
    }

    private void AddDetailToGrid(Grid grid, string icon, string text, int row, int column)
    {
        var label = CreateDetailLabel(icon, text);
        Grid.SetRow(label, row);
        Grid.SetColumn(label, column);
        grid.Children.Add(label);
    }

    private Label CreateDetailLabel(string icon, string text)
    {
        return new Label
        {
            Text = $"{icon} {text}",
            FontSize = 12,
            TextColor = Color.FromHex(TEXT_SECONDARY_COLOR)
        };
    }

    private HorizontalStackLayout CreateActionButtons(Place place)
    {
        var buttonsLayout = new HorizontalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var mapButton = CreateMapButton(place);
        var favoriteButton = CreateFavoriteButton(place);

        buttonsLayout.Children.Add(mapButton);
        buttonsLayout.Children.Add(favoriteButton);

        return buttonsLayout;
    }

    private Button CreateMapButton(Place place)
    {
        var button = new Button
        {
            Text = "На карте",
            FontSize = 12,
            BackgroundColor = Color.FromHex(PRIMARY_COLOR),
            TextColor = Colors.White,
            CornerRadius = 15,
            Padding = new Thickness(15, 8)
        };

        button.Clicked += (s, e) => OnShowOnMapClicked(place);
        return button;
    }

    private Button CreateFavoriteButton(Place place)
    {
        var isPlaceFavorite = IsPlaceInFavorites(place.Id);
        var button = new Button
        {
            Text = isPlaceFavorite ? "❤️" : "🤍",
            FontSize = 12,
            BackgroundColor = Colors.White,
            TextColor = isPlaceFavorite ? Color.FromHex(FAVORITE_COLOR) : Color.FromHex(DISABLED_COLOR),
            BorderColor = isPlaceFavorite ? Color.FromHex(FAVORITE_COLOR) : Color.FromHex(DISABLED_COLOR),
            BorderWidth = 1,
            CornerRadius = 15,
            WidthRequest = 40,
            HeightRequest = 32
        };

        button.Clicked += (s, e) => OnTogglePlaceFavoriteClicked(place, button);
        return button;
    }

    private void UpdateRetakeSurveyVisibility()
    {
        var hasRecommendations = _currentRecommendations?.Any() == true;
        RetakeSurveyFrame.IsVisible = hasRecommendations;
        ShowFullRouteButton.IsVisible = hasRecommendations;
    }

    private bool IsPlaceInFavorites(int placeId)
    {
        try
        {
            var favorites = Preferences.Get("FavoritePlaces", "");
            if (string.IsNullOrEmpty(favorites)) return false;

            var favoritesList = favorites.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(int.Parse).ToList();
            return favoritesList.Contains(placeId);
        }
        catch
        {
            return false;
        }
    }

    private async void OnTogglePlaceFavoriteClicked(Place place, Button favoriteButton)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var favorites = GetCurrentFavorites();

            if (favorites.Contains(place.Id))
            {
                await RemoveFromFavorites(place, favoriteButton, favorites);
            }
            else
            {
                await AddToFavorites(place, favoriteButton, favorites);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось изменить избранное: {ex.Message}", "OK");
        }
    }

    private List<int> GetCurrentFavorites()
    {
        var favorites = Preferences.Get("FavoritePlaces", "");
        return string.IsNullOrEmpty(favorites) ? new List<int>() :
               favorites.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(int.Parse).ToList();
    }

    private async Task RemoveFromFavorites(Place place, Button favoriteButton, List<int> favorites)
    {
        favorites.Remove(place.Id);
        favoriteButton.Text = "🤍";
        favoriteButton.TextColor = Color.FromHex(DISABLED_COLOR);
        favoriteButton.BorderColor = Color.FromHex(DISABLED_COLOR);

        Preferences.Set("FavoritePlaces", string.Join(",", favorites));
        await DisplayAlert("Удалено", $"{place.Name} удалено из избранного", "OK");
    }

    private async Task AddToFavorites(Place place, Button favoriteButton, List<int> favorites)
    {
        favorites.Add(place.Id);
        favoriteButton.Text = "❤️";
        favoriteButton.TextColor = Color.FromHex(FAVORITE_COLOR);
        favoriteButton.BorderColor = Color.FromHex(FAVORITE_COLOR);

        Preferences.Set("FavoritePlaces", string.Join(",", favorites));
        await DisplayAlert("Добавлено", $"{place.Name} добавлено в избранное!", "OK");
    }

    private async Task NavigateToSurvey()
    {
        await Shell.Current.GoToAsync("//" + nameof(FirstSurveyPage));
    }

    private async void OnShowOnMapClicked(Place place)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var navigationParameter = new Dictionary<string, object>
            {
                { "SelectedPlace", place }
            };

            await Shell.Current.GoToAsync($"//{nameof(YandexMapPage)}", navigationParameter);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть карту: {ex.Message}", "OK");
        }
    }

    private async void OnShowFullRouteClicked(object sender, EventArgs e)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            if (_currentRecommendations?.Any() != true)
            {
                await DisplayAlert("Ошибка", "Нет рекомендаций для отображения маршрута", "OK");
                return;
            }

            var navigationParameter = new Dictionary<string, object>
            {
                { "AllRecommendations", _currentRecommendations }
            };

            await Shell.Current.GoToAsync($"//{nameof(YandexMapPage)}", navigationParameter);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть карту: {ex.Message}", "OK");
        }
    }

    public async void OnUpdatePathesClicked(object sender, EventArgs e)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var button = sender as Button;
            if (button != null)
            {
                button.Text = "Обновление...";
                button.IsEnabled = false;
            }

            await LoadRecommendationsAsync();
            UpdateUIElements();

            if (button != null)
            {
                button.Text = "🔄";
                button.IsEnabled = true;
            }

            await DisplayAlert("Обновлено", "Маршруты обновлены на основе ваших предпочтений", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось обновить маршруты: {ex.Message}", "OK");

            var button = sender as Button;
            if (button != null)
            {
                button.Text = "🔄";
                button.IsEnabled = true;
            }
        }
    }

    public async void OnFavouriteClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(FavouritePage), true);
    }

    public async void OnProfileClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(ProfilePage), true);
    }

    private async void OnAddRouteToFavoritesClicked(object sender, EventArgs e)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            if (_currentRecommendations?.Any() != true)
            {
                await DisplayAlert("Ошибка", "Нет рекомендаций для добавления в избранное", "OK");
                return;
            }

            await ShowRouteSelectionDialog();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось добавить маршрут в избранное: {ex.Message}", "OK");
        }
    }

    private async void OnRetakeSurveyClicked(object sender, EventArgs e)
    {
        try
        {
            if (sender != null)
                AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var shouldRetake = await DisplayAlert("Пройти опрос заново",
                "Вы хотите пройти опрос заново? Это поможет получить новые персонализированные рекомендации на основе вашего текущего настроения.",
                "Да, пройти опрос", "Отмена");

            if (shouldRetake)
            {
                await HandleSurveyRetake();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось перейти к опросу: {ex.Message}", "OK");
        }
    }

    private async Task HandleSurveyRetake()
    {
        var clearPreviousAnswers = await DisplayAlert("Очистить предыдущие ответы",
            "Хотите очистить предыдущие ответы и начать с чистого листа?",
            "Да, очистить", "Нет, оставить");

        if (clearPreviousAnswers)
        {
            AppStateService.ResetSurvey();
        }

        await Shell.Current.GoToAsync("//" + nameof(FirstSurveyPage), true);
    }

    private async Task RetakeSurveyAsync()
    {
        try
        {
            var shouldRetake = await DisplayAlert("Пройти опрос заново",
                "Вы хотите пройти опрос заново? Это поможет получить новые персонализированные рекомендации на основе вашего текущего настроения.",
                "Да, пройти опрос", "Отмена");

            if (shouldRetake)
            {
                await HandleSurveyRetake();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось перейти к опросу: {ex.Message}", "OK");
        }
    }

    private async Task ShowRouteSelectionDialog()
    {
        try
        {
            var selectionPage = CreateRouteSelectionPage();
            await Navigation.PushModalAsync(new NavigationPage(selectionPage));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть диалог выбора: {ex.Message}", "OK");
        }
    }

    private ContentPage CreateRouteSelectionPage()
    {
        var selectionPage = new ContentPage
        {
            Title = "Выбор маршрута",
            BackgroundColor = Color.FromHex(BACKGROUND_COLOR)
        };

        var scrollView = new ScrollView();
        var mainLayout = new VerticalStackLayout { Padding = 20, Spacing = 15 };

        var titleLabel = new Label
        {
            Text = "Выберите места для маршрута",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        mainLayout.Children.Add(titleLabel);

        var selectedPlaces = new List<Place>();
        var checkBoxes = new List<CheckBox>();

        foreach (var place in _currentRecommendations)
        {
            var placeFrame = CreatePlaceSelectionFrame(place, selectedPlaces, checkBoxes);
            mainLayout.Children.Add(placeFrame);
            selectedPlaces.Add(place);
        }

        var optimizeButton = CreateOptimizeButton(selectedPlaces, checkBoxes);
        mainLayout.Children.Add(optimizeButton);

        var analysisFrame = CreateAnalysisFrame(selectedPlaces, checkBoxes);
        mainLayout.Children.Add(analysisFrame);

        var nameEntry = new Entry
        {
            Placeholder = "Введите название маршрута",
            Text = $"Маршрут от {DateTime.Now:dd.MM.yyyy}",
            FontSize = 16,
            Margin = new Thickness(0, 20, 0, 0)
        };
        mainLayout.Children.Add(nameEntry);

        var buttonsLayout = CreateDialogButtons(selectedPlaces, nameEntry);
        mainLayout.Children.Add(buttonsLayout);

        scrollView.Content = mainLayout;
        selectionPage.Content = scrollView;

        return selectionPage;
    }

    private Frame CreatePlaceSelectionFrame(Place place, List<Place> selectedPlaces, List<CheckBox> checkBoxes)
    {
        var placeFrame = new Frame
        {
            BackgroundColor = Colors.White,
            BorderColor = Color.FromHex(PLACE_CARD_BORDER_COLOR),
            CornerRadius = 10,
            Padding = 15,
            Margin = new Thickness(0, 5)
        };

        var placeLayout = new HorizontalStackLayout { Spacing = 15 };

        var checkBox = new CheckBox
        {
            IsChecked = true,
            Color = Color.FromHex(PRIMARY_COLOR)
        };
        checkBoxes.Add(checkBox);

        var placeInfo = new VerticalStackLayout { Spacing = 5, HorizontalOptions = LayoutOptions.FillAndExpand };

        var placeNameLabel = new Label
        {
            Text = place.Name,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black
        };

        var placeDetailsLabel = new Label
        {
            Text = $"{place.District} • {place.Activity}",
            FontSize = 12,
            TextColor = Color.FromHex(TEXT_SECONDARY_COLOR)
        };

        placeInfo.Children.Add(placeNameLabel);
        placeInfo.Children.Add(placeDetailsLabel);

        placeLayout.Children.Add(checkBox);
        placeLayout.Children.Add(placeInfo);

        placeFrame.Content = placeLayout;

        checkBox.CheckedChanged += (s, e) =>
        {
            if (e.Value)
            {
                if (!selectedPlaces.Contains(place))
                    selectedPlaces.Add(place);
            }
            else
            {
                selectedPlaces.Remove(place);
            }
        };

        return placeFrame;
    }

    private Button CreateOptimizeButton(List<Place> selectedPlaces, List<CheckBox> checkBoxes)
    {
        var optimizeButton = new Button
        {
            Text = "🎯 Оптимизировать порядок мест",
            FontSize = 12,
            BackgroundColor = Color.FromHex("#FF9800"),
            TextColor = Colors.White,
            CornerRadius = 15,
            Padding = new Thickness(15, 8),
            Margin = new Thickness(0, 10, 0, 0)
        };

        optimizeButton.Clicked += (s, e) =>
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            if (selectedPlaces.Count > 1)
            {
                var optimizedPlaces = RouteOptimizationService.OptimizeRoute(selectedPlaces, _userLocation);
                selectedPlaces.Clear();
                selectedPlaces.AddRange(optimizedPlaces);

                for (int i = 0; i < checkBoxes.Count; i++)
                {
                    var place = _currentRecommendations[i];
                    var isSelected = selectedPlaces.Contains(place);
                    checkBoxes[i].IsChecked = isSelected;
                }

                DisplayAlert("Успех", "Маршрут оптимизирован! Порядок мест изменен для минимизации расстояния.", "OK");
            }
            else
            {
                DisplayAlert("Информация", "Для оптимизации нужно выбрать минимум 2 места", "OK");
            }
        };

        return optimizeButton;
    }

    private Frame CreateAnalysisFrame(List<Place> selectedPlaces, List<CheckBox> checkBoxes)
    {
        var analysisFrame = new Frame
        {
            BackgroundColor = Color.FromHex("#F0F8FF"),
            BorderColor = Color.FromHex(PRIMARY_COLOR),
            CornerRadius = 10,
            Padding = 15,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var analysisLayout = new VerticalStackLayout { Spacing = 8 };
        var analysisLabel = new Label
        {
            Text = "📊 Анализ маршрута",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black
        };
        analysisLayout.Children.Add(analysisLabel);

        var updateAnalysis = new Action(() => UpdateRouteAnalysis(analysisLayout, selectedPlaces));

        foreach (var checkBox in checkBoxes)
        {
            checkBox.CheckedChanged += (s, e) => updateAnalysis();
        }

        updateAnalysis();

        analysisFrame.Content = analysisLayout;
        return analysisFrame;
    }

    private void UpdateRouteAnalysis(VerticalStackLayout analysisLayout, List<Place> selectedPlaces)
    {
        if (analysisLayout.Children.Count > 1)
        {
            for (int i = analysisLayout.Children.Count - 1; i > 0; i--)
            {
                analysisLayout.Children.RemoveAt(i);
            }
        }

        if (selectedPlaces.Count > 0)
        {
            var analysis = RouteOptimizationService.AnalyzeRoute(selectedPlaces, _userLocation);

            var analysisText = $"📏 Расстояние: {analysis.TotalDistance} км\n" +
                              $"🚶 Время в пути: {analysis.GetFormattedWalkingTime()}\n" +
                              $"⏱️ Общее время: {analysis.GetFormattedTime()}\n" +
                              $"🏘️ Районов: {analysis.DistrictsCount}\n" +
                              $"🎯 Основная активность: {analysis.MainActivity}";

            var analysisDetailsLabel = new Label
            {
                Text = analysisText,
                FontSize = 12,
                TextColor = Color.FromHex(TEXT_SECONDARY_COLOR)
            };

            analysisLayout.Children.Add(analysisDetailsLabel);

            if (analysis.Recommendations.Any())
            {
                var recommendationsLabel = new Label
                {
                    Text = "💡 Рекомендации:\n" + string.Join("\n", analysis.Recommendations),
                    FontSize = 11,
                    TextColor = Color.FromHex("#FF9800"),
                    Margin = new Thickness(0, 5, 0, 0)
                };
                analysisLayout.Children.Add(recommendationsLabel);
            }
        }
    }

    private HorizontalStackLayout CreateDialogButtons(List<Place> selectedPlaces, Entry nameEntry)
    {
        var buttonsLayout = new HorizontalStackLayout
        {
            Spacing = 15,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var cancelButton = new Button
        {
            Text = "Отмена",
            FontSize = 14,
            BackgroundColor = Color.FromHex(DISABLED_COLOR),
            TextColor = Colors.White,
            CornerRadius = 20,
            Padding = new Thickness(20, 10)
        };

        var saveButton = new Button
        {
            Text = "Сохранить маршрут",
            FontSize = 14,
            BackgroundColor = Color.FromHex(PRIMARY_COLOR),
            TextColor = Colors.White,
            CornerRadius = 20,
            Padding = new Thickness(20, 10)
        };

        cancelButton.Clicked += async (s, e) =>
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
            await Navigation.PopModalAsync();
        };

        saveButton.Clicked += async (s, e) => await SaveRouteToFavorites(selectedPlaces, nameEntry);

        buttonsLayout.Children.Add(cancelButton);
        buttonsLayout.Children.Add(saveButton);

        return buttonsLayout;
    }

    private async Task SaveRouteToFavorites(List<Place> selectedPlaces, Entry nameEntry)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

        if (selectedPlaces.Count == 0)
        {
            await DisplayAlert("Ошибка", "Выберите хотя бы одно место для маршрута", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(nameEntry.Text))
        {
            await DisplayAlert("Ошибка", "Введите название маршрута", "OK");
            return;
        }

        var description = $"Маршрут из {selectedPlaces.Count} мест, созданный {DateTime.Now:dd.MM.yyyy HH:mm}";
        var favoriteRoute = new FavoriteRoute(nameEntry.Text, description, selectedPlaces);

        var success = FavoriteRoutesService.AddFavoriteRoute(favoriteRoute);
        if (success)
        {
            await Navigation.PopModalAsync();
            await DisplayAlert("Успех", $"Маршрут \"{nameEntry.Text}\" добавлен в избранное!", "OK");
        }
        else
        {
            await DisplayAlert("Ошибка", "Маршрут с таким названием уже существует в избранном", "OK");
        }
    }
}