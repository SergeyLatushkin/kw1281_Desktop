using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using kw1281Desktop.Models;
using kw1281Desktop.PageModels.BasePageViewModels;
using System.Collections.ObjectModel;

namespace kw1281Desktop.PageModels;

[QueryProperty(nameof(Address), "address")]
public sealed class GroupReadPageViewModel : BaseScanViewPageModel
{
    private readonly Diagnostic _diagnostic;
    private readonly ILoaderService _loader;
    private Action<IBaseResult>? handler;

    public GroupReadPageViewModel(Diagnostic diagnostic, ILoaderService loader)
        : base(diagnostic, loader)
    {
        for (int i = 1; i < 6; i++)
        {
            var row = new GroupRowModel
            {
                Input = i.ToString()
            };
            row.StartCommand = new Command(() => ExecuteStart(row));
            row.CancelCommand = new Command(() => ExecuteCancel(row));
            Rows.Add(row);
        }

        _diagnostic = diagnostic;
        _loader = loader;
    }

    public ObservableCollection<GroupRowModel> Rows { get; } = [];

    private string _address;
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    private bool _isBasicSetting;
    public bool IsBasicSetting
    {
        get => _isBasicSetting;
        set => SetProperty(ref _isBasicSetting, value);
    }

    private void ExecuteCancel(GroupRowModel row)
    {
        _diagnostic.Cts.Cancel();
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
                handler = (result) =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (result.Ok && result is Result<List<KeyValuePair<byte, string>>> group)
                        {
                            row.Fields.Clear();

                            int groupsCount = group.Data.Count();

                            for (int i = 0; i < (groupsCount < 4 ? 4 : groupsCount); i++)
                            {
                                if (i < groupsCount)
                                {
                                    row.Fields.Add(new FieldItem { Key = group.Data[i].Key, Value = group.Data[i].Value });
                                }
                                else
                                {
                                    row.Fields.Add(new FieldItem { Key = 0, Value = "Not available" });
                                }
                            }
                        }
                    });
                };

                DataSender.Instance.DataReceived += handler;

                await _diagnostic.Run(
                    AppSettings.Port!,
                    AppSettings.Baud,
                    Address,
                    !IsBasicSetting ? Commands.GroupRead : Commands.BasicSetting,
                    row.Input);
            });

        }
        finally
        {
            await _loader.HideAsync();
            DataSender.Instance.DataReceived -= handler;
        }
    }
}
