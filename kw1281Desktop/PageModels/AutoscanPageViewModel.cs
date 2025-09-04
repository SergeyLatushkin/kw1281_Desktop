using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using System.Windows.Input;
using kw1281Desktop.PageModels.BasePageViewModels;
using BitFab.KW1281Test.Models;

namespace kw1281Desktop.PageModels;

public sealed class AutoscanPageViewModel : BaseScanViewPageModel
{
    public AutoscanPageViewModel(Diagnostic diagnostic, ILoaderService loader)
        : base(diagnostic, loader)
    {
    }

    public ICommand ReadCommand => new Command(async () =>
    {
        DataSender.Instance.DataReceived += OnResultReceived;

        await ExecuteReadInBackgroundWithLoader(
            0,
            Commands.AutoScan,
            args: [.. Addresses.Select(address => (Arg)address.Value)]);

        DataSender.Instance.DataReceived -= OnResultReceived;
    });
}