using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;

namespace SkyUI.Core.Theming;

/// <summary>Default <see cref="ISkyMotionAnimator"/> using Avalonia keyframe animations.</summary>
public sealed class SkyMotionAnimator : ISkyMotionAnimator
{
    public static ISkyMotionAnimator Default { get; } = new SkyMotionAnimator();

    public Task FadeAsync(
        Visual target,
        double from,
        double to,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        if (duration <= TimeSpan.Zero)
        {
            target.Opacity = to;
            return Task.CompletedTask;
        }

        return RunAsync(target, duration, Visual.OpacityProperty, from, to, cancellationToken);
    }

    public Task ScaleAsync(
        Visual target,
        double from,
        double to,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var scale = EnsureScaleTransform(target);
        if (duration <= TimeSpan.Zero)
        {
            scale.ScaleX = to;
            scale.ScaleY = to;
            return Task.CompletedTask;
        }

        return RunAsync(scale, duration, ScaleTransform.ScaleXProperty, from, to, cancellationToken, scale);
    }

    public Task TranslateYAsync(
        Visual target,
        double from,
        double to,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var translate = EnsureTranslateTransform(target);
        if (duration <= TimeSpan.Zero)
        {
            translate.Y = to;
            return Task.CompletedTask;
        }

        return RunAsync(translate, duration, TranslateTransform.YProperty, from, to, cancellationToken);
    }

    public Task TranslateXAsync(
        Visual target,
        double from,
        double to,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var translate = EnsureTranslateTransform(target);
        if (duration <= TimeSpan.Zero)
        {
            translate.X = to;
            return Task.CompletedTask;
        }

        return RunAsync(translate, duration, TranslateTransform.XProperty, from, to, cancellationToken);
    }

    private static ScaleTransform EnsureScaleTransform(Visual target)
    {
        if (target.RenderTransform is ScaleTransform existing)
            return existing;

        var scale = new ScaleTransform(1, 1);
        target.RenderTransform = scale;
        target.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        return scale;
    }

    private static TranslateTransform EnsureTranslateTransform(Visual target)
    {
        if (target.RenderTransform is TranslateTransform existing)
            return existing;

        var translate = new TranslateTransform();
        target.RenderTransform = translate;
        return translate;
    }

    private static async Task RunAsync(
        Animatable target,
        TimeSpan duration,
        AvaloniaProperty property,
        double from,
        double to,
        CancellationToken cancellationToken,
        ScaleTransform? scaleTransform = null)
    {
        var fromValue = (double)from;
        var toValue = (double)to;

        if (duration <= TimeSpan.Zero)
        {
            target.SetValue(property, toValue);
            if (scaleTransform is not null)
                scaleTransform.ScaleY = toValue;

            return;
        }

        var animation = new Animation
        {
            Duration = duration,
            FillMode = FillMode.Forward,
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters = { new Setter(property, fromValue) },
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters = { new Setter(property, toValue) },
                },
            },
        };

        try
        {
            await animation.RunAsync(target, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!cancellationToken.IsCancellationRequested)
            target.SetValue(property, toValue);

        if (scaleTransform is not null)
            scaleTransform.ScaleY = toValue;
    }
}
