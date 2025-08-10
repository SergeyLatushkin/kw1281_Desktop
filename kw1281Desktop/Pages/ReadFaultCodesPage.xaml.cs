using kw1281Desktop.Helpers;

namespace kw1281Desktop.Pages;

public partial class ReadFaultCodesPage : ContentPage
{
    public ReadFaultCodesPage(ReadFaultCodesPageViewModel model)
    {
        InitializeComponent();
        BindingContext = model;

        PageHelper.HookLogBinding(model.LogLines, LogContainer, ScrollView);
    }

    private async void OnLinkTapped(object sender, EventArgs e)
    {
        await Launcher.OpenAsync("https://www.blafusel.de/obd/vag_codes.php");
    }
}