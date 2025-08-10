using kw1281Desktop.Models.Base;

namespace kw1281Desktop.Models;

public class CustomTuple : BasePropertyChanged
{
    private string _value1;
    public string Value1
    {
        get => _value1;
        set => SetProperty(ref _value1, value);
    }

    private string _value2;
    public string Value2
    {
        get => _value2;
        set => SetProperty(ref _value2, value);
    }
}
