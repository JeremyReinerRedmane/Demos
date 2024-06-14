using System.ComponentModel;

namespace DemoKatan.mCase.Static
{
    public static class MCaseStringExtensions
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

            if (map.TryGetValue(description, out TEnum result))
            {
                return result;
            }

            return default;
        }
    }
}
