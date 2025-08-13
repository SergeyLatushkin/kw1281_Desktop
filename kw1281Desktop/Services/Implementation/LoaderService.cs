using CommunityToolkit.Maui.Extensions;

namespace kw1281Desktop.Services.Implementation;

public class LoaderService : ILoaderService
{
    private readonly Loader _loader;

    public LoaderService()
    {
        _loader = new Loader { CanBeDismissedByTappingOutsideOfPopup = false };
    }

    public async void ShowAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.ShowPopupAsync(_loader);
        });
    }

    public async Task HideAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await _loader.CloseAsync();
        });
    }
}
