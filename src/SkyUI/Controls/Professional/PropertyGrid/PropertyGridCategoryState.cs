namespace SkyUI.Controls.Professional;

/// <summary>Tracks expand/collapse for a property grid category section.</summary>
public sealed class PropertyGridCategoryState
{
    public PropertyGridCategoryState(string name) => Name = name;

    public string Name { get; }

    public bool IsExpanded { get; set; } = true;
}
