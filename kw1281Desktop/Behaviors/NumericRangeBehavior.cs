namespace kw1281Desktop.Behaviors;

public class NumericRangeBehavior : Behavior<Entry>
{
    public static readonly BindableProperty MinProperty =
        BindableProperty.Create(nameof(Min), typeof(int), typeof(NumericRangeBehavior), 0);

    public static readonly BindableProperty MaxProperty =
        BindableProperty.Create(nameof(Max), typeof(int), typeof(NumericRangeBehavior), int.MaxValue);

    public int Min
    {
        get => (int)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public int Max
    {
        get => (int)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    protected override void OnAttachedTo(Entry entry)
    {
        entry.TextChanged += OnTextChanged!;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnTextChanged!;
        base.OnDetachingFrom(entry);
    }

    void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;

        if (string.IsNullOrEmpty(e.NewTextValue))
        {
            return;
        }

        string text = e.NewTextValue;

        bool isS = text.StartsWith("$");

        if (isS)
        {
            text = text.Replace("$", string.Empty);

            if (string.IsNullOrEmpty(text))
            {
                return;
            }
        }

        int maxLength = Max.ToString().Length;
        if (text.Length > maxLength)
        {
            entry.Text = e.OldTextValue;
            return;
        }

        if (int.TryParse(text, out int value) && value >= Min && value <= Max)
        {
            return;
        }

        entry.Text = e.OldTextValue;
    }
}
