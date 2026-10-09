using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;

namespace SkyUI.Controls;

/// <summary>Bell-style notification hub with read/unread tracking and dismiss actions.</summary>
public class SkyNotificationCenter : TemplatedControl
{
    public const string TogglePartName = "PART_Toggle";
    public const string PopupPartName = "PART_Popup";
    public const string PanelPartName = "PART_Panel";
    public const string ItemsHostPartName = "PART_ItemsHost";
    public const string UnreadBadgePartName = "PART_UnreadBadge";
    public const string UnreadBadgeTextPartName = "PART_UnreadBadgeText";

    public static readonly StyledProperty<bool> IsPanelOpenProperty =
        AvaloniaProperty.Register<SkyNotificationCenter, bool>(nameof(IsPanelOpen));

    public static readonly DirectProperty<SkyNotificationCenter, int> UnreadCountProperty =
        AvaloniaProperty.RegisterDirect<SkyNotificationCenter, int>(
            nameof(UnreadCount),
            c => c.UnreadCount,
            (c, v) => c.UnreadCount = v);

    private readonly ObservableCollection<SkyNotificationEntry> notifications = new();
    private Button? toggle;
    private Popup? popup;
    private ItemsControl? itemsHost;
    private Border? unreadBadgeHost;
    private TextBlock? unreadBadgeText;
    private int unreadCount;

    public SkyNotificationCenter()
    {
        notifications.CollectionChanged += OnNotificationsChanged;
        Notifications = new ReadOnlyObservableCollection<SkyNotificationEntry>(notifications);
        Classes.Add("sky");
        Classes.Add("sky-notification-center");
    }

    public ReadOnlyObservableCollection<SkyNotificationEntry> Notifications { get; }

    public bool IsPanelOpen
    {
        get => GetValue(IsPanelOpenProperty);
        set => SetValue(IsPanelOpenProperty, value);
    }

    public int UnreadCount
    {
        get => unreadCount;
        private set => SetAndRaise(UnreadCountProperty, ref unreadCount, value);
    }

    static SkyNotificationCenter()
    {
        IsPanelOpenProperty.Changed.AddClassHandler<SkyNotificationCenter>((c, e) =>
            c.OnPanelOpenChanged((bool)e.NewValue!));
    }

    public void Push(string title, string? body = null, ICommand? action = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return;
        notifications.Insert(0, new SkyNotificationEntry
        {
            Title = title.Trim(),
            Body = body?.Trim(),
            Timestamp = DateTimeOffset.Now,
            IsRead = false,
            ActionCommand = action,
        });
        RecalculateUnread();
    }

    public void MarkRead(Guid id)
    {
        var entry = notifications.FirstOrDefault(n => n.Id == id);
        if (entry is null || entry.IsRead)
            return;
        entry.IsRead = true;
        RecalculateUnread();
    }

    public void MarkAllRead()
    {
        foreach (var entry in notifications)
            entry.IsRead = true;
        RecalculateUnread();
    }

    public void Dismiss(Guid id)
    {
        var entry = notifications.FirstOrDefault(n => n.Id == id);
        if (entry is null)
            return;
        notifications.Remove(entry);
        RecalculateUnread();
    }

    public void ClearAll()
    {
        notifications.Clear();
        RecalculateUnread();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        toggle = e.NameScope.Find<Button>(TogglePartName);
        popup = e.NameScope.Find<Popup>(PopupPartName);
        itemsHost = e.NameScope.Find<ItemsControl>(ItemsHostPartName);
        unreadBadgeHost = e.NameScope.Find<Border>(UnreadBadgePartName);
        unreadBadgeText = e.NameScope.Find<TextBlock>(UnreadBadgeTextPartName);

        if (toggle is not null)
            toggle.Click += (_, _) => IsPanelOpen = !IsPanelOpen;
        if (popup is not null)
        {
            popup.PlacementTarget = toggle;
            popup.Placement = PlacementMode.BottomEdgeAlignedLeft;
            popup.HorizontalOffset = GetIconLeftInset();
            popup.Closed += OnPopupClosed;
        }

        if (itemsHost is not null)
            itemsHost.ItemsSource = notifications;

        SyncPopupOpen();
        UpdateUnreadBadge();
    }

    private void OnPopupClosed(object? sender, EventArgs e)
    {
        if (IsPanelOpen)
            SetCurrentValue(IsPanelOpenProperty, false);
    }

    private void OnPanelOpenChanged(bool open) => SyncPopupOpen();

    private void SyncPopupOpen()
    {
        if (popup is not null)
            popup.IsOpen = IsPanelOpen;
    }

    private void OnNotificationsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RecalculateUnread();

    private void RecalculateUnread()
    {
        UnreadCount = notifications.Count(n => !n.IsRead);
        UpdateUnreadBadge();
    }

    private void UpdateUnreadBadge()
    {
        if (unreadBadgeHost is null || unreadBadgeText is null)
            return;
        unreadBadgeText.Text = UnreadCount > 99 ? "99+" : UnreadCount.ToString();
        unreadBadgeHost.IsVisible = UnreadCount > 0;
    }

    /// <summary>Matches toggle padding + icon inset in the notification theme template.</summary>
    private static double GetIconLeftInset() => 12;
}
