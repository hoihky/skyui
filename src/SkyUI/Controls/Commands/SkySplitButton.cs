using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Primary action button with a separate drop-down for alternates.</summary>
public class SkySplitButton : SkyDropDownButtonBase
{
    public const string RootPartName = "PART_Root";
    public const string PrimaryPartName = "PART_Primary";
    public const string DropDownPartName = "PART_DropDown";

    public static readonly StyledProperty<object?> PrimaryContentProperty =
        AvaloniaProperty.Register<SkySplitButton, object?>(nameof(PrimaryContent));

    public static readonly StyledProperty<ICommand?> PrimaryCommandProperty =
        AvaloniaProperty.Register<SkySplitButton, ICommand?>(nameof(PrimaryCommand));

    private Button? primary;
    private Button? dropDown;

    public object? PrimaryContent
    {
        get => GetValue(PrimaryContentProperty);
        set => SetValue(PrimaryContentProperty, value);
    }

    public ICommand? PrimaryCommand
    {
        get => GetValue(PrimaryCommandProperty);
        set => SetValue(PrimaryCommandProperty, value);
    }

    public SkySplitButton() => Classes.Add("sky-split-button");

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        primary = e.NameScope.Find<Button>(PrimaryPartName);
        dropDown = e.NameScope.Find<Button>(DropDownPartName);
        if (primary is not null)
            primary.Click += OnPrimaryClick;
        if (dropDown is not null)
            dropDown.Click += OnDropDownClick;

        var root = e.NameScope.Find<Grid>(RootPartName);
        if (root is not null)
        {
            root.PointerEntered += OnRootPointerEntered;
            root.PointerExited += OnRootPointerExited;
        }
    }

    private void OnRootPointerEntered(object? sender, PointerEventArgs e) =>
        Classes.Set("split-hover", true);

    private void OnRootPointerExited(object? sender, PointerEventArgs e) =>
        Classes.Set("split-hover", false);

    private void OnPrimaryClick(object? sender, RoutedEventArgs e)
    {
        if (PrimaryCommand?.CanExecute(null) == true)
            PrimaryCommand.Execute(null);
    }

    private void OnDropDownClick(object? sender, RoutedEventArgs e)
    {
        if (dropDown is not null)
            ShowDropDown(dropDown);
    }
}
