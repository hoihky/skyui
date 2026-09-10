using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace SkyUI.Controls;

/// <summary>Dashboard metric card with value, delta, and optional sparkline content.</summary>
public class SkyKpiTile : TemplatedControl
{
    public const string SparklinePartName = "PART_SparklinePresenter";

    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<SkyKpiTile, string?>(nameof(Label));

    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<SkyKpiTile, object?>(nameof(Value));

    public static readonly StyledProperty<IDataTemplate?> ValueTemplateProperty =
        AvaloniaProperty.Register<SkyKpiTile, IDataTemplate?>(nameof(ValueTemplate));

    public static readonly StyledProperty<string?> DeltaProperty =
        AvaloniaProperty.Register<SkyKpiTile, string?>(nameof(Delta));

    public static readonly StyledProperty<SkyKpiDeltaTrend> DeltaTrendProperty =
        AvaloniaProperty.Register<SkyKpiTile, SkyKpiDeltaTrend>(nameof(DeltaTrend));

    public static readonly StyledProperty<object?> SparklineContentProperty =
        AvaloniaProperty.Register<SkyKpiTile, object?>(nameof(SparklineContent));

    public static readonly StyledProperty<IDataTemplate?> SparklineTemplateProperty =
        AvaloniaProperty.Register<SkyKpiTile, IDataTemplate?>(nameof(SparklineTemplate));

    static SkyKpiTile()
    {
        DeltaTrendProperty.Changed.AddClassHandler<SkyKpiTile>((tile, _) => tile.SyncDeltaTrendClass());
    }

    public SkyKpiTile()
    {
        Classes.Add("sky");
        Classes.Add("sky-kpi-tile");
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IDataTemplate? ValueTemplate
    {
        get => GetValue(ValueTemplateProperty);
        set => SetValue(ValueTemplateProperty, value);
    }

    public string? Delta
    {
        get => GetValue(DeltaProperty);
        set => SetValue(DeltaProperty, value);
    }

    public SkyKpiDeltaTrend DeltaTrend
    {
        get => GetValue(DeltaTrendProperty);
        set => SetValue(DeltaTrendProperty, value);
    }

    public object? SparklineContent
    {
        get => GetValue(SparklineContentProperty);
        set => SetValue(SparklineContentProperty, value);
    }

    public IDataTemplate? SparklineTemplate
    {
        get => GetValue(SparklineTemplateProperty);
        set => SetValue(SparklineTemplateProperty, value);
    }

    private void SyncDeltaTrendClass()
    {
        Classes.Set("sky-kpi-tile-delta-positive", DeltaTrend == SkyKpiDeltaTrend.Positive);
        Classes.Set("sky-kpi-tile-delta-negative", DeltaTrend == SkyKpiDeltaTrend.Negative);
        Classes.Set("sky-kpi-tile-delta-neutral", DeltaTrend == SkyKpiDeltaTrend.Neutral);
    }
}
