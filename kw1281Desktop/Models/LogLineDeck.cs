using kw1281Desktop.Models.Base;

namespace kw1281Desktop.Models;

public class LogLineDeck : BasePropertyChanged
{
    private string? _text;
    private Color? _textColor;

    public string? Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public Color? TextColor
    {
        get => _textColor;
        set => SetProperty(ref _textColor, value);
    }
}
