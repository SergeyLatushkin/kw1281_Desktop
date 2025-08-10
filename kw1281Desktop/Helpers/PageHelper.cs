using kw1281Desktop.Models;
using System.Collections.ObjectModel;

namespace kw1281Desktop.Helpers;

internal static class PageHelper
{
    internal static void HookLogBinding(
        ObservableCollection<LogLineDeck> source,
        VerticalStackLayout container,
        ScrollView scrollView)
    {
        source.CollectionChanged += async (_, e) =>
        {
            if (e.NewItems == null) return;

            foreach (LogLineDeck line in e.NewItems)
            {
                var label = new Label
                {
                    Text = line.Text,
                    TextColor = line.TextColor,
                    FontSize = 14
                };
                container.Children.Add(label);
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Task.Delay(30);
                if (container.Children.LastOrDefault() is View lastView)
                {
                    await scrollView.ScrollToAsync(lastView, ScrollToPosition.End, false);
                }
            });
        };
    }
}
