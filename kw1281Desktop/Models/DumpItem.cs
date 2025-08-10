using BitFab.KW1281Test.Enums;

namespace kw1281Desktop.Models;

public class DumpItem : ElementItem<Commands>
{
    public DumpItem(
        Commands number,
        string displayName,
        (string, bool) startField,
        (string, bool) lenghtField)
        : this(number, displayName)
    {
        Start = startField;
        Lenght = lenghtField;
    }

    public DumpItem(Commands number, string displayName)
        : base(number, displayName)
    {
    }

    public (string, bool) Start { get; }
    public (string, bool) Lenght { get; }
}
