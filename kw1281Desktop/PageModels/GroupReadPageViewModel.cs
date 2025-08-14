using BitFab.KW1281Test;
using BitFab.KW1281Test.Actions;
using BitFab.KW1281Test.Actions.Records;
using BitFab.KW1281Test.Enums;
using kw1281Desktop.Converters;
using kw1281Desktop.Models;
using kw1281Desktop.Models.Base;
using System.Collections.ObjectModel;

namespace kw1281Desktop.PageModels;

[QueryProperty(nameof(Address), "address")]
public sealed class GroupReadPageViewModel : BasePropertyChanged
{
    private readonly Diagnostic _diagnostic;
    private Action<IBaseResult>? handler;
    private event EventHandler? ScrollToLastRequested;
    private readonly ILoaderService _loader;

    public GroupReadPageViewModel(Diagnostic diagnostic, IErrorHandler errorHandler, ILoaderService loader)
    {
        for (int i = 1; i < (AppSettings.Logging ? 4 : 6); i++)
        {
            var row = new GroupRowModel { Input = i.ToString() };
            row.StartCommand = new Command(() => ExecuteStart(row));
            row.CancelCommand = new Command(() => ExecuteCancel(row));
            Rows.Add(row);
        }

        _diagnostic = diagnostic;
        _loader = loader;
    }

    public ObservableCollection<GroupRowModel> Rows { get; } = new();

    private string _address;
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public ObservableCollection<LogLineDeck> LogLines { get; } = new();

    private bool _isBasicSetting;
    public bool IsBasicSetting
    {
        get => _isBasicSetting;
        set => SetProperty(ref _isBasicSetting, value);
    }

    public bool ShowLogs => AppSettings.Logging;

    private void ExecuteCancel(GroupRowModel row)
    {
        _diagnostic.Cts.Cancel();

        row.IsRunning = false;
    }

    private async void ExecuteStart(GroupRowModel row)
    {
        if (string.IsNullOrWhiteSpace(row.Input))
        {
            return;
        }

        _loader.ShowAsync();

        try
        {
            await Task.Run(async () =>
            {
                if (row.Input.Equals("0"))
                {
                    row.IsRunning = true;
                }

                handler = (group) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        row.Fields.Clear();

                        foreach (var kv in ((Result<IEnumerable<KeyValuePair<byte, string>>>)group).Data)
                        {
                            row.Fields.Add(new FieldItem { Key = kv.Key, Value = kv.Value });
                        }

                        DataSender.Instance.DataReceived -= handler;
                    });
                };

                DataSender.Instance.DataReceived += handler;

                if (AppSettings.Logging)
                {
                    Messenger.Instance.MessageReceived += OnGrupMessageReceived;
                }

                await _diagnostic.Run(
                    AppSettings.Port!,
                    AppSettings.Baud,
                    Address,
                    !IsBasicSetting ? Commands.GroupRead : Commands.BasicSetting,
                    row.Input);

                if (AppSettings.Logging)
                {
                    Messenger.Instance.MessageReceived -= OnGrupMessageReceived;
                }
            });

        }
        finally
        {
            await _loader.HideAsync();
        }
    }

    private void OnGrupMessageReceived(TextLine message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LogLines.Add(new()
            {
                Text = message.Text,
                TextColor = message.TextColor.ToMauiColor()
            });
            ScrollToLastRequested?.Invoke(this, EventArgs.Empty);
        });
    }
}
