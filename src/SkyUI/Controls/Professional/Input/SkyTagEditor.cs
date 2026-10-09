using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using SkyUI.Controls;

namespace SkyUI.Controls.Professional;

public class SkyTagEditor : TemplatedControl
{
    public const string TagsHostPartName = "PART_TagsHost";
    public const string InputPartName = "PART_Input";

    public static readonly StyledProperty<IEnumerable?> TagsProperty =
        AvaloniaProperty.Register<SkyTagEditor, IEnumerable?>(nameof(Tags));

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<SkyTagEditor, string?>(nameof(PlaceholderText), "Add tag…");

    public static readonly RoutedEvent<RoutedEventArgs> TagAddedEvent =
        RoutedEvent.Register<SkyTagEditor, RoutedEventArgs>(nameof(TagAdded), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<RoutedEventArgs> TagRemovedEvent =
        RoutedEvent.Register<SkyTagEditor, RoutedEventArgs>(nameof(TagRemoved), RoutingStrategies.Bubble);

    private WrapPanel? tagsHost;
    private TextBox? input;
    private INotifyCollectionChanged? subscribed;

    public IEnumerable? Tags
    {
        get => GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? TagAdded
    {
        add => AddHandler(TagAddedEvent, value);
        remove => RemoveHandler(TagAddedEvent, value);
    }

    public event EventHandler<RoutedEventArgs>? TagRemoved
    {
        add => AddHandler(TagRemovedEvent, value);
        remove => RemoveHandler(TagRemovedEvent, value);
    }

    static SkyTagEditor()
    {
        TagsProperty.Changed.AddClassHandler<SkyTagEditor>((e, _) => e.RebuildTags());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        tagsHost = e.NameScope.Find<WrapPanel>(TagsHostPartName);
        input = e.NameScope.Find<TextBox>(InputPartName);
        if (input is not null)
            input.KeyDown += OnInputKeyDown;
        RebuildTags();
    }

    public void CommitInput()
    {
        if (input is null || Tags is not IList list)
            return;
        var text = input.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return;
        if (!list.Contains(text))
        {
            list.Add(text);
            RaiseEvent(new RoutedEventArgs(TagAddedEvent));
        }

        input.Text = "";
        RebuildTags();
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            CommitInput();
            e.Handled = true;
        }
    }

    private void RebuildTags()
    {
        if (tagsHost is null)
            return;
        Unsubscribe();
        tagsHost.Children.Clear();
        if (Tags is null)
            return;
        foreach (var tag in Tags)
            tagsHost.Children.Add(CreateTagPill(tag));

        if (Tags is INotifyCollectionChanged notify)
        {
            subscribed = notify;
            subscribed.CollectionChanged += OnTagsChanged;
        }
    }

    private void OnTagsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildTags();

    private Control CreateTagPill(object? tag)
    {
        var label = new TextBlock
        {
            Text = tag?.ToString(),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 4, 0),
            Classes = { "sky-body-secondary" },
        };
        var remove = new Button
        {
            Classes = { "sky" },
            Content = "×",
            Padding = new Thickness(0),
            MinWidth = 22,
            MinHeight = 22,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            FontSize = 14,
        };
        remove.Click += (_, e) =>
        {
            if (Tags is IList list)
                list.Remove(tag);
            RaiseEvent(new RoutedEventArgs(TagRemovedEvent));
            RebuildTags();
            e.Handled = true;
        };
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
        };
        Grid.SetColumn(label, 0);
        Grid.SetColumn(remove, 1);
        grid.Children.Add(label);
        grid.Children.Add(remove);
        return new Border
        {
            Classes = { "sky-card" },
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(2, 2, 4, 2),
            Margin = new Thickness(0, 0, 6, 6),
            Child = grid,
        };
    }

    private void Unsubscribe()
    {
        if (subscribed is null)
            return;
        subscribed.CollectionChanged -= OnTagsChanged;
        subscribed = null;
    }
}
