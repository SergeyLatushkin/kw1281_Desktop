using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using CommunityToolkit.Maui.Extensions;
using kw1281Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using kw1281Desktop.Extensions;
using kw1281Desktop.PageModels.BasePageViewModels;

namespace kw1281Desktop.PageModels;

public sealed class UtilsPageViewModel : BaseScanViewPageModel
{
    public ObservableCollection<ObservableAddressValuePair> AddressValuePairs { get; } = [];
    readonly ActuatorDialogPage _popup = new() { CanBeDismissedByTappingOutsideOfPopup = false };

    public UtilsPageViewModel(Diagnostic diagnostic, ILoaderService loader)
        : base(diagnostic, loader)
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

        await ExecuteReadInBackgroundWithLoader(SelectedAddress.Value, SelectedCommand, args:args);

        DataSender.Instance.DataReceived -= OnResultReceived;
    });

    public async Task RunActuatorLoopAsync()
    {
        _popup.Input = "Test is started...";
        _popup.NextClicked += Diagnostic.Control.RequestNext;
        _popup.CancelClicked += Diagnostic.Control.RequestStop;

        var popupTask = Shell.Current.ShowPopupAsync(_popup);

        DataSender.Instance.DataReceived += OnPopupResultReceived;

        await ExecuteReadInBackground(SelectedAddress.Value, SelectedCommand);

        DataSender.Instance.DataReceived -= OnPopupResultReceived;

        await popupTask;

        _popup.NextClicked -= Diagnostic.Control.RequestNext;
        _popup.CancelClicked -= Diagnostic.Control.RequestStop;
    }

    private void OnPopupResultReceived(IBaseResult baseResult)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (baseResult.Ok)
            {
                _popup.Input = baseResult.Content;
            }
            else if (!baseResult.Ok && baseResult is Result<Exception> ex)
            {
                _popup.Input = $"Error : {ex.Error.Message}";
            }
            else
            {
                _popup.Input = "Data result error.";
            }
        });
    }
}