using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;

namespace SkyUI.FilterEditor;

/// <summary>
/// Hierarchical filter criteria editor (DevExpress-style): logical groups, conditions, optional SQL preview,
/// and pluggable <see cref="IFilterSqlExporter"/>.
/// </summary>
public class FilterEditor : TemplatedControl
{
    public static readonly StyledProperty<FilterDocument?> DocumentProperty =
        AvaloniaProperty.Register<FilterEditor, FilterDocument?>(nameof(Document));

    public static readonly StyledProperty<IFilterSqlExporter?> SqlExporterProperty =
        AvaloniaProperty.Register<FilterEditor, IFilterSqlExporter?>(nameof(SqlExporter));

    public static readonly StyledProperty<bool> ShowSqlPreviewProperty =
        AvaloniaProperty.Register<FilterEditor, bool>(nameof(ShowSqlPreview), true);

    private FilterEditorViewModel? _viewModel;

    static FilterEditor()
    {
        DocumentProperty.Changed.AddClassHandler<FilterEditor>((o, _) => o.OnDocumentOrExporterChanged());
        SqlExporterProperty.Changed.AddClassHandler<FilterEditor>((o, _) => o.OnDocumentOrExporterChanged());
    }

    public FilterDocument? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public IFilterSqlExporter? SqlExporter
    {
        get => GetValue(SqlExporterProperty);
        set => SetValue(SqlExporterProperty, value);
    }

    public bool ShowSqlPreview
    {
        get => GetValue(ShowSqlPreviewProperty);
        set => SetValue(ShowSqlPreviewProperty, value);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        TearDownViewModel();
        base.OnDetachedFromLogicalTree(e);
    }

    private void OnDocumentOrExporterChanged()
    {
        var doc = Document;
        if (doc is null)
        {
            TearDownViewModel();
            DataContext = null;
            return;
        }

        if (_viewModel is null || !ReferenceEquals(_viewModel.Document, doc))
        {
            TearDownViewModel();
            _viewModel = new FilterEditorViewModel(doc, SqlExporter);
            DataContext = _viewModel;
        }
        else if (SqlExporter is not null)
        {
            _viewModel.SetSqlExporter(SqlExporter);
        }
        else
        {
            _viewModel.SetSqlExporter(new BasicFilterSqlExporter());
        }
    }

    private void TearDownViewModel()
    {
        _viewModel?.Dispose();
        _viewModel = null;
    }
}
