using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace SkyUI.Controls;

public class Badge : ContentControl
{
    public const string BadgeContainerPartName = "PART_BadgeContainer";
    public const string BadgeContentPartName = "PART_BadgeContent";

    public static readonly StyledProperty<object?> BadgeContentProperty =
        AvaloniaProperty.Register<Badge, object?>(nameof(BadgeContent));

    public static readonly StyledProperty<bool> IsDotProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(IsDot));

    public static readonly StyledProperty<BadgePlacement> PlacementProperty =
        AvaloniaProperty.Register<Badge, BadgePlacement>(nameof(Placement), BadgePlacement.TopRight);

    public static readonly StyledProperty<IBrush?> BadgeBackgroundProperty =
        AvaloniaProperty.Register<Badge, IBrush?>(nameof(BadgeBackground));

    public static readonly StyledProperty<IBrush?> BadgeForegroundProperty =
        AvaloniaProperty.Register<Badge, IBrush?>(nameof(BadgeForeground));

    public static readonly StyledProperty<double> HorizontalBadgeOffsetProperty =
        AvaloniaProperty.Register<Badge, double>(nameof(HorizontalBadgeOffset));

    public static readonly StyledProperty<double> VerticalBadgeOffsetProperty =
        AvaloniaProperty.Register<Badge, double>(nameof(VerticalBadgeOffset));

    /// <summary>When true, hides the badge when <see cref="BadgeContent"/> is null or empty (numbers equal to 0 are hidden unless <see cref="ShowZero"/> is true).</summary>
    public static readonly StyledProperty<bool> AutoHideProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(AutoHide), true);

    /// <summary>When <see cref="AutoHide"/> is true, still show a numeric zero badge.</summary>
    public static readonly StyledProperty<bool> ShowZeroProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(ShowZero));

    /// <summary>Force badge visibility; when false the badge is always hidden.</summary>
    public static readonly StyledProperty<bool> ShowBadgeProperty =
        AvaloniaProperty.Register<Badge, bool>(nameof(ShowBadge), true);

    private Border? _badgeContainer;
    private ContentPresenter? _badgeContentPresenter;

    static Badge()
    {
        BadgeContentProperty.Changed.AddClassHandler<Badge>((b, _) => b.ApplyBadgeState());
        IsDotProperty.Changed.AddClassHandler<Badge>((b, _) => b.ApplyBadgeState());
        AutoHideProperty.Changed.AddClassHandler<Badge>((b, _) => b.ApplyBadgeState());
        ShowZeroProperty.Changed.AddClassHandler<Badge>((b, _) => b.ApplyBadgeState());
        ShowBadgeProperty.Changed.AddClassHandler<Badge>((b, _) => b.ApplyBadgeState());
        PlacementProperty.Changed.AddClassHandler<Badge>((b, _) => b.ApplyPlacement());
        HorizontalBadgeOffsetProperty.Changed.AddClassHandler<Badge>((b, _) => b.ApplyPlacement());
        VerticalBadgeOffsetProperty.Changed.AddClassHandler<Badge>((b, _) => b.ApplyPlacement());
        ClipToBoundsProperty.OverrideDefaultValue<Badge>(false);
    }

    public object? BadgeContent
    {
        get => GetValue(BadgeContentProperty);
        set => SetValue(BadgeContentProperty, value);
    }

    public bool IsDot
    {
        get => GetValue(IsDotProperty);
        set => SetValue(IsDotProperty, value);
    }

    public BadgePlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public IBrush? BadgeBackground
    {
        get => GetValue(BadgeBackgroundProperty);
        set => SetValue(BadgeBackgroundProperty, value);
    }

    public IBrush? BadgeForeground
    {
        get => GetValue(BadgeForegroundProperty);
        set => SetValue(BadgeForegroundProperty, value);
    }

    public double HorizontalBadgeOffset
    {
        get => GetValue(HorizontalBadgeOffsetProperty);
        set => SetValue(HorizontalBadgeOffsetProperty, value);
    }

    public double VerticalBadgeOffset
    {
        get => GetValue(VerticalBadgeOffsetProperty);
        set => SetValue(VerticalBadgeOffsetProperty, value);
    }

    public bool AutoHide
    {
        get => GetValue(AutoHideProperty);
        set => SetValue(AutoHideProperty, value);
    }

    public bool ShowZero
    {
        get => GetValue(ShowZeroProperty);
        set => SetValue(ShowZeroProperty, value);
    }

    public bool ShowBadge
    {
        get => GetValue(ShowBadgeProperty);
        set => SetValue(ShowBadgeProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _badgeContainer = e.NameScope.Find(BadgeContainerPartName) as Border;
        _badgeContentPresenter = e.NameScope.Find(BadgeContentPartName) as ContentPresenter;

        ApplyPlacement();
        ApplyBadgeState();
    }

    private bool ShouldDisplayBadge()
    {
        if (!ShowBadge)
            return false;

        if (IsDot)
            return true;

        if (!AutoHide)
            return true;

        if (BadgeContent is null)
            return false;

        if (BadgeContent is string s)
            return !string.IsNullOrEmpty(s);

        if (BadgeContent is int i)
            return ShowZero || i != 0;

        if (BadgeContent is long l)
            return ShowZero || l != 0;

        return true;
    }

    private void ApplyBadgeState()
    {
        if (_badgeContainer is null)
            return;

        var visible = ShouldDisplayBadge();
        _badgeContainer.IsVisible = visible;

        if (!visible)
            return;

        if (IsDot)
        {
            _badgeContainer.Padding = new Thickness(0);
            _badgeContainer.MinWidth = 8;
            _badgeContainer.MinHeight = 8;
            _badgeContainer.CornerRadius = new CornerRadius(4);
            if (_badgeContentPresenter is not null)
                _badgeContentPresenter.IsVisible = false;
        }
        else
        {
            _badgeContainer.Padding = new Thickness(4, 2);
            _badgeContainer.MinWidth = 18;
            _badgeContainer.MinHeight = 18;
            _badgeContainer.CornerRadius = new CornerRadius(10);
            if (_badgeContentPresenter is not null)
                _badgeContentPresenter.IsVisible = true;
        }
    }

    private void ApplyPlacement()
    {
        if (_badgeContainer is null)
            return;

        var h = HorizontalBadgeOffset;
        var v = VerticalBadgeOffset;

        switch (Placement)
        {
            case BadgePlacement.TopRight:
                _badgeContainer.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                _badgeContainer.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
                _badgeContainer.Margin = new Thickness(0, -6 + v, -6 + h, 0);
                break;
            case BadgePlacement.TopLeft:
                _badgeContainer.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                _badgeContainer.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
                _badgeContainer.Margin = new Thickness(-6 + h, -6 + v, 0, 0);
                break;
            case BadgePlacement.BottomRight:
                _badgeContainer.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                _badgeContainer.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
                _badgeContainer.Margin = new Thickness(0, 0, -6 + h, -6 + v);
                break;
            case BadgePlacement.BottomLeft:
                _badgeContainer.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                _badgeContainer.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
                _badgeContainer.Margin = new Thickness(-6 + h, 0, 0, -6 + v);
                break;
        }
    }
}
