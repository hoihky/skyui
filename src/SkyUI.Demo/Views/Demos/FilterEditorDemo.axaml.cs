using Avalonia.Controls;
using SkyUI.FilterEditor;

namespace SkyUI.Demo.Views.Demos;

public partial class FilterEditorDemo : UserControl
{
    public FilterEditorDemo()
    {
        InitializeComponent();

        var doc = new FilterDocument();
        doc.Fields.Add(new FilterFieldDescriptor("Title", "Title", FilterFieldDataKind.String));
        doc.Fields.Add(new FilterFieldDescriptor("DurationMs", "Duration (ms)", FilterFieldDataKind.Number));
        doc.Fields.Add(new FilterFieldDescriptor("Artist.Name", "Artist name", FilterFieldDataKind.String));

        doc.Root.AddCondition("Title", FilterCompareOperator.Contains, "Sky");
        var orGroup = doc.Root.AddGroup(FilterLogicalKind.Or);
        orGroup.AddCondition("DurationMs", FilterCompareOperator.GreaterThan, "180000");
        orGroup.AddCondition("Artist.Name", FilterCompareOperator.Equal, "Various");

        Editor.Document = doc;
    }
}
