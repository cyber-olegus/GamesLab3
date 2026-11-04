namespace GamesLab3.Core.Common;
internal static class Guard
{
    public static double Positive(double value, string name)
    {
        if (!double.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(name);
        return value;
    }
    public static double Delta(double value)
    {
        if (!double.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        return value;
    }
}
