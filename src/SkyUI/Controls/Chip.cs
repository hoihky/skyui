using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

using SkyUI.Core.Theming;

namespace SkyUI.Controls;

public class Chip : TemplatedControl
{
    public const string RootPartName = "PART_Root";
    public const string AvatarHostPartName = "PART_AvatarHost";
    public const string LabelPartName = "PART_Label";
    public const string DeletePartName = "PART_Delete";

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<Chip, string?>(nameof(Label));

    /// <summary>Leading content, typically a <see cref="Avatar"/>.</summary>
    public static readonly StyledProperty<object?> AvatarContentProperty =
        AvaloniaProperty.Register<Chip, object?>(nameof(AvatarContent));

    public static readonly StyledProperty<bool> IsDeletableProperty =
        AvaloniaProperty.Register<Chip, bool>(nameof(IsDeletable));

    public static readonly StyledProperty<ChipVariant> VariantProperty =
        AvaloniaProperty.Register<Chip, ChipVariant>(nameof(Variant), ChipVariant.Filled);

    public static readonly StyledProperty<ChipSize> SizeProperty =
        AvaloniaProperty.Register<Chip, ChipSize>(nameof(Size), ChipSize.Medium);

    public static readonly StyledProperty<bool> IsClickableProperty =
        AvaloniaProperty.Register<Chip, bool>(nameof(IsClickable), false);

    public static readonly RoutedEvent<RoutedEventArgs> DeleteRequestedEvent =
        RoutedEvent.Register<Chip, RoutedEventArgs>(nameof(DeleteRequested), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> ChipClickEvent =
        RoutedEvent.Register<Chip, RoutedEventArgs>(nameof(ChipClick), RoutingStrategies.Bubble);

    private Border? _root;
    private ContentPresenter? _avatarHost;
    private TextBlock? _labelBlock;
    private Button? _deleteButton;

    static Chip()
    {
        ClipToBoundsProperty.OverrideDefaultValue<Chip>(false);

        LabelProperty.Changed.AddClassHandler<Chip>((c, _) => c.SyncLabel());
        AvatarContentProperty.Changed.AddClassHandler<Chip>((c, _) => c.SyncAvatarVisibility());
        IsDeletableProperty.Changed.AddClassHandler<Chip>((c, _) => c.SyncDeleteVisibility());
        VariantProperty.Changed.AddClassHandler<Chip>((c, _) => c.ApplyVariant());
        SizeProperty.Changed.AddClassHandler<Chip>((c, _) => c.ApplySize());
        IsClickableProperty.Changed.AddClassHandler<Chip>((c, _) => c.ApplyClickableCursor());
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object? AvatarContent
    {
        get => GetValue(AvatarContentProperty);
        set => SetValue(AvatarContentProperty, value);
    }

    public bool IsDeletable
    {
        get => GetValue(IsDeletableProperty);
        set => SetValue(IsDeletableProperty, value);
    }

    public ChipVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public ChipSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public bool IsClickable
    {
        get => GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? DeleteRequested
    {
        add => AddHandler(DeleteRequestedEvent, value);
        remove => RemoveHandler(DeleteRequestedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? ChipClick
    {
        add => AddHandler(ChipClickEvent, value);
        remove => RemoveHandler(ChipClickEvent, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ApplyVariant();
        ApplySize();
        ApplyClickableCursor();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _root = e.NameScope.Find(RootPartName) as Border;
        _avatarHost = e.NameScope.Find(AvatarHostPartName) as ContentPresenter;
        _labelBlock = e.NameScope.Find(LabelPartName) as TextBlock;
        if (e.NameScope.Find(DeletePartName) is Button db)
        {
            if (_deleteButton is not null)
                _deleteButton.Click -= OnDeleteClick;
            _deleteButton = db;
            _deleteButton.Click += OnDeleteClick;
        }

        if (_root is not null)
        {
            _root.PointerPressed -= OnRootPointerPressed;
            _root.PointerPressed += OnRootPointerPressed;
        }

        SyncLabel();
        SyncAvatarVisibility();
        SyncDeleteVisibility();
        ApplyVariant();
        ApplySize();
        ApplyClickableCursor();
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        RaiseEvent(new RoutedEventArgs(DeleteRequestedEvent));
    }

    private void OnRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsClickable)
            return;

        if (e.Source is not Visual source)
            return;

        if (_deleteButton is not null && IsDescendant(_deleteButton, source))
            return;

        e.Handled = true;
        RaiseEvent(new RoutedEventArgs(ChipClickEvent));
    }

    private static bool IsDescendant(Visual ancestor, Visual? node)
    {
        for (StyledElement? el = node; el is not null; el = el.Parent)
        {
            if (ReferenceEquals(el, ancestor))
                return true;
        }

        return false;
    }

    private void SyncLabel()
    {
        if (_labelBlock is not null)
            _labelBlock.Text = Label ?? string.Empty;
    }

    private void SyncAvatarVisibility()
    {
        if (_avatarHost is not null)
            _avatarHost.IsVisible = AvatarContent is not null;
    }

    private void SyncDeleteVisibility()
    {
        if (_deleteButton is not null)
            _deleteButton.IsVisible = IsDeletable;
    }

    private void ApplyVariant()
    {
        switch (Variant)
        {
            case ChipVariant.Filled:
                Background = FindBrush(SkyTokenKeys.Brush.SurfaceElevated,
                    FindBrush("SpotifySurfaceElevatedBrush", new SolidColorBrush(Color.Parse("#1f1f1f"))));
                BorderBrush = Brushes.Transparent;
                BorderThickness = new Thickness(0);
                break;
            case ChipVariant.Outlined:
                Background = Brushes.Transparent;
                BorderBrush = FindBrush(SkyTokenKeys.Brush.BorderStrong,
                    FindBrush("SpotifyBorderLightBrush", new SolidColorBrush(Color.Parse("#7c7c7c"))));
                BorderThickness = new Thickness(1);
                break;
        }

        Foreground = FindBrush(SkyTokenKeys.Brush.TextPrimary,
            FindBrush("SpotifyTextPrimaryBrush", Brushes.White));
    }

    private void ApplySize()
    {
        var isSmall = Size == ChipSize.Small;
        Padding = isSmall ? new Thickness(6, 2, 4, 2) : new Thickness(8, 4, 6, 4);
        MinHeight = isSmall ? 24 : 32;

        if (_labelBlock is not null)
            _labelBlock.FontSize = isSmall ? 12 : 13;

        if (_deleteButton is not null)
        {
            _deleteButton.FontSize = isSmall ? 14 : 16;
            _deleteButton.MinWidth = isSmall ? 20 : 24;
            _deleteButton.MinHeight = isSmall ? 20 : 24;
        }
    }

    private void ApplyClickableCursor()
    {
        if (IsClickable)
            Classes.Add("sky-clickable");
        else
            Classes.Remove("sky-clickable");
        Cursor = IsClickable ? new Cursor(StandardCursorType.Hand) : Cursor.Default;
    }

    private IBrush FindBrush(string key, IBrush fallback)
    {
        if (TryGetResource(key, ActualThemeVariant, out var o) && o is IBrush b)
            return b;
        return fallback;
    }
}
