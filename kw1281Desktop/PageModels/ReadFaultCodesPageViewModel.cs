using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using kw1281Desktop.Models;
using System.Windows.Input;
using kw1281Desktop.PageModels.BasePageViewModels;

namespace kw1281Desktop.PageModels;

public sealed class ReadFaultCodesPageViewModel : BaseScanViewPageModel
{
    public ReadFaultCodesPageViewModel(Diagnostic diagnostic, IErrorHandler errorHandler, ILoaderService loader)
        : base(diagnostic, errorHandler, loader)
    {
    }

    private ElementItem<string> _selectedAddress;
    public ElementItem<string> SelectedAddress
    {
        get => _selectedAddress ?? Addresses.First();
        set => SetProperty(ref _selectedAddress, value);
    }

    public ICommand ReadClearCommand => new Command(async command =>
    {
        DataSender.Instance.DataReceived += OnResultReceived;

        await ExecuteReadInBackgroundWithLogDescription(
            SelectedAddress.Value,
            Enum.Parse<Commands>(command.ToString()!));

        DataSender.Instance.DataReceived -= OnResultReceived;
    });

    public ICommand GoToGroupCommand => new Command(async () =>
    {
        await Shell.Current.GoToAsync($"{nameof(GroupReadPage)}?address={SelectedAddress.Value}");
    });
}