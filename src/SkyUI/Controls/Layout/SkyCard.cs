using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace SkyUI.Controls;

/// <summary>Elevated surface container with optional header and footer.</summary>
[TemplatePart(SkyCard.PartRoot, typeof(Border))]
public class SkyCard : ContentControl
{
    public const string PartRoot = "PART_Root";
    public const string PartHeaderPresenter = "PART_HeaderPresenter";
    public const string PartFooterPresenter = "PART_FooterPresenter";

    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<SkyCard, object?>(nameof(Header));

    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<SkyCard, IDataTemplate?>(nameof(HeaderTemplate));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<SkyCard, object?>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<SkyCard, IDataTemplate?>(nameof(FooterTemplate));

    public static readonly StyledProperty<bool> IsHoverableProperty =
        AvaloniaProperty.Register<SkyCard, bool>(nameof(IsHoverable));

    static SkyCard()
    {
        IsHoverableProperty.Changed.AddClassHandler<SkyCard>((card, _) => card.SyncHoverClass());
    }

    public SkyCard()
    {
        Classes.Add("sky");
        Classes.Add("sky-card");
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    public bool IsHoverable
    {
        get => GetValue(IsHoverableProperty);
        set => SetValue(IsHoverableProperty, value);
    }

    private void SyncHoverClass() => Classes.Set("sky-card-hoverable", IsHoverable);
}
