using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpcAzureIot
{
    public enum SiUnit : byte
    {
        [Display(Name="")]
        None = 0,
        // 7 base units
        Length,
        Time,
        AmountOfSubstance,
        Current,
        Temperature,
        LuminousIntensity,
        Mass,
        // Derived units
        Pressure
    }

    public static class SiUnitHelpers
    {
        // https://www.nist.gov/pml/weights-and-measures/metric-si-prefixes

        public static SiUnit Parse(string value)
        {
            return value != null ? (SiUnit)Enum.Parse(typeof(SiUnit), value) : SiUnit.None;
        }

        public  static string Name(this SiUnit unit)
        {
            switch (unit)
            {
                case SiUnit.None:
                    return string.Empty;
                case SiUnit.Length:
                    return "metre";
                case SiUnit.Time:
                    return "second";
                case SiUnit.AmountOfSubstance:
                    return "mole";
                case SiUnit.Current:
                    return "ampere";
                case SiUnit.Temperature:
                    return "kelvin";
                case SiUnit.LuminousIntensity:
                    return "candlela";
                case SiUnit.Mass:
                    return "kilogram";

                case SiUnit.Pressure:
                    return "pascal";
            }

            throw new InvalidEnumArgumentException(nameof(SiUnit), (int)unit, typeof(SiUnit));

        }

        public static string Symbol(this SiUnit unit)
        {
            switch (unit)
            {
                case SiUnit.None:
                    return string.Empty;
                case SiUnit.Length:
                    return "m";
                case SiUnit.Time:
                    return "s";
                case SiUnit.AmountOfSubstance:
                    return "mole";
                case SiUnit.Current:
                    return "A";
                case SiUnit.Temperature:
                    return "K";
                case SiUnit.LuminousIntensity:
                    return "cd";
                case SiUnit.Mass:
                    return "kg";

                case SiUnit.Pressure:
                    return "Pa";
            }

            throw new InvalidEnumArgumentException(nameof(SiUnit), (int)unit, typeof(SiUnit));
        }

    }
}