namespace UOMacroMobile.Controls;

public partial class StatusIndicator : ContentView
{
    public static readonly BindableProperty StatusTextProperty =
        BindableProperty.Create(nameof(StatusText), typeof(string), typeof(StatusIndicator), "Unknown");

    public static readonly BindableProperty StatusColorProperty =
        BindableProperty.Create(nameof(StatusColor), typeof(Color), typeof(StatusIndicator), Colors.Gray);

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public Color StatusColor
    {
        get => (Color)GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    public StatusIndicator()
    {
        InitializeComponent();
    }
}