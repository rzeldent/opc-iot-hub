using System;
using System.ComponentModel;

namespace OpcAzureIot
{
    public enum SiPrefix : sbyte
        {
            Yocto = -24,
            Zepto = -21,
            Atto = -18,
            Femto = -15,
            Pico = -12,
            Nano = -9,
            Micro = -6,
            Milli = -3,
            Centi = -2,
            Deci = -1,
            None = 0,
            Deka = 1,
            Hecto = 2,
            Kilo = 3,
            Mega = 6,
            Giga = 9,
            Tera = 12,
            Penta = 15,
            Exa = 18,
            Zeta = 21,
            Yotta = 24
        }

    public static class SiPrefixHelpers
    {

        public static SiPrefix Parse(string value)
        {
            return value !=null ? (SiPrefix)Enum.Parse(typeof(SiPrefix), value): SiPrefix.None;
        }

        public static string Name(this SiPrefix prefix)
        {
            switch (prefix)
            {
                case SiPrefix.Yocto:
                    return "yocto";
                case SiPrefix.Zepto:
                    return "zepto";
                case SiPrefix.Atto:
                    return "atto";
                case SiPrefix.Femto:
                    return "femto";
                case SiPrefix.Pico:
                    return "pico";
                case SiPrefix.Nano:
                    return "nano";
                case SiPrefix.Micro:
                    return "micro";
                case SiPrefix.Milli:
                    return "milli";
                case SiPrefix.Centi:
                    return "centi";
                case SiPrefix.Deci:
                    return "deci";
                case SiPrefix.None:
                    return string.Empty;
                case SiPrefix.Deka:
                    return "deka";
                case SiPrefix.Hecto:
                    return "hecto";
                case SiPrefix.Kilo:
                    return "kilo";
                case SiPrefix.Mega:
                    return "mega";
                case SiPrefix.Giga:
                    return "giga";
                case SiPrefix.Tera:
                    return "tera";
                case SiPrefix.Penta:
                    return "penta";
                case SiPrefix.Exa:
                    return "exa";
                case SiPrefix.Zeta:
                    return "zeta";
                case SiPrefix.Yotta:
                    return "yotta";
            }

            throw new InvalidEnumArgumentException(nameof(SiPrefix), (int) prefix, typeof(SiPrefix));
        }
    }
}