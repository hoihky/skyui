using System.Windows.Input;

namespace SkyUI.Controls;

/// <summary>Segment in <see cref="SkyBreadcrumb"/>.</summary>
public class SkyBreadcrumbItem
{
    public string? Title { get; set; }

    public bool IsCurrent { get; set; }

    public ICommand? Command { get; set; }
}
