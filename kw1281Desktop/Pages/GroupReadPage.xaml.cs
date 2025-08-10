using kw1281Desktop.Helpers;

namespace kw1281Desktop.Pages;

public partial class GroupReadPage : ContentPage
{
    public GroupReadPage(GroupReadPageViewModel model)
    {
        InitializeComponent();
        BindingContext = model;

        PageHelper.HookLogBinding(model.LogLines, LogContainer, ScrollView);
    }
}
