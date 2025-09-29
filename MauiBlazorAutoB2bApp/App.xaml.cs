namespace MauiBlazorAutoB2bApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        //return new Window(new MainPage()) { Title = "MauiBlazorAutoB2bApp" };
        return new Window(new NavigationPage(new MainPage()))
        {
	        Title = "MauiBlazorAutoB2bApp"
        };
	}
}
