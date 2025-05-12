using Microsoft.Maui.Platform;

namespace Emotional_Map;

public partial class YandexMapPage : ContentPage
{
    public YandexMapPage()
    {
        InitializeComponent();
        LoadMap();
    }

    private void LoadMap()
    {
        var htmlSource = new HtmlWebViewSource();
        htmlSource.Html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"">
                <title>Яндекс Карты</title>
                <script src=""https://api-maps.yandex.ru/2.1/?apikey=ВАШ_API_КЛЮЧ&lang=ru_RU"" type=""text/javascript""></script>
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
                    ymaps.ready(init);
                    function init() {
                        var map = new ymaps.Map('map', {
                            center: [55.751574, 37.573856], // Москва
                            zoom: 10
                        });
                    }
                </script>
            </body>
            </html>"
        ;

        MapWebView.Source = htmlSource;
    }
}