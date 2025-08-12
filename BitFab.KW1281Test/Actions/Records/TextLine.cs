using System.Drawing;

namespace BitFab.KW1281Test.Actions.Records
{
    public record TextLine
    {
        public string? Text { get; internal set; }
        public Color TextColor { get; internal set; }
    }
}
