using Avalonia.Interactivity;

namespace SkyUI.Controls;

/// <summary>Static message-box API backed by <see cref="SkyDialogHost"/>.</summary>
public static class SkyMessageBox
{
    private static SkyDialogHost? attachedHost;
    private static readonly SemaphoreSlim dialogGate = new(1, 1);

    /// <summary>Registers the dialog host used by static <c>ShowAsync</c> calls.</summary>
    public static void Attach(SkyDialogHost host) => attachedHost = host;

    internal static void ResetForTests() => attachedHost = null;

    public static async Task ShowInfoAsync(
        string message,
        string? title = "Information",
        string primaryText = "OK",
        SkyDialogHost? dialogHost = null,
        CancellationToken cancellationToken = default) =>
        await ShowAsync(message, title, SkyMessageBoxKind.Info, primaryText, secondaryText: null, dialogHost, cancellationToken)
            .ConfigureAwait(false);

    public static async Task ShowWarningAsync(
        string message,
        string? title = "Warning",
        string primaryText = "OK",
        SkyDialogHost? dialogHost = null,
        CancellationToken cancellationToken = default) =>
        await ShowAsync(message, title, SkyMessageBoxKind.Warning, primaryText, secondaryText: null, dialogHost, cancellationToken)
            .ConfigureAwait(false);

    public static async Task ShowErrorAsync(
        string message,
        string? title = "Error",
        string primaryText = "OK",
        SkyDialogHost? dialogHost = null,
        CancellationToken cancellationToken = default) =>
        await ShowAsync(message, title, SkyMessageBoxKind.Error, primaryText, secondaryText: null, dialogHost, cancellationToken)
            .ConfigureAwait(false);

    public static Task<SkyMessageBoxResult> ConfirmAsync(
        string message,
        string? title = "Confirm",
        string primaryText = "OK",
        string secondaryText = "Cancel",
        SkyDialogHost? dialogHost = null,
        CancellationToken cancellationToken = default) =>
        ShowAsync(message, title, SkyMessageBoxKind.Confirm, primaryText, secondaryText, dialogHost, cancellationToken);

    public static async Task<SkyMessageBoxResult> ShowAsync(
        string message,
        string? title,
        SkyMessageBoxKind kind,
        string primaryText = "OK",
        string? secondaryText = null,
        SkyDialogHost? dialogHost = null,
        CancellationToken cancellationToken = default)
    {
        var host = dialogHost ?? attachedHost
            ?? throw new InvalidOperationException("Call SkyMessageBox.Attach(SkyDialogHost) or pass dialogHost.");

        await dialogGate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await ShowOnHostAsync(host, message, title, kind, primaryText, secondaryText, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            dialogGate.Release();
        }
    }

    private static Task<SkyMessageBoxResult> ShowOnHostAsync(
        SkyDialogHost host,
        string message,
        string? title,
        SkyMessageBoxKind kind,
        string primaryText,
        string? secondaryText,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<SkyMessageBoxResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var resolvedSecondary = kind == SkyMessageBoxKind.Confirm ? secondaryText ?? "Cancel" : secondaryText;
        var variant = MapVariant(kind);

        EventHandler<RoutedEventArgs>? onPrimary = null;
        EventHandler<RoutedEventArgs>? onSecondary = null;
        EventHandler<RoutedEventArgs>? onClosed = null;
        var completed = false;

        void Complete(SkyMessageBoxResult result)
        {
            if (completed)
                return;

            completed = true;
            DetachHandlers();
            host.Close();
            tcs.TrySetResult(result);
        }

        void DetachHandlers()
        {
            if (onPrimary is not null)
                host.PrimaryAction -= onPrimary;
            if (onSecondary is not null)
                host.SecondaryAction -= onSecondary;
            if (onClosed is not null)
                host.Closed -= onClosed;
        }

        onPrimary = (_, _) => Complete(SkyMessageBoxResult.Primary);
        onSecondary = (_, _) => Complete(SkyMessageBoxResult.Secondary);
        onClosed = (_, _) => Complete(SkyMessageBoxResult.None);

        host.Title = title;
        host.DialogContent = new SkyAlert
        {
            Title = title,
            Message = message,
            Variant = variant,
            IsCloseable = false,
        };
        host.PrimaryButtonText = primaryText;
        host.SecondaryButtonText = resolvedSecondary;
        host.PrimaryAction += onPrimary;
        if (!string.IsNullOrEmpty(resolvedSecondary))
            host.SecondaryAction += onSecondary;
        host.Closed += onClosed;

        if (cancellationToken.CanBeCanceled)
            cancellationToken.Register(() => Complete(SkyMessageBoxResult.None));

        host.Show();
        return tcs.Task;
    }

    private static SkyFeedbackVariant MapVariant(SkyMessageBoxKind kind) =>
        kind switch
        {
            SkyMessageBoxKind.Info => SkyFeedbackVariant.Info,
            SkyMessageBoxKind.Warning => SkyFeedbackVariant.Warning,
            SkyMessageBoxKind.Error => SkyFeedbackVariant.Danger,
            SkyMessageBoxKind.Confirm => SkyFeedbackVariant.Neutral,
            _ => SkyFeedbackVariant.Neutral,
        };
}
