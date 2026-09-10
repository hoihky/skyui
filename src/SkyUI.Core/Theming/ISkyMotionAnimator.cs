using Avalonia;

namespace SkyUI.Core.Theming;

/// <summary>Animates common enter/exit transitions using Sky motion tokens.</summary>
public interface ISkyMotionAnimator
{
    Task FadeAsync(Visual target, double from, double to, TimeSpan duration, CancellationToken cancellationToken = default);

    Task ScaleAsync(Visual target, double from, double to, TimeSpan duration, CancellationToken cancellationToken = default);

    Task TranslateYAsync(Visual target, double from, double to, TimeSpan duration, CancellationToken cancellationToken = default);

    Task TranslateXAsync(Visual target, double from, double to, TimeSpan duration, CancellationToken cancellationToken = default);
}
