---
title: Picker culture formatting
order: 70
---

SkyUI pickers use .NET `CultureInfo` for date patterns, first day of week, and 12/24-hour clocks.

## Controls

| Control | Use for |
|---------|---------|
| `SkyDatePicker` | Text field with calendar dropdown (`CalendarDatePicker`) |
| `SkyCalendar` | Inline month grid |
| `SkyTimePicker` | Hour/minute selection with culture-aware clock |

Set `Culture` on any picker to override the thread culture:

```xml
<sky:SkyDatePicker SelectedDate="{Binding DueDate}"
                   Culture="{Binding AppCulture}" />
```

When `Culture` is null, `CultureInfo.CurrentCulture` is used.

## Formatting (`SkyPickerFormat`)

```csharp
var shortDate = SkyPickerFormat.FormatDate(date, culture);
var longDate = SkyPickerFormat.FormatLongDate(date, culture);
var time = SkyPickerFormat.FormatTime(timeSpan, culture);

SkyPickerFormat.ApplyCulture(datePicker, culture);
SkyPickerFormat.ApplyCulture(calendar, culture);
SkyPickerFormat.ApplyCulture(timePicker, culture);
```

For bindings:

```xml
<TextBlock Text="{Binding DueDate, Converter={x:Static sky:SkyPickerFormatConverter.DateInstance}, ConverterParameter={Binding AppCulture}}" />
```

## What culture changes

| Setting | Source |
|---------|--------|
| Short date pattern | `CultureInfo.DateTimeFormat.ShortDatePattern` |
| Long date pattern | `CultureInfo.DateTimeFormat.LongDatePattern` |
| First day of week | `CultureInfo.DateTimeFormat.FirstDayOfWeek` |
| 12 vs 24-hour clock | Derived from `ShortTimePattern` (`h`/`t` → 12-hour) |
| Calendar language | Day/month names follow thread culture when rendering |

## MVVM example

```csharp
public CultureInfo AppCulture { get; set; } = new CultureInfo("de-DE");
public DateTime? DueDate { get; set; }
public string DueDateLabel => SkyPickerFormat.FormatDate(DueDate, AppCulture);
```

## Related

- [Development roadmap](development-roadmap.md) — Pickers sprint scope
- [Design tokens](design-tokens.md) — picker surface and accent brushes
