namespace BitFab.KW1281Test.Models;

public readonly struct Args
{
    private readonly int _intValue;
    private readonly string _stringValue;
    private readonly bool _isInt;

    private Args(int value)
    {
        _intValue = value;
        _stringValue = null!;
        _isInt = true;
    }

    private Args(string value)
    {
        _stringValue = value;
        _intValue = 0;
        _isInt = false;
    }

    public static implicit operator Args(int value) => new(value);
    public static implicit operator Args(string value) => new(value);

    public static implicit operator int(Args arg) => arg._isInt ? arg._intValue : throw new InvalidCastException();
    public static implicit operator string(Args arg) => !arg._isInt ? arg._stringValue : throw new InvalidCastException();

    public T Get<T>() => typeof(T) == typeof(int) && _isInt
        ? (T)(object)_intValue
        : typeof(T) == typeof(string) && !_isInt
            ? (T)(object)_stringValue
            : throw new InvalidCastException();

    public override string ToString() => _isInt ? _intValue.ToString() : _stringValue;
}

