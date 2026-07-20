using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SkyUI.Diagram.Model;

namespace SkyUI.Demo.Views.Demos;

public partial class DiagramDemo : UserControl
{
    private readonly DiagramModel _model = new();

    public DiagramDemo()
    {
        InitializeComponent();
        Diagram.Model = _model;
        Seed();
    }

    private void Seed()
    {
        _model.AddNode("step", "Start", new Rect(48, 140, 160, 100));
        _model.AddNode("step", "Process", new Rect(280, 100, 180, 120));
        _model.AddNode("step", "End", new Rect(520, 160, 160, 100));
        var n0 = _model.Nodes[0];
        var n1 = _model.Nodes[1];
        _model.TryAddEdge(n0.Id, "R", n1.Id, "L");
        _model.TryAddEdge(n1.Id, "R", _model.Nodes[2].Id, "L");
    }

    private void OnAddNode(object? sender, RoutedEventArgs e)
    {
        var x = 80 + Random.Shared.Next(0, 320);
        var y = 60 + Random.Shared.Next(0, 180);
        _model.AddNode("step", "New node", new Rect(x, y, 150, 96));
    }

    private void OnClear(object? sender, RoutedEventArgs e)
    {
        while (_model.Nodes.Count > 0)
            _model.RemoveNode(_model.Nodes[0].Id);
    }
}
