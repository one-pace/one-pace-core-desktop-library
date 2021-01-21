using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OnePaceCore.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<T> GetFlags<T>(this T e) where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().Where(i => e.HasFlag(i));
        }
        public static string ToDescriptionString<TEnum>(this TEnum val) where TEnum : Enum
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
                .GetType()
                .GetField(val.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
        public static IEnumerable<string> GetDescriptionStrings<TEnum>() where TEnum : Enum
        {
            return typeof(TEnum).GetEnumValues().OfType<TEnum>().ToList().Select(i => i.ToDescriptionString());
        }
        public static bool IsValidDescriptionString<TEnum>(string descriptionString) where TEnum : Enum
        {
            return GetDescriptionStrings<TEnum>().Contains(descriptionString);
        }
        public static TEnum GetValueFromDescriptionString<TEnum>(string descriptionString) where TEnum : Enum
        {
            foreach (TEnum enumItem in typeof(TEnum).GetEnumValues())
            {
                if (descriptionString == enumItem.ToDescriptionString())
                {
                    return enumItem;
                }
            }
            throw new ArgumentException();
        }
    }
}
