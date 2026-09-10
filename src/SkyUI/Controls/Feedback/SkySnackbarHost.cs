using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using SkyUI.Core.Theming;

namespace SkyUI.Controls;

/// <summary>Hosts a queue of transient snackbar messages (bottom of layout).</summary>
public class SkySnackbarHost : TemplatedControl
{
    public const string ItemsHostPartName = "PART_ItemsHost";

    public static readonly StyledProperty<int> DefaultDurationMsProperty =
        AvaloniaProperty.Register<SkySnackbarHost, int>(nameof(DefaultDurationMs), 4000);

    private ItemsControl? itemsHost;
    private readonly ObservableCollection<SkySnackbarMessage> messages = new();
    private readonly Queue<(SkySnackbarMessage Message, DispatcherTimer Timer)> queue = new();
    private bool showing;

    public int DefaultDurationMs
    {
        get => GetValue(DefaultDurationMsProperty);
        set => SetValue(DefaultDurationMsProperty, value);
    }

    public ReadOnlyObservableCollection<SkySnackbarMessage> Messages { get; }

    public SkySnackbarHost()
    {
        Messages = new ReadOnlyObservableCollection<SkySnackbarMessage>(messages);
    }

    public void Enqueue(string message, SkyFeedbackVariant variant = SkyFeedbackVariant.Neutral, int? durationMs = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var duration = durationMs ?? DefaultDurationMs;
        var item = new SkySnackbarMessage(message.Trim(), variant, Math.Max(1000, duration));
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(item.DurationMs) };
        timer.Tick += (_, _) => _ = ExpireMessageAsync(item, timer);
        queue.Enqueue((item, timer));
        TryShowNext();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        itemsHost = e.NameScope.Find(ItemsHostPartName) as ItemsControl;
        if (itemsHost is not null)
            itemsHost.ItemsSource = messages;
    }

    private void TryShowNext()
    {
        if (showing || queue.Count == 0)
            return;

        var (message, timer) = queue.Dequeue();
        showing = true;
        messages.Add(message);
        timer.Start();
        _ = PlayEnterAnimationAsync(message);
    }

    private async Task PlayEnterAnimationAsync(SkySnackbarMessage message)
    {
        if (itemsHost is null)
            return;

        SkySnackbarBar? bar = null;
        for (var attempt = 0; attempt < 8 && bar is null; attempt++)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { });
            bar = itemsHost.ContainerFromItem(message) as SkySnackbarBar;
            if (bar is null)
                await Task.Delay(16);
        }

        if (bar is null)
            return;

        bar.Opacity = 0;
        await Task.WhenAll(
            SkyMotionAnimator.Default.FadeAsync(bar, 0, 1, SkyMotionDurations.Enter),
            SkyMotionAnimator.Default.TranslateYAsync(bar, 16, 0, SkyMotionDurations.Enter));
    }

    private async Task ExpireMessageAsync(SkySnackbarMessage message, DispatcherTimer timer)
    {
        timer.Stop();
        if (itemsHost is not null)
        {
            var bar = itemsHost.ContainerFromItem(message) as SkySnackbarBar;
            if (bar is not null)
            {
                await Task.WhenAll(
                    SkyMotionAnimator.Default.FadeAsync(bar, bar.Opacity, 0, SkyMotionDurations.Exit),
                    SkyMotionAnimator.Default.TranslateYAsync(bar, 0, 8, SkyMotionDurations.Exit));
            }
        }

        messages.Remove(message);
        showing = false;
        TryShowNext();
    }
}
