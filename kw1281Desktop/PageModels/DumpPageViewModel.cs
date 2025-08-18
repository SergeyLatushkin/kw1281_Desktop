using BitFab.KW1281Test;
using BitFab.KW1281Test.Enums;
using kw1281Desktop.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using kw1281Desktop.PageModels.BasePageViewModels;
using WindowsAPICodePack.Dialogs;

namespace kw1281Desktop.PageModels;

public sealed class DumpPageViewModel : BaseScanViewPageModel
{
    public DumpPageViewModel(Diagnostic diagnostic, ILoaderService loader)
        : base(diagnostic, loader)
    {
    }

    private ElementItem<string> _selectedAddress;
    public ElementItem<string> SelectedAddress
    {
        get => _selectedAddress ?? Addresses.First();
        set => SetProperty(ref _selectedAddress, value);
    }

    public ObservableCollection<DumpItem> DumpCommands { get; } =
    [
        new(Commands.DumpEeprom, "Dump Eeprom", new ("0", true), new ("2048", true)),
        new(Commands.DumpEdc15Eeprom, "Dump Edc15Eeprom"),
        new(Commands.DumpMarelliMem, "Dump Marelli Mem", new ("3072", true), new ("1024", true)),
        new(Commands.DumpMem, "Dump Mem", new ("8192", true), new ("65536", true)),
        new(Commands.DumpRam, "Dump Ram", new ("8192", true), new ("65536", true)),
        new(Commands.DumpRom, "Dump Rom", new ("8192", true), new ("65536", true)),
        new(Commands.DumpRBxMem, "Dump RBxMem", new ("66560", true), new ("1024", true)),
        new(Commands.DumpRBxMemOdd, "Dump RBxMemOdd"),
        new(Commands.DumpCcmRom, "Dump CcmRom"),
        new(Commands.DumpClusterNecRom, "Dump RBxMemOdd"),
        new(Commands.ReadEeprom, "Read Eeprom", new ("4361", true), new (null!, false)),
        new(Commands.ReadRAM, "Read RAM", new ("4361", true), new (null!, false)),
        new(Commands.ReadROM, "Read ROM", new("4361", true), new(null!, false)),
        new(Commands.LoadEeprom, "Load Eeprom", new ("0", true), new(null!, false)),
        new(Commands.MapEeprom, "Map Eeprom")
    ];

    DumpItem _previousSelectedDump;

    private DumpItem _selectedDump;
    public DumpItem SelectedDump
    {
        get => _selectedDump ?? DumpCommands.First();
        set
        {
            SetProperty(ref _selectedDump, value);
            ChangePropertiesState(value);
        }
    }

    private string _filePath;
    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    private string _start;
    public string Start
    {
        get => _start;
        set => SetProperty(ref _start, value);
    }


    private string _length;
    public string Length
    {
        get => _length;
        set => SetProperty(ref _length, value);
    }

    private bool _isStartFieldEnabled;
    public bool IsStartFieldEnabled
    {
        get => _isStartFieldEnabled;
        set => SetProperty(ref _isStartFieldEnabled, value);
    }

    private bool _isLengthFieldEnabled;
    public bool IsLengthFieldEnabled
    {
        get => _isLengthFieldEnabled;
        set => SetProperty(ref _isLengthFieldEnabled, value);
    }

    public ICommand ReadCommand => new Command(async () =>
    {
        List<(string, bool)> parameters = [
             (Start, _selectedDump.Start.Item2),
            (Length, _selectedDump.Length.Item2),
            (FilePath, true)];

        string[] args = parameters.Where(arg => arg.Item2).Select(arg => arg.Item1).ToArray();

        DataSender.Instance.DataReceived += OnResultReceived;

        await ExecuteReadInBackgroundWithLoader(SelectedAddress.Value, SelectedDump.Value, args);

        DataSender.Instance.DataReceived -= OnResultReceived;
    });

    public ICommand ResetCommand => new Command(async () =>
    {
        await ExecuteReadInBackgroundWithLoader(SelectedAddress.Value, Commands.Reset);
    });

    public ICommand ChooseCommand => new Command(() =>
    {
        bool isFolderPicker = !SelectedDump.Value.Equals(Commands.LoadEeprom);

        using CommonOpenFileDialog dialog = new() { IsFolderPicker = isFolderPicker };

        if (!isFolderPicker)
        {
            dialog.Filters.Add(new("Dump", "*.bin"));
        }

        CommonFileDialogResult result = dialog.ShowDialog();
        if (result == CommonFileDialogResult.Ok)
        {
            FilePath = dialog.FileName;
        }
    });

    private void ChangePropertiesState(DumpItem value)
    {
        bool wasSpecial = _previousSelectedDump?.Value == Commands.LoadEeprom;
        bool nowSpecial = _selectedDump.Value == Commands.LoadEeprom;

        if (wasSpecial != nowSpecial)
        {
            FilePath = null!;
        }

        _previousSelectedDump = _selectedDump;

        IsLengthFieldEnabled = _selectedDump.Length.Item2;
        IsStartFieldEnabled = _selectedDump.Start.Item2;
        Start = value.Start.Item1!;
        Length = value.Length.Item1!;
    }
}