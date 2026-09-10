namespace SkyUI.Controls;

/// <summary>Immutable start/end pair produced by <see cref="SkyDateRangePicker"/>.</summary>
public sealed record SkyDateRangeValue(DateTime? Start, DateTime? End);
