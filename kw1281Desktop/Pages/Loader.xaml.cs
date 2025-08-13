namespace kw1281Desktop.Pages;

public partial class Loader : CommunityToolkit.Maui.Views.Popup
{
    public Loader()
    {
        InitializeComponent();
    }

    public async void HideLoader()
    {
        await CloseAsync();
    }
}
