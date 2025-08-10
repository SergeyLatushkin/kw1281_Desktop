using kw1281Desktop.Helpers;

namespace kw1281Desktop.Pages;

public partial class AdaptationPage : ContentPage
{
    public AdaptationPage(AdaptationPageViewModel model)
    {
        InitializeComponent();
        BindingContext = model;

        PageHelper.HookLogBinding(model.LogLines, LogContainer, ScrollView);
    }
}