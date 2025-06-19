using Emotional_Map.Models;
using Emotional_Map.Services;

namespace Emotional_Map;

public partial class FavouritePage : ContentPage
{
    private List<FavoriteRoute> _favoriteRoutes;
    private List<Place> _favoritePlaces;

    public FavouritePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadFavorites();
    }

    private void LoadFavorites()
    {
        try
        {
            LoadFavoriteRoutes();
            LoadFavoritePlaces();
            UpdateVisibility();
        }
        catch (Exception ex)
        {
            DisplayAlert("Ошибка", $"Не удалось загрузить избранное: {ex.Message}", "OK");
        }
    }

    private void LoadFavoriteRoutes()
    {
        _favoriteRoutes = Emotional_Map.Services.FavoriteRoutesService.GetFavoriteRoutes();

        FavoriteRoutesContainer.Children.Clear();

        if (_favoriteRoutes.Any())
        {
            NoFavoriteRoutesLabel.IsVisible = false;

            foreach (var route in _favoriteRoutes.OrderByDescending(r => r.CreatedAt))
            {
                var routeCard = CreateFavoriteRouteCard(route);
                FavoriteRoutesContainer.Children.Add(routeCard);
            }
        }
        else
        {
            NoFavoriteRoutesLabel.IsVisible = true;
        }
    }

    private void LoadFavoritePlaces()
    {
        try
        {
            var favorites = Preferences.Get("FavoritePlaces", "");
            var favoriteIds = string.IsNullOrEmpty(favorites) ? new List<int>() :
                             favorites.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(int.Parse).ToList();

            var allPlaces = Emotional_Map.Services.RecommendationService.GetAllPlaces();
            _favoritePlaces = allPlaces.Where(p => favoriteIds.Contains(p.Id)).ToList();

            FavoritePlacesContainer.Children.Clear();

            if (_favoritePlaces.Any())
            {
                NoFavoritePlacesLabel.IsVisible = false;

                foreach (var place in _favoritePlaces)
                {
                    var placeCard = CreateFavoritePlaceCard(place);
                    FavoritePlacesContainer.Children.Add(placeCard);
                }
            }
            else
            {
                NoFavoritePlacesLabel.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки избранных мест: {ex.Message}");
            _favoritePlaces = new List<Place>();
            NoFavoritePlacesLabel.IsVisible = true;
        }
    }

    private void UpdateVisibility()
    {
        var hasAnyFavorites = _favoriteRoutes.Any() || _favoritePlaces.Any();

        if (hasAnyFavorites)
        {
            NoFavoritesFrame.IsVisible = false;
            FavoritePlacesFrame.IsVisible = true;
            FavoriteRoutesFrame.IsVisible = true;

            var totalCount = _favoriteRoutes.Count + _favoritePlaces.Count;
            FavoritesCountLabel.Text = $"У вас {totalCount} избранных элементов";
        }
        else
        {
            NoFavoritesFrame.IsVisible = true;
            FavoritePlacesFrame.IsVisible = false;
            FavoriteRoutesFrame.IsVisible = false;
            FavoritesCountLabel.Text = "У вас нет избранного";
        }
    }

    private Frame CreateFavoritePlaceCard(Place place)
    {
        var frame = new Frame
        {
            BackgroundColor = Color.FromHex("#F8F9FA"),
            BorderColor = Color.FromHex("#E0E0E0"),
            CornerRadius = 10,
            Padding = 12,
            Margin = new Thickness(0, 2)
        };

        var layout = new HorizontalStackLayout { Spacing = 12 };

        var heartLabel = new Label
        {
            Text = "❤️",
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center
        };

        var infoLayout = new VerticalStackLayout
        {
            Spacing = 3,
            HorizontalOptions = LayoutOptions.FillAndExpand
        };

        var nameLabel = new Label
        {
            Text = place.Name,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black
        };

        var detailsLabel = new Label
        {
            Text = $"{place.District} • {place.Activity}",
            FontSize = 12,
            TextColor = Color.FromHex("#666")
        };

        infoLayout.Children.Add(nameLabel);
        infoLayout.Children.Add(detailsLabel);

        var removeButton = new Button
        {
            Text = "🗑️",
            FontSize = 12,
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromHex("#FF6B6B"),
            WidthRequest = 30,
            HeightRequest = 30,
            VerticalOptions = LayoutOptions.Center
        };
        removeButton.Clicked += (s, e) => OnRemovePlaceFromFavoritesClicked(place);

        layout.Children.Add(heartLabel);
        layout.Children.Add(infoLayout);
        layout.Children.Add(removeButton);

        frame.Content = layout;
        return frame;
    }

    private Frame CreateFavoriteRouteCard(FavoriteRoute route)
    {
        var frame = new Frame
        {
            BackgroundColor = Colors.White,
            BorderColor = Color.FromHex("#E0E0E0"),
            CornerRadius = 15,
            Padding = 15,
            Margin = new Thickness(0, 5),
            HasShadow = true
        };

        var mainLayout = new VerticalStackLayout { Spacing = 12 };

        var headerLayout = new HorizontalStackLayout { Spacing = 10 };

        var favoriteIcon = new Label
        {
            Text = "⭐",
            FontSize = 20,
            VerticalOptions = LayoutOptions.Center
        };

        var titleLabel = new Label
        {
            Text = route.Name,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.FillAndExpand
        };

        var dateLabel = new Label
        {
            Text = route.CreatedAt.ToString("dd.MM.yyyy"),
            FontSize = 12,
            TextColor = Color.FromHex("#999"),
            VerticalOptions = LayoutOptions.Center
        };

        headerLayout.Children.Add(favoriteIcon);
        headerLayout.Children.Add(titleLabel);
        headerLayout.Children.Add(dateLabel);

        var descriptionLabel = new Label
        {
            Text = route.Description,
            FontSize = 14,
            TextColor = Color.FromHex("#666"),
            Margin = new Thickness(0, 5, 0, 10)
        };

        var routeInfoLayout = new HorizontalStackLayout { Spacing = 15 };

        var placesCountLabel = new Label
        {
            Text = $"📍 {route.Places.Count} мест",
            FontSize = 12,
            TextColor = Color.FromHex("#666")
        };

        var distanceLabel = new Label
        {
            Text = $"📏 {route.TotalDistance} км",
            FontSize = 12,
            TextColor = Color.FromHex("#666")
        };

        var timeLabel = new Label
        {
            Text = $"⏱️ {route.EstimatedTime} мин",
            FontSize = 12,
            TextColor = Color.FromHex("#666")
        };

        routeInfoLayout.Children.Add(placesCountLabel);
        routeInfoLayout.Children.Add(distanceLabel);
        routeInfoLayout.Children.Add(timeLabel);

        var placesLayout = new VerticalStackLayout { Spacing = 5, Margin = new Thickness(0, 10, 0, 0) };
        var placesToShow = route.Places.Take(3).ToList();

        foreach (var place in placesToShow)
        {
            var placeLabel = new Label
            {
                Text = $"• {place.Name} ({place.District})",
                FontSize = 12,
                TextColor = Color.FromHex("#666")
            };
            placesLayout.Children.Add(placeLabel);
        }

        if (route.Places.Count > 3)
        {
            var moreLabel = new Label
            {
                Text = $"... и еще {route.Places.Count - 3} мест",
                FontSize = 12,
                TextColor = Color.FromHex("#999"),
                FontAttributes = FontAttributes.Italic
            };
            placesLayout.Children.Add(moreLabel);
        }

        var buttonsLayout = new HorizontalStackLayout
        {
            Spacing = 10,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 15, 0, 0)
        };

        var openMapButton = new Button
        {
            Text = "Открыть на карте",
            FontSize = 12,
            BackgroundColor = Color.FromHex("#14D0FF"),
            TextColor = Colors.White,
            CornerRadius = 15,
            Padding = new Thickness(15, 8)
        };
        openMapButton.Clicked += (s, e) => OnOpenRouteOnMapClicked(route);

        var removeButton = new Button
        {
            Text = "🗑️",
            FontSize = 12,
            BackgroundColor = Colors.White,
            TextColor = Color.FromHex("#FF6B6B"),
            BorderColor = Color.FromHex("#FF6B6B"),
            BorderWidth = 1,
            CornerRadius = 15,
            WidthRequest = 40,
            HeightRequest = 32
        };


        buttonsLayout.Children.Add(openMapButton);
        buttonsLayout.Children.Add(removeButton);

        mainLayout.Children.Add(headerLayout);
        mainLayout.Children.Add(descriptionLabel);
        mainLayout.Children.Add(routeInfoLayout);
        mainLayout.Children.Add(placesLayout);
        mainLayout.Children.Add(buttonsLayout);

        frame.Content = mainLayout;
        return frame;
    }

    private async void OnRemovePlaceFromFavoritesClicked(Place place)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var result = await DisplayAlert("Подтверждение",
                $"Удалить \"{place.Name}\" из избранного?",
                "Удалить", "Отмена");

            if (result)
            {
                var favorites = Preferences.Get("FavoritePlaces", "");
                var favoritesList = string.IsNullOrEmpty(favorites) ? new List<int>() :
                                   favorites.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(int.Parse).ToList();

                favoritesList.Remove(place.Id);
                Preferences.Set("FavoritePlaces", string.Join(",", favoritesList));

                LoadFavorites();
                await DisplayAlert("Успех", "Место удалено из избранного", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка удаления места: {ex.Message}", "OK");
        }
    }

    private async void OnOpenRouteOnMapClicked(FavoriteRoute route)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var navigationParameter = new Dictionary<string, object>
            {
                { "AllRecommendations", route.Places }
            };

            await Shell.Current.GoToAsync($"//{nameof(YandexMapPage)}", navigationParameter);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть карту: {ex.Message}", "OK");
        }
    }

    private async void OnGoToRecommendationsClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(MainPage));
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
        await Shell.Current.GoToAsync("//" + nameof(MainPage));
    }

    private async void OnClearAllClicked(object sender, EventArgs e)
    {
        try
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);

            var hasAnyFavorites = _favoriteRoutes.Any() || _favoritePlaces.Any();
            if (!hasAnyFavorites)
            {
                await DisplayAlert("Информация", "Нет избранного для удаления", "OK");
                return;
            }

            var result = await DisplayAlert("Подтверждение",
                "Удалить все избранное (места и маршруты)? Это действие нельзя отменить.",
                "Удалить все", "Отмена");

            if (result)
            {
                Emotional_Map.Services.FavoriteRoutesService.ClearAllFavorites();
                Preferences.Remove("FavoritePlaces");
                LoadFavorites();
                await DisplayAlert("Успех", "Все избранное удалено", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Ошибка очистки избранного: {ex.Message}", "OK");
        }
    }
}