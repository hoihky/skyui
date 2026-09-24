using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace SkyUI.Controls;

/// <summary>Applies safe-area offsets to a floating control anchored to bottom edges.</summary>
internal sealed class SkyFabInsetsCoordinator
{
    private readonly Control target;
    private IInsetsManager? insetsManager;
    private Thickness edgeMargin;

    public SkyFabInsetsCoordinator(Control target) => this.target = target;

    public bool HonorSafeArea { get; set; } = true;

    public Thickness EdgeMargin
    {
        get => edgeMargin;
        set
        {
            edgeMargin = value;
            Apply();
        }
    }

    public void Attach()
    {
        Detach();
        insetsManager = TopLevel.GetTopLevel(target)?.InsetsManager;
        if (insetsManager is null)
        {
            Apply();
            return;
        }

        insetsManager.SafeAreaChanged += OnSafeAreaChanged;
        Apply();
    }

    public void Detach()
    {
        if (insetsManager is null)
            return;

        insetsManager.SafeAreaChanged -= OnSafeAreaChanged;
        insetsManager = null;
    }

    private void OnSafeAreaChanged(object? sender, EventArgs e) => Apply();

    private void Apply()
    {
        var safe = HonorSafeArea ? insetsManager?.SafeAreaPadding ?? default : default;
        var horizontal = target.HorizontalAlignment;
        var vertical = target.VerticalAlignment;

        var left = horizontal == HorizontalAlignment.Left ? edgeMargin.Left + safe.Left : edgeMargin.Left;
        var top = vertical == VerticalAlignment.Top ? edgeMargin.Top + safe.Top : edgeMargin.Top;
        var right = horizontal == HorizontalAlignment.Right ? edgeMargin.Right + safe.Right : edgeMargin.Right;
        var bottom = vertical == VerticalAlignment.Bottom ? edgeMargin.Bottom + safe.Bottom : edgeMargin.Bottom;

        target.Margin = new Thickness(left, top, right, bottom);
    }
}
