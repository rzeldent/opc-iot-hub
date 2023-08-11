using System;

namespace OpcIotHub.Helpers
{
    public static class SiSystem
    {
        public static string ToString(double value)
        {
            var power = Math.Log10(value);
            var prefix = (SiPrefix)(Math.Floor(power / 3) * 3);
            return prefix != 0 ? $"{value / Math.Pow(10, (double)prefix)}{prefix.GetDisplay()}" : $"{value}";
        }
    }
}
