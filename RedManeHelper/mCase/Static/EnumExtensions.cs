﻿using System.ComponentModel;

namespace DemoKatan.mCase.Static
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var customAttributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customAttributes != null && customAttributes.Length != 0 ? customAttributes[0].Description : value.ToString();
        }

        public static TEnum GetEnumValue<TEnum>(this string description) where TEnum : Enum
        {
            if (string.IsNullOrEmpty(description))
            {
                return default;
            }

            var map = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .ToDictionary(e => e.GetDescription());

            if (map.TryGetValue(description, out var result))
            {
                return result;
            }

            return default;
        }

        public static List<string> GetEnumDescriptions<TEnum>() where TEnum : Enum =>
            Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => e.GetDescription())
                .ToList();
    }
}