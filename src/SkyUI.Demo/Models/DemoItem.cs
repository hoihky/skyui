using System;
using Avalonia.Controls;
using SkyUI.Icons;

namespace SkyUI.Demo.Models;

public sealed class DemoItem
{
    public DemoItem(string title, Func<Control> createView, SkyIconKind icon = SkyIconKind.LayoutGrid)
    {
        Title = title;
        CreateView = createView;
        Icon = icon;
    }

    public string Title { get; }
    public SkyIconKind Icon { get; }
    public Func<Control> CreateView { get; }
}
