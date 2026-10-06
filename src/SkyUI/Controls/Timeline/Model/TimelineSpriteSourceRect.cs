namespace SkyUI.Controls.Timeline.Model;

/// <summary>Atlas region in pixel coordinates (optional; host interprets space).</summary>
public readonly struct TimelineSpriteSourceRect : IEquatable<TimelineSpriteSourceRect>
{
    public TimelineSpriteSourceRect(double x, double y, double width, double height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public double X { get; }

    public double Y { get; }

    public double Width { get; }

    public double Height { get; }

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public bool Equals(TimelineSpriteSourceRect other) =>
        X.Equals(other.X)
        && Y.Equals(other.Y)
        && Width.Equals(other.Width)
        && Height.Equals(other.Height);

    public override bool Equals(object? obj) => obj is TimelineSpriteSourceRect other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
}
