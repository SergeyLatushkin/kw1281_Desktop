using CommunityToolkit.Mvvm.ComponentModel;

namespace kw1281Desktop.Models;
public class ObservableAddressValuePair : ObservableObject
{
    private string _address;
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    private string _value;
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    private bool _isVisible;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private bool _isNotVisible;
    public bool IsNotVisible
    {
        get => _isNotVisible;
        set => SetProperty(ref _isNotVisible, value);
    }
}
