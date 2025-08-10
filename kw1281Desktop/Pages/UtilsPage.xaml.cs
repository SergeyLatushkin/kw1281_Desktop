using kw1281Desktop.Helpers;
using kw1281Desktop.Models;

namespace kw1281Desktop.Pages;

public partial class UtilsPage : ContentPage
{
    public UtilsPage(UtilsPageViewModel model)
    {
        InitializeComponent();
        BindingContext = model;

        PageHelper.HookLogBinding(model.LogLines, LogContainer, ScrollView);
    }

    private void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton button)
        {
            return;
        }

        if (button.BindingContext is not ObservableAddressValuePair item)
        {
            return;
        }

        var vm = BindingContext as UtilsPageViewModel;
        if (vm == null)
        {
            return;
        }

        if (vm.AddressValuePairs.Count > 1)
        {
            vm.AddressValuePairs.Remove(item);
            UpdatePairsVisibility(vm);
        }
    }

    private void OnAddClicked(object sender, EventArgs e)
    {
        if (BindingContext is not UtilsPageViewModel vm)
            return;

        vm.AddressValuePairs.Add(new ObservableAddressValuePair());
        UpdatePairsVisibility(vm);
    }

    private void UpdatePairsVisibility(UtilsPageViewModel vm)
    {
        for (int i = 0; i < vm.AddressValuePairs.Count; i++)
        {
            vm.AddressValuePairs[i].IsVisible = i == vm.AddressValuePairs.Count - 1;
            vm.AddressValuePairs[i].IsNotVisible = i != vm.AddressValuePairs.Count - 1;
        }
    }
}