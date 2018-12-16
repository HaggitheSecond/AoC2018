namespace AoC2018.Extensions
{
    public static class IntExtensions
    {
        public static string FormatForDisplay(this int self)
        {
            return self >= 1000 ? self.ToString()
                : self >= 100 ? " " + self
                : self >= 10 ? "  " + self
                : "   " + self;
        }
    }
}