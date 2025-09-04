using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using kw1281Desktop.Models;
using System.Windows.Input;
using kw1281Desktop.PageModels.BasePageViewModels;

namespace kw1281Desktop.PageModels;

public sealed class ReadFaultCodesPageViewModel : BaseScanViewPageModel
{
    public ReadFaultCodesPageViewModel(Diagnostic diagnostic, ILoaderService loader)
        : base(diagnostic, loader)
    {
    }

    private ElementItem<int> _selectedAddress;
    public ElementItem<int> SelectedAddress
    {
        get => _selectedAddress ?? Addresses.First();
        set => SetProperty(ref _selectedAddress, value);
    }

    public ICommand ReadClearCommand => new Command(async command =>
    {
        DataSender.Instance.DataReceived += OnResultReceived;

        await ExecuteReadInBackgroundWithLoader(SelectedAddress.Value, Enum.Parse<Commands>(command.ToString()!));

        DataSender.Instance.DataReceived -= OnResultReceived;
    });

    public ICommand GoToGroupCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync($"{nameof(GroupReadPage)}?address={SelectedAddress.Value}");
    });
}