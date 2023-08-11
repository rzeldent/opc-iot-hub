using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpcIotHub.Helpers
{
    public enum SiUnit : byte
    {
        // 7 base units
        [Description("metre")]
        [Display(Name = "m")]
        Length,
        [Description("second")]
        [Display(Name = "s")]
        Time,
        [Description("mole")]
        [Display(Name = "mole")]
        AmountOfSubstance,
        [Description("Ampere")]
        [Display(Name = "A")]
        Current,
        [Description("Kelvin")]
        [Display(Name = "K")]
        Temperature,
        [Description("candela")]
        [Display(Name = "cd")]
        LuminousIntensity,
        [Description("kilogram")]
        [Display(Name = "kg")]
        Mass,

        // Derived units
        [Description("Pascal")]
        [Display(Name = "Pa")]
        Pressure
    }
}