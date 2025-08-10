namespace kw1281Desktop.Models;

public class ElementItem<T>
{
    public ElementItem(T value, string name)
    {
        Value = value;
        Name = name;
    }

    public T Value { get; }
    public string Name { get; }
}
