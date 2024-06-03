using Silk.NET.Maths;

namespace Autopalya;

public static class BoxHelper
{
    public static bool Overlaps(this Box3D<float> b1, Box3D<float> other)
    {
        if (b1.Max.X < other.Min.X || b1.Min.X > other.Max.X) return false;
        if (b1.Max.Y < other.Min.Y || b1.Min.Y > other.Max.Y) return false;
        if (b1.Max.Z < other.Min.Z || b1.Min.Z > other.Max.Z) return false;
        return true;
    }
}