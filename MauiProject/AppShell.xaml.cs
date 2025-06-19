using Emotional_Map.Services;

namespace Emotional_Map;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        RegisterRoutes();

        _ = SetInitialRouteAsync();
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(FirstSurveyPage), typeof(FirstSurveyPage));
        Routing.RegisterRoute(nameof(SecondSurveyPage), typeof(SecondSurveyPage));
        Routing.RegisterRoute(nameof(ThirdSurveyPage), typeof(ThirdSurveyPage));
        Routing.RegisterRoute(nameof(FourthSurveyPage), typeof(FourthSurveyPage));
        Routing.RegisterRoute(nameof(FifthSurveyPage), typeof(FifthSurveyPage));
        Routing.RegisterRoute(nameof(SixthSurveyPage), typeof(SixthSurveyPage));
        Routing.RegisterRoute(nameof(SeventhSurveyPage), typeof(SeventhSurveyPage));
        Routing.RegisterRoute(nameof(EighthSurveyPage), typeof(EighthSurveyPage));
        Routing.RegisterRoute(nameof(ProfileImageSetPage), typeof(ProfileImageSetPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(YandexMapPage), typeof(YandexMapPage));
        Routing.RegisterRoute(nameof(FavouritePage), typeof(FavouritePage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
    }

    private async Task SetInitialRouteAsync()
    {
        try
        {
            await Task.Delay(100);

            if (AppStateService.IsSurveyCompleted())
            {
                await GoToAsync("//" + nameof(MainPage));
                System.Diagnostics.Debug.WriteLine("Переход к главной странице - опрос завершен");
            }
            else
            {
                await GoToAsync("//" + nameof(FirstSurveyPage));
                System.Diagnostics.Debug.WriteLine("Переход к опросу - опрос не завершен");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка установки начального маршрута: {ex.Message}");
            await GoToAsync("//" + nameof(FirstSurveyPage));
        }
    }
}
