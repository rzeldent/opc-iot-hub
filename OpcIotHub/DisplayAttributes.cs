using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace OpcIotHub
{
    public static class DisplayAttributes
    {
        public static T? ParseNullable<T>(this string value) where T : struct
        {
            if (!string.IsNullOrEmpty(value))
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.IsValid(value))
                    return (T)converter.ConvertFromString(value);

                if (typeof(T).IsEnum)
                {
                    if (Enum.TryParse(value, out T t))
                        return t;
                }
            }

            return null;
        }

        private static TAttribute GetAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
        {
            return enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttribute<TAttribute>();
        }

        // [Display(Name = "Something To Name")]
        public static string GetDisplay(this Enum enumValue)
        {
            return GetAttribute<DisplayAttribute>(enumValue).Name;
        }

        // [Description = "Something To Describe")]
        public static string GetDescription<T>(this Enum enumValue)
        {
            return GetAttribute<DescriptionAttribute>(enumValue).Description;
        }

        // [DisplayName = "Something To Name")]
        public static string GetDisplayName<T>(this Enum enumValue)
        {
            return GetAttribute<DisplayNameAttribute>(enumValue).DisplayName;
        }
    }
}
