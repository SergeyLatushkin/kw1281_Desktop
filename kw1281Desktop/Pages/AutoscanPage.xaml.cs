using kw1281Desktop.Helpers;

namespace kw1281Desktop.Pages;

public partial class AutoscanPage : ContentPage
{
    public AutoscanPage(AutoscanPageViewModel model)
    {
        InitializeComponent();
        BindingContext = model;

        PageHelper.HookLogBinding(model.LogLines, LogContainer, ScrollView);
    }
}