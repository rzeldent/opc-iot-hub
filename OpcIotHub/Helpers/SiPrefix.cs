using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpcIotHub.Helpers
{
    public enum SiPrefix : sbyte
    {
        [Description("yocto")]
        [Display(Name = "y")]
        Yocto = -24,
        [Description("zepto")]
        [Display(Name = "z")]
        Zepto = -21,
        [Description("atto")]
        [Display(Name = "a")]
        Atto = -18,
        [Description("femto")]
        [Display(Name = "f")]
        Femto = -15,
        [Description("pico")]
        [Display(Name = "p")]
        Pico = -12,
        [Description("nano")]
        [Display(Name = "n")]
        Nano = -9,
        [Description("micro")]
        [Display(Name = "μ")]
        Micro = -6,
        [Description("milli")]
        [Display(Name = "m")]
        Milli = -3,
        [Description("kilo")]
        [Display(Name = "k")]
        Kilo = 3,
        [Description("mega")]
        [Display(Name = "M")]
        Mega = 6,
        [Description("giga")]
        [Display(Name = "G")]
        Giga = 9,
        [Description("tera")]
        [Display(Name = "T")]
        Tera = 12,
        [Description("penta")]
        [Display(Name = "P")]
        Penta = 15,
        [Description("exa")]
        [Display(Name = "E")]
        Exa = 18,
        [Description("zeta")]
        [Display(Name = "Z")]
        Zeta = 21,
        [Description("yotta")]
        [Display(Name = "Y")]
        Yotta = 24
    }
}