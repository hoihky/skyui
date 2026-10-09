using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Button that reveals a menu of secondary commands.</summary>
public class SkyDropDownButton : SkyDropDownButtonBase
{
    public const string AnchorPartName = "PART_Anchor";

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<SkyDropDownButton>();

    private Button? anchor;

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public SkyDropDownButton() => Classes.Add("sky-drop-down-button");

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        anchor = e.NameScope.Find<Button>(AnchorPartName);
        if (anchor is null)
            return;
        anchor.Click += OnAnchorClick;
    }

    private void OnAnchorClick(object? sender, RoutedEventArgs e)
    {
        if (anchor is not null)
            ShowDropDown(anchor);
    }
}
