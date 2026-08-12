using System.ComponentModel;
using System.Globalization;
using SkyUI.Controls;

namespace SkyUI.Demo.ViewModels;

/// <summary>MVVM sample for date/time pickers and culture-aware formatting.</summary>
public sealed class PickersDemoViewModel : INotifyPropertyChanged
{
    private DateTime? _selectedDate = DateTime.Today;
    private TimeSpan? _selectedTime = new(14, 30, 0);
    private CultureInfo _culture = CultureInfo.CurrentCulture;

    private CultureOption _selectedCulture;

    public PickersDemoViewModel()
    {
        _selectedCulture = Cultures[0];
        _culture = _selectedCulture.Culture;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate == value)
                return;
            _selectedDate = value;
            Notify(nameof(SelectedDate), nameof(FormattedDate), nameof(FormattedLongDate));
        }
    }

    public TimeSpan? SelectedTime
    {
        get => _selectedTime;
        set
        {
            if (_selectedTime == value)
                return;
            _selectedTime = value;
            Notify(nameof(SelectedTime), nameof(FormattedTime));
        }
    }

    public CultureInfo Culture
    {
        get => _culture;
        set
        {
            if (Equals(_culture, value))
                return;
            _culture = value;
            Notify(nameof(Culture), nameof(FormattedDate), nameof(FormattedLongDate), nameof(FormattedTime), nameof(Uses12HourClock), nameof(ClockHint));
        }
    }

    public CultureOption SelectedCulture
    {
        get => _selectedCulture;
        set
        {
            if (ReferenceEquals(_selectedCulture, value) || value is null)
                return;
            _selectedCulture = value;
            Culture = value.Culture;
            Notify(nameof(SelectedCulture));
        }
    }

    public IReadOnlyList<CultureOption> Cultures { get; } =
    [
        new("English (US)", new CultureInfo("en-US")),
        new("German (Germany)", new CultureInfo("de-DE")),
        new("Japanese (Japan)", new CultureInfo("ja-JP")),
    ];

    public string FormattedDate => SkyPickerFormat.FormatDate(SelectedDate, Culture);

    public string FormattedLongDate => SkyPickerFormat.FormatLongDate(SelectedDate, Culture);

    public string FormattedTime => SkyPickerFormat.FormatTime(SelectedTime, Culture);

    public bool Uses12HourClock => SkyPickerFormat.Uses12HourClock(Culture);

    public string ClockHint => Uses12HourClock
        ? "12-hour clock for selected culture"
        : "24-hour clock for selected culture";

    private void Notify(params string[] names)
    {
        foreach (var name in names)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed record CultureOption(string Label, CultureInfo Culture);
}
