using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using CommunityToolkit.Maui.Extensions;
using kw1281Desktop.Extantions;
using kw1281Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace kw1281Desktop.PageModels;

public sealed class UtilsPageViewModel : BaseScanViewPageModel
{
    public ObservableCollection<ObservableAddressValuePair> AddressValuePairs { get; } = new();
    ActuatorDialogPage popup = new();

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
            case Commands.ActuatorTest:
                await RunActuatorLoopAsync();
                return;
        }

        DataSender.Instance.DataReceived += OnResultReceived;

        await ExecuteReadInBackgroundWithLogDescription(SelectedAddress.Value, SelectedCommand, args:args);

        DataSender.Instance.DataReceived -= OnResultReceived;
    });

    public async Task RunActuatorLoopAsync()
    {
        popup.NextClicked += Diagnostic.Control.RequestNext;
        popup.CancelClicked += Diagnostic.Control.RequestStop;

        var popupTask = Shell.Current.ShowPopupAsync(popup);

        DataSender.Instance.DataReceived += OnPopupResultReceived;

        await ExecuteReadInBackgroundWithLogDescription(SelectedAddress.Value, SelectedCommand);

        DataSender.Instance.DataReceived -= OnPopupResultReceived;

        await popupTask;

        popup.NextClicked -= Diagnostic.Control.RequestNext;
        popup.CancelClicked -= Diagnostic.Control.RequestStop;
    }

    private void OnPopupResultReceived(IBaseResult baseResult)
    {
        popup.Input = baseResult.Content;
    }
}