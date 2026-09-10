using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkyUI.Controls;

/// <summary>Form-level validation list that can focus the first invalid field.</summary>
public class SkyValidationSummary : TemplatedControl
{
    public const string ItemsPartName = "PART_Items";

    public static readonly DirectProperty<SkyValidationSummary, IList> ItemsProperty =
        AvaloniaProperty.RegisterDirect<SkyValidationSummary, IList>(
            nameof(Items),
            summary => summary.Items);

    public static readonly StyledProperty<string?> HeaderProperty =
        AvaloniaProperty.Register<SkyValidationSummary, string?>(nameof(Header), "Please fix the following:");

    private readonly AvaloniaList<SkyValidationSummaryItem> items = new();
    private readonly Dictionary<string, Control> registeredFields = new(StringComparer.Ordinal);
    private ListBox? itemsList;

    public SkyValidationSummary()
    {
        Classes.Add("sky");
        Classes.Add("sky-validation-summary");
        items.CollectionChanged += OnItemsChanged;
    }

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public IList Items => items;

    public bool HasErrors => items.Count > 0;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (itemsList is not null)
            itemsList.SelectionChanged -= OnItemSelectionChanged;

        itemsList = e.NameScope.Find(ItemsPartName) as ListBox;
        if (itemsList is not null)
            itemsList.SelectionChanged += OnItemSelectionChanged;

        SyncItems();
    }

    public void RegisterField(string fieldKey, Control control) =>
        registeredFields[fieldKey] = control;

    public void UnregisterField(string fieldKey) =>
        registeredFields.Remove(fieldKey);

    public void SetErrors(IEnumerable<SkyValidationSummaryItem> errors)
    {
        items.Clear();
        foreach (var error in errors)
            items.Add(error);
    }

    public void ClearErrors() => items.Clear();

    public bool FocusFirstInvalidField()
    {
        if (items.Count == 0)
            return false;

        var first = items[0];
        if (!registeredFields.TryGetValue(first.FieldKey, out var control))
            return false;

        return control.Focus();
    }

    public void OnErrorItemActivated(SkyValidationSummaryItem item)
    {
        if (registeredFields.TryGetValue(item.FieldKey, out var control))
            control.Focus();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        SyncItems();

    private void OnItemSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (itemsList?.SelectedItem is SkyValidationSummaryItem item)
            OnErrorItemActivated(item);
    }

    private void SyncItems()
    {
        if (itemsList is not null)
            itemsList.ItemsSource = items;

        IsVisible = items.Count > 0;
    }
}
