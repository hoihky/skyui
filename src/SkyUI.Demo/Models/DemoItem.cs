using System;
using Avalonia.Controls;

namespace SkyUI.Demo.Models;

public sealed class DemoItem
{
    public DemoItem(string title, Func<Control> createView)
    {
        Title = title;
        CreateView = createView;
    }

    public string Title { get; }
    public Func<Control> CreateView { get; }
}
