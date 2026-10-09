using System.Windows.Input;
using Avalonia;

namespace SkyUI.Controls;

/// <summary>Persistent notification item hosted by <see cref="SkyNotificationCenter"/>.</summary>
public sealed class SkyNotificationEntry : AvaloniaObject
{
    public static readonly StyledProperty<Guid> IdProperty =
        AvaloniaProperty.Register<SkyNotificationEntry, Guid>(nameof(Id));

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SkyNotificationEntry, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<string?> BodyProperty =
        AvaloniaProperty.Register<SkyNotificationEntry, string?>(nameof(Body));

    public static readonly StyledProperty<DateTimeOffset> TimestampProperty =
        AvaloniaProperty.Register<SkyNotificationEntry, DateTimeOffset>(nameof(Timestamp));

    public static readonly StyledProperty<bool> IsReadProperty =
        AvaloniaProperty.Register<SkyNotificationEntry, bool>(nameof(IsRead));

    public static readonly StyledProperty<ICommand?> ActionCommandProperty =
        AvaloniaProperty.Register<SkyNotificationEntry, ICommand?>(nameof(ActionCommand));

    public SkyNotificationEntry() => Id = Guid.NewGuid();

    public Guid Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public DateTimeOffset Timestamp
    {
        get => GetValue(TimestampProperty);
        set => SetValue(TimestampProperty, value);
    }

    public bool IsRead
    {
        get => GetValue(IsReadProperty);
        set => SetValue(IsReadProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }
}
