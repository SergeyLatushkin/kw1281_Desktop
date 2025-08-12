namespace kw1281Desktop;

public static class AppSettings
{
    private const string baudKey = "baud";
    public const int BaudDefault = 9600;
    public static int Baud
    {
        get => Preferences.Get(baudKey, BaudDefault);
        set => Preferences.Set(baudKey, value);
    }

    public const string port = "port";
    public const string PortDefault = "COM1";
    public static string? Port
    {
        get => Preferences.Get(port, PortDefault);
        set => Preferences.Set(port, value);
    }

    public const string logging = "logging";
    public static bool Logging
    {
        get => Preferences.Get(logging, true);
        set => Preferences.Set(logging, value);
    }

    private const string page = "page";
    public const string PageDefault = "//appSettings";
    public static string Page
    {
        get => Preferences.Get(page, PageDefault);
        set => Preferences.Set(page, value);
    }

    public static void Clear() => Preferences.Clear();
}