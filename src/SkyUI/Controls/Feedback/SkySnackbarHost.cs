using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;

namespace SkyUI.Controls;

/// <summary>Hosts a queue of transient snackbar messages (bottom of layout).</summary>
public class SkySnackbarHost : TemplatedControl
{
    public const string ItemsHostPartName = "PART_ItemsHost";

    public static readonly StyledProperty<int> DefaultDurationMsProperty =
        AvaloniaProperty.Register<SkySnackbarHost, int>(nameof(DefaultDurationMs), 4000);

    private ItemsControl? _itemsHost;
    private readonly ObservableCollection<SkySnackbarMessage> _messages = new();
    private readonly Queue<(SkySnackbarMessage Message, DispatcherTimer Timer)> _queue = new();
    private bool _showing;

    public int DefaultDurationMs
    {
        get => GetValue(DefaultDurationMsProperty);
        set => SetValue(DefaultDurationMsProperty, value);
    }

    public ReadOnlyObservableCollection<SkySnackbarMessage> Messages { get; }

    public SkySnackbarHost()
    {
        Messages = new ReadOnlyObservableCollection<SkySnackbarMessage>(_messages);
    }

    public void Enqueue(string message, SkyFeedbackVariant variant = SkyFeedbackVariant.Neutral, int? durationMs = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var duration = durationMs ?? DefaultDurationMs;
        var item = new SkySnackbarMessage(message.Trim(), variant, Math.Max(1000, duration));
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(item.DurationMs) };
        timer.Tick += (_, _) => OnMessageExpired(item, timer);
        _queue.Enqueue((item, timer));
        TryShowNext();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsHost = e.NameScope.Find(ItemsHostPartName) as ItemsControl;
        if (_itemsHost is not null)
            _itemsHost.ItemsSource = _messages;
    }

    private void TryShowNext()
    {
        if (_showing || _queue.Count == 0)
            return;

        var (message, timer) = _queue.Dequeue();
        _showing = true;
        _messages.Add(message);
        timer.Start();
    }

    private void OnMessageExpired(SkySnackbarMessage message, DispatcherTimer timer)
    {
        timer.Stop();
        _messages.Remove(message);
        _showing = false;
        TryShowNext();
    }
}
