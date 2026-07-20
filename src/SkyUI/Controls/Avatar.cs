using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SkyUI.Controls;

public class Avatar : TemplatedControl
{
    public const string RootPartName = "PART_Root";
    public const string ImagePartName = "PART_Image";
    public const string InitialsPartName = "PART_Initials";

    public static readonly StyledProperty<IImage?> SourceProperty =
        AvaloniaProperty.Register<Avatar, IImage?>(nameof(Source));

    public static readonly StyledProperty<string?> InitialsProperty =
        AvaloniaProperty.Register<Avatar, string?>(nameof(Initials));

    /// <summary>Used to derive initials when <see cref="Initials"/> is not set (e.g. &quot;Jane Doe&quot; → &quot;JD&quot;).</summary>
    public static readonly StyledProperty<string?> DisplayNameProperty =
        AvaloniaProperty.Register<Avatar, string?>(nameof(DisplayName));

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<Avatar, double>(nameof(Size), 40d, coerce: CoerceSize);

    public static readonly StyledProperty<bool> IsCircularProperty =
        AvaloniaProperty.Register<Avatar, bool>(nameof(IsCircular), true);

    private Border? _root;
    private Image? _image;
    private TextBlock? _initialsBlock;

    static Avatar()
    {
        ClipToBoundsProperty.OverrideDefaultValue<Avatar>(false);
        SourceProperty.Changed.AddClassHandler<Avatar>((a, _) => a.UpdateVisuals());
        InitialsProperty.Changed.AddClassHandler<Avatar>((a, _) => a.UpdateVisuals());
        DisplayNameProperty.Changed.AddClassHandler<Avatar>((a, _) => a.UpdateVisuals());
        SizeProperty.Changed.AddClassHandler<Avatar>((a, _) => a.ApplySizeAndShape());
        IsCircularProperty.Changed.AddClassHandler<Avatar>((a, _) => a.ApplySizeAndShape());
        CornerRadiusProperty.Changed.AddClassHandler<Avatar>((a, _) => a.ApplySizeAndShape());
    }

    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string? Initials
    {
        get => GetValue(InitialsProperty);
        set => SetValue(InitialsProperty, value);
    }

    public string? DisplayName
    {
        get => GetValue(DisplayNameProperty);
        set => SetValue(DisplayNameProperty, value);
    }

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public bool IsCircular
    {
        get => GetValue(IsCircularProperty);
        set => SetValue(IsCircularProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _root = e.NameScope.Find(RootPartName) as Border;
        _image = e.NameScope.Find(ImagePartName) as Image;
        _initialsBlock = e.NameScope.Find(InitialsPartName) as TextBlock;

        ApplySizeAndShape();
        UpdateVisuals();
    }

    private static double CoerceSize(AvaloniaObject _, double value) => Math.Clamp(value, 8, 512);

    private void ApplySizeAndShape()
    {
        var s = Size;
        Width = s;
        Height = s;

        if (_root is null)
            return;

        if (IsCircular)
        {
            var r = s * 0.5;
            _root.CornerRadius = new CornerRadius(r);
        }
        else
        {
            _root.CornerRadius = CornerRadius;
        }

        if (_initialsBlock is not null)
            _initialsBlock.FontSize = Math.Max(10, s * 0.42);
    }

    private void UpdateVisuals()
    {
        var hasImage = Source is not null;

        if (_image is not null)
        {
            _image.Source = Source;
            _image.IsVisible = hasImage;
        }

        if (_initialsBlock is not null)
        {
            _initialsBlock.IsVisible = !hasImage;
            _initialsBlock.Text = ResolveInitialsText();
        }
    }

    private string ResolveInitialsText()
    {
        if (!string.IsNullOrWhiteSpace(Initials))
            return Initials.Trim().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(DisplayName))
            return GenerateInitials(DisplayName);

        return "?";
    }

    internal static string GenerateInitials(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0)
            return "?";

        var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            var a = parts[0];
            var b = parts[^1];
            return $"{char.ToUpperInvariant(a[0])}{char.ToUpperInvariant(b[0])}";
        }

        var s = parts[0];
        if (s.Length >= 2)
            return s[..2].ToUpperInvariant();

        return char.ToUpperInvariant(s[0]).ToString();
    }
}
