using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace SkyUI.Controls;

/// <summary>Typeahead input with optional async provider, free text, and suggestion popup.</summary>
public class SkyAutocomplete : TemplatedControl
{
    public const string TextBoxPartName = "PART_TextBox";
    public const string PopupPartName = "PART_Popup";
    public const string SuggestionsPartName = "PART_Suggestions";

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SkyAutocomplete, string?>(
            nameof(Text),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<SkyAutocomplete, object?>(
            nameof(SelectedItem),
            defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<SkyAutocomplete, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<ISkyAutocompleteProvider?> ProviderProperty =
        AvaloniaProperty.Register<SkyAutocomplete, ISkyAutocompleteProvider?>(nameof(Provider));

    public static readonly StyledProperty<bool> IsFreeTextAllowedProperty =
        AvaloniaProperty.Register<SkyAutocomplete, bool>(nameof(IsFreeTextAllowed), true);

    public static readonly StyledProperty<int> MinimumQueryLengthProperty =
        AvaloniaProperty.Register<SkyAutocomplete, int>(nameof(MinimumQueryLength), 1);

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<SkyAutocomplete, string?>(nameof(PlaceholderText));

    public static readonly StyledProperty<bool> IsSuggestionOpenProperty =
        AvaloniaProperty.Register<SkyAutocomplete, bool>(nameof(IsSuggestionOpen));

    private TextBox? textBox;
    private Popup? popup;
    private ListBox? suggestions;
    private CancellationTokenSource? searchCancellation;
    private int searchVersion;

    static SkyAutocomplete()
    {
        TextProperty.Changed.AddClassHandler<SkyAutocomplete>((control, e) =>
        {
            control.QueueTextChanged();
        });
        ItemsSourceProperty.Changed.AddClassHandler<SkyAutocomplete>((control, e) => control.RefreshLocalSuggestions());
    }

    public SkyAutocomplete()
    {
        Classes.Add("sky");
        Classes.Add("sky-autocomplete");
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public ISkyAutocompleteProvider? Provider
    {
        get => GetValue(ProviderProperty);
        set => SetValue(ProviderProperty, value);
    }

    public bool IsFreeTextAllowed
    {
        get => GetValue(IsFreeTextAllowedProperty);
        set => SetValue(IsFreeTextAllowedProperty, value);
    }

    public int MinimumQueryLength
    {
        get => GetValue(MinimumQueryLengthProperty);
        set => SetValue(MinimumQueryLengthProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public bool IsSuggestionOpen
    {
        get => GetValue(IsSuggestionOpenProperty);
        private set => SetValue(IsSuggestionOpenProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (textBox is not null)
        {
            textBox.TextChanged -= OnInputTextChanged;
            textBox.KeyDown -= OnInputKeyDown;
            textBox.GotFocus -= OnInputGotFocus;
        }

        if (suggestions is not null)
            suggestions.SelectionChanged -= OnSuggestionSelectionChanged;

        textBox = e.NameScope.Find(TextBoxPartName) as TextBox;
        popup = e.NameScope.Find(PopupPartName) as Popup;
        suggestions = e.NameScope.Find(SuggestionsPartName) as ListBox;

        if (textBox is not null)
        {
            textBox.Classes.Add("sky");
            textBox.Text = Text;
            textBox.PlaceholderText = PlaceholderText;
            textBox.TextChanged += OnInputTextChanged;
            textBox.KeyDown += OnInputKeyDown;
            textBox.GotFocus += OnInputGotFocus;
        }

        if (suggestions is not null)
            suggestions.SelectionChanged += OnSuggestionSelectionChanged;

        RefreshLocalSuggestions();
    }

    private void OnInputGotFocus(object? sender, RoutedEventArgs e) =>
        _ = OnTextChangedAsync();

    private void OnInputTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (textBox is null)
            return;

        Text = textBox.Text;
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            CloseSuggestions();
    }

    private void OnSuggestionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (suggestions?.SelectedItem is not SkyAutocompleteItem item)
            return;

        ApplySelection(item);
    }

    private void QueueTextChanged() =>
        _ = OnTextChangedAsync();

    private async Task OnTextChangedAsync()
    {
        if (textBox is not null && textBox.Text != Text)
            textBox.Text = Text;

        var query = Text ?? string.Empty;
        if (query.Length < MinimumQueryLength)
        {
            CloseSuggestions();
            return;
        }

        if (Provider is not null)
            await SearchAsync(query);
        else
            RefreshLocalSuggestions(query);
    }

    private async Task SearchAsync(string query)
    {
        searchCancellation?.Cancel();
        searchCancellation = new CancellationTokenSource();
        var token = searchCancellation.Token;
        var version = ++searchVersion;

        try
        {
            var results = await Provider!.SearchAsync(query, token);
            if (token.IsCancellationRequested || version != searchVersion)
                return;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (suggestions is null)
                    return;

                suggestions.ItemsSource = results;
                IsSuggestionOpen = results.Count > 0;
                popup?.IsOpen = IsSuggestionOpen;
            });
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer query.
        }
    }

    private void RefreshLocalSuggestions(string? query = null)
    {
        if (suggestions is null)
            return;

        var items = FilterLocalItems(query ?? Text);
        suggestions.ItemsSource = items;
        IsSuggestionOpen = items.Count > 0 && (textBox?.IsFocused ?? false);
        if (popup is not null)
            popup.IsOpen = IsSuggestionOpen;
    }

    private List<SkyAutocompleteItem> FilterLocalItems(string? query)
    {
        var results = new List<SkyAutocompleteItem>();
        if (ItemsSource is null)
            return results;

        foreach (var item in ItemsSource)
        {
            var text = item switch
            {
                SkyAutocompleteItem autocompleteItem => autocompleteItem.Text,
                string value => value,
                _ => item?.ToString(),
            };

            if (string.IsNullOrWhiteSpace(text))
                continue;

            if (!string.IsNullOrWhiteSpace(query) &&
                !text.Contains(query, StringComparison.OrdinalIgnoreCase))
                continue;

            results.Add(item is SkyAutocompleteItem existing
                ? existing
                : new SkyAutocompleteItem(text, item));
        }

        return results;
    }

    private void ApplySelection(SkyAutocompleteItem item)
    {
        Text = item.Text;
        SelectedItem = item.Value;
        CloseSuggestions();
    }

    private void CloseSuggestions()
    {
        IsSuggestionOpen = false;
        if (popup is not null)
            popup.IsOpen = false;
    }
}
