using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using kw1281Desktop.Extantions;
using kw1281Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace kw1281Desktop.PageModels;

public sealed class UtilsPageViewModel : BaseScanViewPageModel
{
    public ObservableCollection<ObservableAddressValuePair> AddressValuePairs { get; } = new();

    public UtilsPageViewModel(Diagnostic diagnostic, IErrorHandler errorHandler)
        : base(diagnostic, errorHandler)
    {
        AddressValuePairs.Add(new ObservableAddressValuePair { IsVisible = true });
    }

    private Commands _selectedCommand;
    public Commands SelectedCommand
    {
        get => _selectedCommand;
        set => SetProperty(ref _selectedCommand, value);
    }

    private ElementItem<string> _selectedAddress;
    public ElementItem<string> SelectedAddress
    {
        get => _selectedAddress ?? Addresses.First();
        set => SetProperty(ref _selectedAddress, value);
    }

    public CustomTuple CodeWorkshop{ get; } = new();

    private string _login;
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public CustomTuple AddressValue { get; } = new();

    public ICommand RunCommand => new Command(async () =>
    {
        string[] args = null!;

        switch (SelectedCommand)
        {
            case Commands.SetSoftwareCoding:
                args = [CodeWorkshop.Value1, CodeWorkshop.Value2];
                break;
            case Commands.WriteEeprom:
                args = [AddressValue.Value1, AddressValue.Value2];
                break;
            case Commands.FindLogins:
                args = [Login];
                break;
            case Commands.WriteEdc15Eeprom:
                args = AddressValuePairs.FlattenPairs();
                break;
        }

        DataSender.Instance.DataReceived += OnResultReceived;

        await ExecuteReadInBackgroundWithLogDescription(SelectedAddress.Value, SelectedCommand, args:args);

        DataSender.Instance.DataReceived -= OnResultReceived;
    });
}