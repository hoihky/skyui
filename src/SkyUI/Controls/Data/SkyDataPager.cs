using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace SkyUI.Controls;

/// <summary>Full-featured pager with page size selection and range summary for grids and lists.</summary>
public class SkyDataPager : SkyPaginationBase
{
    public const string PageSizeComboPartName = "PART_PageSizeCombo";
    public const string FirstButtonPartName = "PART_FirstButton";
    public const string PreviousButtonPartName = "PART_PreviousButton";
    public const string NextButtonPartName = "PART_NextButton";
    public const string LastButtonPartName = "PART_LastButton";

    public static readonly StyledProperty<IEnumerable> PageSizesProperty =
        AvaloniaProperty.Register<SkyDataPager, IEnumerable>(nameof(PageSizes), new[] { 10, 25, 50, 100 });

    public static readonly StyledProperty<string?> PageSizeLabelProperty =
        AvaloniaProperty.Register<SkyDataPager, string?>(nameof(PageSizeLabel), "Page size");

    public static readonly StyledProperty<string> SummaryFormatProperty =
        AvaloniaProperty.Register<SkyDataPager, string>(nameof(SummaryFormat), "Showing {0}–{1} of {2}");

    public static readonly DirectProperty<SkyDataPager, string> SummaryTextProperty =
        AvaloniaProperty.RegisterDirect<SkyDataPager, string>(
            nameof(SummaryText),
            pager => pager.SummaryText);

    private ComboBox? pageSizeCombo;
    private string summaryText = string.Empty;

    static SkyDataPager()
    {
        SummaryFormatProperty.Changed.AddClassHandler<SkyDataPager>((pager, _) => pager.UpdateSummaryText());
        CurrentPageProperty.Changed.AddClassHandler<SkyDataPager>((pager, _) => pager.UpdateSummaryText());
        PageSizeProperty.Changed.AddClassHandler<SkyDataPager>((pager, _) => pager.UpdateSummaryText());
        TotalCountProperty.Changed.AddClassHandler<SkyDataPager>((pager, _) => pager.UpdateSummaryText());
        RangeStartProperty.Changed.AddClassHandler<SkyDataPager>((pager, _) => pager.UpdateSummaryText());
        RangeEndProperty.Changed.AddClassHandler<SkyDataPager>((pager, _) => pager.UpdateSummaryText());
    }

    public SkyDataPager()
    {
        Classes.Add("sky");
        Classes.Add("sky-data-pager");
    }

    public IEnumerable PageSizes
    {
        get => GetValue(PageSizesProperty);
        set => SetValue(PageSizesProperty, value);
    }

    public string? PageSizeLabel
    {
        get => GetValue(PageSizeLabelProperty);
        set => SetValue(PageSizeLabelProperty, value);
    }

    public string SummaryFormat
    {
        get => GetValue(SummaryFormatProperty);
        set => SetValue(SummaryFormatProperty, value);
    }

    public string SummaryText => summaryText;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PageSizeProperty && pageSizeCombo is not null)
            pageSizeCombo.SelectedItem = PageSize;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (pageSizeCombo is not null)
            pageSizeCombo.SelectionChanged -= OnPageSizeSelectionChanged;

        pageSizeCombo = e.NameScope.Find(PageSizeComboPartName) as ComboBox;
        if (pageSizeCombo is not null)
        {
            pageSizeCombo.ItemsSource = PageSizes;
            pageSizeCombo.SelectedItem = PageSize;
            pageSizeCombo.SelectionChanged += OnPageSizeSelectionChanged;
        }

        UpdateSummaryText();
    }

    private void OnPageSizeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (pageSizeCombo?.SelectedItem is not int selectedSize || selectedSize == PageSize)
            return;

        PageSize = selectedSize;
    }

    private void UpdateSummaryText()
    {
        var text = TotalCount <= 0
            ? string.Format(SummaryFormat, 0, 0, 0)
            : string.Format(SummaryFormat, RangeStart, RangeEnd, TotalCount);

        SetAndRaise(SummaryTextProperty, ref summaryText, text);
    }

}
