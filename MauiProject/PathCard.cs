﻿using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System.Globalization;
using System.Diagnostics;

namespace Emotional_Map
{
    public class PathCard : Border
    {
        public const int CardWidth = 350;
        public const int CardHeight = 170;
        public int Duration { get; private set; } = 666;
        private Place[] _places;
        private bool _isFavorited = false;
        private Border _favouriteButton;
        private VerticalStackLayout _parent;
        private bool _isNavigating = false; // Флаг для предотвращения множественных нажатий

        public PathCard(VerticalStackLayout parent, params Place[] place)
        {
            _places = place;
            _parent = parent;
            InitializeCard();
        }

        private void OnCrossClicked(object sender, EventArgs e)
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
            if (_parent != null && _parent.Children.Contains(this))
            {
                _parent.Children.Remove(this);
            }
        }

        private void InitializeCard()
        {
            BackgroundColor = Color.FromArgb("#FFFEFE");
            WidthRequest = CardWidth;
            HeightRequest = CardHeight;
            Padding = new Thickness(0);
            Stroke = Colors.Transparent;
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) };
            HorizontalOptions = LayoutOptions.Center;

            var grid = new Grid();
            Content = grid;

            var actionLabel = new Label
            {
                Text = "В путь!",
                FontSize = 13,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            var actionButton = new Border
            {
                WidthRequest = 75,
                HeightRequest = 20,
                Stroke = Colors.Transparent,
                BackgroundColor = Colors.White,
                Padding = new Thickness(0),
                Margin = new Thickness(0, 5, 14, 0),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(5) },
                Shadow = new Shadow
                {
                    Brush = Colors.Black,
                    Opacity = 0.25f,
                    Radius = 4,
                    Offset = new Point(0, 4)
                }
            };

            var durationLabel = new Label
            {
                Text = Duration.ToString(),
                FontSize = 10,
                TextColor = Color.FromArgb("#757575"),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(15, 5, 0, 0),
            };
            actionButton.Content = actionLabel;

            actionButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnToPathClicked(this, EventArgs.Empty))
            });

            var infoButton = CreateIconButton("info_button.png", new Thickness(0, 4, 100, 0),
                LayoutOptions.End, LayoutOptions.Start);
            infoButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnInfoClicked(this, EventArgs.Empty))
            });

            var crossButton = CreateIconButton("cross_button.png", new Thickness(0, 0, 20, 5),
                LayoutOptions.End, LayoutOptions.End);
            crossButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnCrossClicked(this, EventArgs.Empty))
            });

            _favouriteButton = CreateIconButton(_isFavorited ? "favourite_button_active.png" : "favourite_button.png",
                new Thickness(0, 0, 50, 5),
                LayoutOptions.End, LayoutOptions.End);
            _favouriteButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => OnFavouriteClicked(this, EventArgs.Empty))
            });

            var placesContainer = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 19,
                Margin = new Thickness(26, 0, 0, 0)
            };

            for (var x = 0; x < _places.Length; x++)
            {
                var placeItem = new VerticalStackLayout
                {
                    Margin = new Thickness(0, 15, 0, 0),
                    Spacing = 5
                };

                var icon = new Border
                {
                    Stroke = Colors.Transparent,
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(30, 30, 30, 30)
                    },
                    WidthRequest = 60,
                    HeightRequest = 60,
                    Content = new Image
                    {
                        Source = _places[x].ImageSource,
                        Aspect = Aspect.AspectFill
                    },
                };

                var label = new Label()
                {
                    Text = _places[x].Title,
                    WidthRequest = 60,
                    LineBreakMode = LineBreakMode.WordWrap,
                    FontSize = 10,
                    HorizontalTextAlignment = TextAlignment.Center,
                    MaxLines = 3,
                    TextColor = Colors.Black,
                };

                placeItem.Children.Add(icon);
                placeItem.Children.Add(label);
                placesContainer.Children.Add(placeItem);
            }

            grid.Children.Add(placesContainer);
            grid.Children.Add(actionButton);
            grid.Children.Add(infoButton);
            grid.Children.Add(crossButton);
            grid.Children.Add(_favouriteButton);
            grid.Children.Add(durationLabel);
        }

        private Border CreateIconButton(string imageSource, Thickness margin,
            LayoutOptions horizontalOptions, LayoutOptions verticalOptions)
        {
            return new Border
            {
                WidthRequest = 24,
                HeightRequest = 24,
                Stroke = Colors.Transparent,
                BackgroundColor = Colors.Transparent,
                Margin = margin,
                HorizontalOptions = horizontalOptions,
                VerticalOptions = verticalOptions,
                Content = new Image
                {
                    Source = imageSource,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };
        }

        private Page CreateDescriptionWindow(Place place, sbyte number)
        {
            var circlesContainer = new HorizontalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 15
            };

            for (var i = 1; i <= 4; i++)
            {
                circlesContainer.Children.Add(new Image
                {
                    Source = i - 1 == number ? "dark_circle.png" : "light_circle.png",
                    WidthRequest = 10,
                    HeightRequest = 10
                });
            }

            var placeImage = new Image
            {
                Source = place.ImageSource,
                Aspect = Aspect.AspectFill,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var imageBorder = new Border
            {
                Stroke = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 60 },
                WidthRequest = 120,
                HeightRequest = 120,
                HorizontalOptions = LayoutOptions.Center,
                Content = placeImage
            };

            var titleLabel = new Label
            {
                Text = place.Title,
                FontSize = 15,
                WidthRequest = 300,
                HorizontalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Black
            };

            var descLabel = new Label
            {
                Text = place.Description,
                FontSize = 10,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Black
            };

            var page = new ContentPage
            {
                BackgroundColor = Colors.White,
                Content = new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Spacing = 20,
                        Padding = new Thickness(20),
                        WidthRequest = 300,
                        Children = { circlesContainer, imageBorder, titleLabel, descLabel },
                    }
                }
            };

            var rightSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
            rightSwipe.Swiped += async (sender, e) =>
            {
                AudioPlayer.PlaySound(AudioPlayer.SlideSound);
                if (number < 3)
                {
                    await Navigation.PushAsync(CreateDescriptionWindow(_places[number + 1], (sbyte)(number + 1)), false);
                }
                else
                {
                    await Shell.Current.GoToAsync("//" + nameof(MainPage), false);
                }
            };

            var leftSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
            leftSwipe.Swiped += async (sender, e) =>
            {
                AudioPlayer.PlaySound(AudioPlayer.SlideSound);
                if (number > 0)
                    await Navigation.PushAsync(CreateDescriptionWindow(_places[number - 1], (sbyte)(number - 1)), false);
                else
                {
                    await Shell.Current.GoToAsync("//" + nameof(MainPage), false);
                }
            };
            page.Content.GestureRecognizers.Add(rightSwipe);
            page.Content.GestureRecognizers.Add(leftSwipe);

            return page;
        }

        private async void OnToPathClicked(object sender, EventArgs e)
        {
            if (_isNavigating)
                return;

            _isNavigating = true;
            AudioPlayer.PlaySound(AudioPlayer.ToPathButtonClickSound);
            try
            {
                if (_places == null || _places.Length < 1)
                {
                    await Application.Current.MainPage.DisplayAlert("Ошибка", "Для построения маршрута нужна минимум 1 точка", "OK");
                    return;
                }

                // Показываем индикатор загрузки
                var loadingIndicator = new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Colors.Blue,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                var loadingLabel = new Label
                {
                    Text = "Получение местоположения...",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var loadingLayout = new VerticalStackLayout
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Children = { loadingIndicator, loadingLabel }
                };

                var loadingPage = new ContentPage
                {
                    Content = loadingLayout,
                    BackgroundColor = Colors.White
                };

                await Application.Current.MainPage.Navigation.PushModalAsync(loadingPage);

                // Получаем текущее местоположение
                var location = await LocationService.Instance.GetCurrentLocationAsync();

                // Объявляем переменные для координат и названий мест один раз
                List<string> coordinatesList;
                List<string> placeNamesList;
                string coordinates;
                string placeNames;
                string encodedCoords;
                string encodedNames;

                if (location == null)
                {
                    await Application.Current.MainPage.Navigation.PopModalAsync();
                    await Application.Current.MainPage.DisplayAlert("Ошибка",
                        "Не удалось получить местоположение. Маршрут будет построен только между выбранными точками.", "OK");

                    // Строим маршрут только между выбранными точками
                    coordinates = string.Join("|", _places.Select(p =>
                        $"{p.Latitude.ToString(CultureInfo.InvariantCulture)},{p.Longitude.ToString(CultureInfo.InvariantCulture)}"));

                    placeNames = string.Join("|", _places.Select(p => p.Title));
                }
                else
                {
                    // Обновляем текст загрузки
                    loadingLabel.Text = "Построение маршрута...";

                    // Создаем список координат, начиная с текущего местоположения
                    coordinatesList = new List<string>
            {
                $"{location.Latitude.ToString(CultureInfo.InvariantCulture)},{location.Longitude.ToString(CultureInfo.InvariantCulture)}"
            };

                    // Добавляем координаты выбранных мест
                    coordinatesList.AddRange(_places.Select(p =>
                        $"{p.Latitude.ToString(CultureInfo.InvariantCulture)},{p.Longitude.ToString(CultureInfo.InvariantCulture)}"));

                    // Объединяем координаты в строку
                    coordinates = string.Join("|", coordinatesList);

                    // Создаем список названий мест, начиная с "Моё местоположение"
                    placeNamesList = new List<string> { "Моё местоположение" };
                    placeNamesList.AddRange(_places.Select(p => p.Title));
                    placeNames = string.Join("|", placeNamesList);
                }

                // Кодируем параметры
                encodedCoords = Uri.EscapeDataString(coordinates);
                encodedNames = Uri.EscapeDataString(placeNames);

                Debug.WriteLine($"Координаты: {coordinates}");
                Debug.WriteLine($"Названия: {placeNames}");
                Debug.WriteLine($"URL: //YandexMapPage?coords={encodedCoords}&names={encodedNames}");

                // Закрываем индикатор загрузки
                await Application.Current.MainPage.Navigation.PopModalAsync();

                // Переходим на страницу карты
                await Shell.Current.GoToAsync($"//YandexMapPage?coords={encodedCoords}&names={encodedNames}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при построении маршрута: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // В случае ошибки показываем сообщение
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    $"Произошла ошибка при построении маршрута: {ex.Message}", "OK");

                // Закрываем индикатор загрузки, если он открыт
                if (Application.Current.MainPage.Navigation.ModalStack.Count > 0)
                {
                    await Application.Current.MainPage.Navigation.PopModalAsync();
                }
            }
            finally
            {
                _isNavigating = false;
            }
        }
        private async void OnFavouriteClicked(object sender, EventArgs e)
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
            _isFavorited = !_isFavorited;

            if (_favouriteButton.Content is Image img)
            {
                await img.ScaleTo(0.8, 100, Easing.SinInOut);
                img.Source = _isFavorited ? "favourite_button_active.png" : "favourite_button.png";
                await img.ScaleTo(1, 100, Easing.SinInOut);
            }
            // Добавление в избранное не работает
        }

        private async void OnInfoClicked(object sender, EventArgs e)
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
            await Navigation.PushAsync(CreateDescriptionWindow(_places[0], 0), false);
        }

        private async void OnUpdateClicked(object sender, EventArgs e)
        {
            AudioPlayer.PlaySound(AudioPlayer.ButtonClickSound);
            await Shell.Current.GoToAsync("//" + nameof(FavouritePage), true);
        }
    }
}