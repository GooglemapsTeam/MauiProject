namespace Emotional_Map;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        for (int i = 0; i < 4; i++)
            MainStack.Children.Add(new PathCard(
                new Place("�����", "������� ��������", "a.png"),
                new Place("������������", "������ �� 56 �������", "b.png"),
                new Place("�����", "��������������� ����������� �������� ����� �. �. �������", "c.png"),
                new Place("������", "���� �������������", "d.png")
            ));


    }


}