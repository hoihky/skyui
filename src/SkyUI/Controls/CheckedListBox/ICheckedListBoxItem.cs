using System.Collections;
using System.ComponentModel;

namespace SkyUI.Controls;

/// <summary>
/// Optional ViewModel contract for hierarchical items (MVVM-friendly; ISP: only what the list needs).
/// </summary>
public interface ICheckedListBoxItem : INotifyPropertyChanged
{
    bool? IsChecked { get; set; }

    bool IsExpanded { get; set; }

    /// <summary>Child items; may be null, empty, or a live collection (INotifyCollectionChanged).</summary>
    IEnumerable? Children { get; }
}
