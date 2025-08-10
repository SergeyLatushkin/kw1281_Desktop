namespace kw1281Desktop.Pages;

public partial class AppSettingsPage : ContentPage
{
	public AppSettingsPage(AppSettingsPageViewModel model)
	{
		InitializeComponent();
        BindingContext = model;
    }
}