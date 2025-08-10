namespace kw1281Desktop.Converters;

internal static class ColorConverter
{
    public static Color ToMauiColor(this System.Drawing.Color color)
    {
        return new Color(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }
}
