using kw1281Desktop.Helpers;

namespace kw1281Desktop.Pages;

public partial class DumpPage : ContentPage
{
    public DumpPage(DumpPageViewModel model)
    {
        InitializeComponent();
        BindingContext = model;

        PageHelper.HookLogBinding(model.LogLines, LogContainer, ScrollView);
    }
}