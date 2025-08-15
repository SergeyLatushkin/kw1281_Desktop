using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using System.Windows.Input;
using kw1281Desktop.PageModels.BasePageViewModels;

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
            null!,
            Commands.AutoScan,
            args: Addresses.Select(address => address.Value).ToArray());

        DataSender.Instance.DataReceived -= OnResultReceived;
    });
}