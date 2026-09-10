using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace SkyUI.Controls;

/// <summary>iOS-style action sheet presented from the bottom of the screen.</summary>
public static class SkyActionSheet
{
    private static SkySheetHost? attachedHost;

    public static void Attach(SkySheetHost host) => attachedHost = host;

    internal static void ResetForTests() => attachedHost = null;

    public static Task<int?> ShowAsync(
        IReadOnlyList<SkyActionSheetItem> items,
        string? title = null,
        string cancelText = "Cancel",
        SkySheetHost? sheetHost = null,
        CancellationToken cancellationToken = default)
    {
        var host = sheetHost ?? attachedHost
            ?? throw new InvalidOperationException("Call SkyActionSheet.Attach(SkySheetHost) or pass sheetHost.");

        return ShowOnHostAsync(host, items, title, cancelText, cancellationToken);
    }

    private static Task<int?> ShowOnHostAsync(
        SkySheetHost host,
        IReadOnlyList<SkyActionSheetItem> items,
        string? title,
        string cancelText,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<int?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var completed = false;

        void Complete(int? result)
        {
            if (completed)
                return;

            completed = true;
            host.Close();
            ScheduleCleanup(host);
            tcs.TrySetResult(result);
        }

        var root = new StackPanel { Spacing = 8 };
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var button = CreateActionButton(item, index, Complete);
            SkyTouchTarget.SetEnsureTouchTarget(button, true);
            root.Children.Add(button);
        }

        var cancel = new Button
        {
            Content = cancelText,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            MinHeight = SkyTouchTarget.RecommendedSize,
        };
        cancel.Classes.Add("sky");
        cancel.Classes.Add("sky-outlined");
        cancel.Click += (_, _) => Complete(null);
        root.Children.Add(cancel);

        if (cancellationToken.CanBeCanceled)
            cancellationToken.Register(() => Complete(null));

        host.Title = title;
        host.SheetContent = root;
        host.Show();
        return tcs.Task;
    }

    private static Button CreateActionButton(
        SkyActionSheetItem item,
        int index,
        Action<int?> complete)
    {
        var button = new Button
        {
            Content = item.Title,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            MinHeight = SkyTouchTarget.RecommendedSize,
        };
        button.Classes.Add("sky");
        button.Classes.Add("sky-subtle");
        if (item.IsDestructive)
            button.Foreground = new SolidColorBrush(Color.Parse("#FF453A"));
        button.Click += (_, _) => complete(item.IsCancel ? null : index);
        return button;
    }

    private static void ScheduleCleanup(SkySheetHost host)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (host.IsOpen)
                return;

            host.SheetContent = null;
            host.Title = null;
        }, DispatcherPriority.Background);
    }
}
