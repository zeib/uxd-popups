using UXDivers.Popups.Services;

namespace UXDivers.Popups.Maui.DemoApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
    
    // Simple Text
    private async void OnSimpleTextTapped(object sender, EventArgs e)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Title", "Sample Title" },
            { "Text", "Additional text goes here" }
        };
        
        await IPopupService.Current.PushAsync<TestPopup>(parameters);
    }
}