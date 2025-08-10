using BitFab.KW1281Test.Messengers.Records;
using System.Drawing;

namespace BitFab.KW1281Test.Messengers;

public sealed class Messenger
{
    private static readonly Messenger _instance = new ();
    public event Action<TextLine>? MessageReceived;

    private Messenger() { }

    public static Messenger Instance => _instance;

    public void Add(string message)
    {
        Add(message, Color.Black);
    }

    public void Add(string message, Color color)
    {
        MessageReceived?.Invoke(new() { Text = message, TextColor = color });
    }

    public void AddLine()
    {
        AddLine(string.Empty);
    }

    public void AddLine(string message)
    {
        AddLine(message, Color.Black);
    }

    public void AddLine(string message, Color color)
    {
        Add(message + Environment.NewLine, color);
    }
}
