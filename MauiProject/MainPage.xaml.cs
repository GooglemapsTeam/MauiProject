namespace Emotional_Map;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        for (int i = 0; i < 4; i++)
            MainStack.Children.Add(new PathCard(
                new Place("Радик", "Родился радистом", "a.png"),
                new Place("Новокольцово", "Молись на 56 автобус", "b.png"),
                new Place("Дурка", "Психиатрическая клиническая больница имени Н. М. Кащенко", "c.png"),
                new Place("Могила", "умру программистом", "d.png")
            ));


    }


}