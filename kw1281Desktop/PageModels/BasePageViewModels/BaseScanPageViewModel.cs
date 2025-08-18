using System.Collections.ObjectModel;
using BitFab.KW1281Test;
using BitFab.KW1281Test.Actions;
using BitFab.KW1281Test.Actions.Records;
using BitFab.KW1281Test.Enums;
using kw1281Desktop.Converters;
using kw1281Desktop.Models;
using kw1281Desktop.Models.Base;

namespace kw1281Desktop.PageModels.BasePageViewModels;

public abstract class BaseScanViewPageModel : BasePropertyChanged
{
    private event EventHandler? ScrollToLastRequested;
    private readonly ILoaderService _loader;

    protected Diagnostic Diagnostic { get; }

    protected BaseScanViewPageModel(Diagnostic diagnostic, ILoaderService loader)
    {
        var route = Shell.Current.CurrentState.Location.ToString();
        AppSettingsStorage.Save("page", route);

        Diagnostic = diagnostic;
        _loader = loader;
    }

    public ObservableCollection<LogLineDeck> LogLines { get; } = [];

    public ObservableCollection<ElementItem<string>> Addresses { get; } =
    [
        new ( "1", "01 - Engine" ),
        new ( "3", "03 - ABS Brakes" ),
        new ( "8", "08 - Auto HVAC" ),
        new ( "15", "15 - Airbags" ),
        new ( "17", "17 - Instruments" ),
        new ( "19", "19 - CAN Gateway" ),
        new ( "25", "25 - Immobilizer" ),
        new ( "37", "37 - Navigation" ),
        new ( "46", "46 - Cent. Conv." ),
        new ( "47", "47 - Sound System" ),
        new ( "56", "56 - Radio" ),
    ];

    protected virtual async Task ExecuteReadInBackground(string controllerAddress, Commands command,
        bool forceLogsOn = false, params string[] args)
    {
        await Task.Run(async () =>
        {
            if (!forceLogsOn && !AppSettings.Logging)
            {
                await Diagnostic.Run(AppSettings.Port!, AppSettings.Baud, controllerAddress, command, args);

                return;
            }

            try
            {
                Messenger.Instance.MessageReceived += OnLogReceived;

                await Diagnostic.Run(AppSettings.Port!, AppSettings.Baud, controllerAddress, command, args);
            }
            finally
            {
                Messenger.Instance.MessageReceived -= OnLogReceived;
            }
        });
    }

    protected async Task ExecuteReadInBackgroundWithLoader(string controllerAddress, Commands command,
        bool forceLogsOn = false, params string[] args)
    {
        _loader.ShowAsync();

        try
        {
            await ExecuteReadInBackground(controllerAddress, command, forceLogsOn, args);
        }
        finally
        {
            await _loader.HideAsync();
        }
    }

    protected void OnResultReceived(IBaseResult baseResult)
    {
        LogLineDeck logLineDeck = new();

        if (baseResult.Ok)
        {
            logLineDeck.Text = baseResult.Content;
            logLineDeck.TextColor = Colors.Blue;
        }
        else if (!baseResult.Ok && baseResult is Result<Exception> ex)
        {
            logLineDeck.Text = $"{ex.Error.Message}.";
            logLineDeck.TextColor = Colors.Red;
        }
        else
        {
            logLineDeck.Text = "Data result error.";
            logLineDeck.TextColor = Colors.Red;
        }

        OnMessageReceived(logLineDeck);
    }

    private void OnLogReceived(TextLine message)
    {
        OnMessageReceived(new LogLineDeck()
        {
            Text = message.Text,
            TextColor = message.TextColor.ToMauiColor()
        });
    }

    private void OnMessageReceived(LogLineDeck message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LogLines.Add(message);
            ScrollToLastRequested?.Invoke(this, EventArgs.Empty);
        });
    }
}