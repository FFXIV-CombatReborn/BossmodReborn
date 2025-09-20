namespace BossMod;

[SkipLocalsInit]
public static class MathExtension
{
    public static bool IsPrime(uint number)
    {
        if (number <= 1u)
            return false;
        if (number == 2u)
            return true;
        for (var i = 2u; i <= Math.Sqrt(number); ++i)
        {
            if (number % i == default)
                return false;
        }
        return true;
    }

    public static bool IsDivisible(uint dividend, uint divisor)
    {
        return dividend % divisor == default;
    }
}
