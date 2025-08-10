using kw1281Desktop.Models.Base;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace kw1281Desktop.Models;

public class GroupRowModel : BasePropertyChanged
{
    private string? _input;
    public string? Input
    {
        get => _input;
        set => SetProperty(ref _input, value);
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning == value)
            {
                return;
            }
            _isRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotRunning));
        }
    }

    public bool IsNotRunning => !IsRunning;

    public ObservableCollection<FieldItem> Fields { get; } = new()
    {
        new FieldItem { Key = 0, Value = string.Empty },
        new FieldItem { Key = 0, Value = string.Empty },
        new FieldItem { Key = 0, Value = string.Empty },
        new FieldItem { Key = 0, Value = string.Empty }
    };

    public ICommand? CancelCommand { get; set; }

    public ICommand? StartCommand { get; set; }
}

public class FieldItem
{
    public byte Key { get; set; }
    public string? Value { get; set; }
}
