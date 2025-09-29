using System.Diagnostics;

namespace MauiBlazorAutoB2bApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnNativeButtonClicked(object? sender, EventArgs e)
    {
	    Debug.WriteLine("OnNativeButtonClicked");
    }
}
