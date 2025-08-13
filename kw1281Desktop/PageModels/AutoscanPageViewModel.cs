using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using System.Windows.Input;
using kw1281Desktop.PageModels.BasePageViewModels;

namespace kw1281Desktop.PageModels;

public sealed class AutoscanPageViewModel : BaseScanViewPageModel
{
    public AutoscanPageViewModel(Diagnostic diagnostic, IErrorHandler errorHandler, ILoaderService loader)
        : base(diagnostic, errorHandler, loader)
    {
    }

    public ICommand ReadCommand => new Command(async () =>
    {
        DataSender.Instance.DataReceived += OnResultReceived;

        await ExecuteReadInBackgroundWithLogDescription(
            null!,
            Commands.AutoScan,
            args: Addresses.Select(address => address.Value).ToArray());

        DataSender.Instance.DataReceived -= OnResultReceived;
    });
}