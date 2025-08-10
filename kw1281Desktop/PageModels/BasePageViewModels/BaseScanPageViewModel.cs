using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using BitFab.KW1281Test.Messengers;
using BitFab.KW1281Test.Messengers.Records;
using kw1281Desktop.Converters;
using kw1281Desktop.Models;
using kw1281Desktop.Models.Base;
using Microsoft.VisualBasic.Logging;
using System.Collections.ObjectModel;

namespace kw1281Desktop.PageModels;

public abstract class BaseScanViewPageModel : BasePropertyChanged
{
    protected Diagnostic Diagnostic { get; }

    private event EventHandler? ScrollToLastRequested;

    protected IErrorHandler ErrorHandler { get; }

    protected BaseScanViewPageModel(Diagnostic diagnostic, IErrorHandler errorHandler)
    {
        ErrorHandler = errorHandler;
        Diagnostic = diagnostic;
    }

    public ObservableCollection<LogLineDeck> LogLines { get; } = new();

    public ObservableCollection<ElementItem<string>> Addresses { get; } =
    new()
    {
        new ( "1", "01 - Engine" ),
        new ( "3", "03 - ABS Brakes" ),
        new ( "8", "08 - Auto HVAC" ),
        new ( "17", "17 - Instruments" ),
        new ( "15", "15 - Airbags" ),
        new ( "19", "19 - CAN Gateway" ),
        new ( "25", "25 - Immobilizer" ),
        new ( "37", "37 - Navigation" ),
        new ( "46", "46 - Cent. Conv." ),
        new ( "47", "47 - Sound System" ),
        new ( "56", "56 - Radio" ),
    };

    protected async Task ExecuteReadInBackgroundWithLogDescription(string controllerAddress,
        Commands command, bool forceLogsOn = false, params string[] args)
    {
        await Task.Run(() =>
        {
            if (!forceLogsOn && !AppSettings.Logging)
            {
                Diagnostic.Run(AppSettings.Port!, AppSettings.Baud, controllerAddress, command, args);

                return;
            }

            Messenger.Instance.MessageReceived += OnLogReceived;

            Diagnostic.Run(AppSettings.Port!, AppSettings.Baud, controllerAddress, command, args);

            Messenger.Instance.MessageReceived -= OnLogReceived;
        });
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