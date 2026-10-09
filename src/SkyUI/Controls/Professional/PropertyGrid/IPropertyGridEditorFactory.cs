namespace SkyUI.Controls.Professional;

/// <summary>Creates value editors for property grid rows (strategy / factory pattern).</summary>
public interface IPropertyGridEditorFactory
{
    Avalonia.Controls.Control CreateEditor(PropertyGridItem item);
}
