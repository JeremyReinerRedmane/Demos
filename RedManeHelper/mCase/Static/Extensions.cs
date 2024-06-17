using Newtonsoft.Json.Linq;

namespace DemoKatan.mCase.Static
{
    public static class Extensions
    {
        public static string MCaseDateTimeStorageFormat => "yyyy-MM-dd HH:mm:ss";
        public static string TimeFormat => "yyyy_MM_dd_HH_mm_ss";

        public static string ParseJson(this JObject jObject, string property)
        {
            var propertyValue = jObject[property]?.Parent?.FirstOrDefault()?.Value<string>();

            if (string.Equals("SystemName", property, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(propertyValue)) 
                return propertyValue;
            
            return string.IsNullOrEmpty(propertyValue)
                ? string.Empty
                : propertyValue.CleanString();
        }

        public static string ParseToken(this JToken token, string property)
        {
            var fieldValue = token[property]?.Parent?.FirstOrDefault()?.Value<string>();

            if (string.Equals("SystemName", property, StringComparison.OrdinalIgnoreCase) || string.Equals("FieldOptions", property, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(fieldValue))
                return fieldValue;

            return string.IsNullOrEmpty(fieldValue)
                ? string.Empty
                : fieldValue.CleanString();
        }

        public static string GetPropertyNameFromSystemName(this string input)
        {
            var inputIsNumber = int.TryParse(input, out _);

            if (inputIsNumber)
                return "f_" + input;

            var firstCharIsNum = int.TryParse(input[0].ToString(), out _);

            if (firstCharIsNum)
            {
                var temp = input[0];
                var updated = input.Remove(0, 1);
                updated += temp;
                return updated.GetPropertyNameFromSystemName();
            }

            var c = input[0];
            char.TryParse(c.ToString().ToUpperInvariant(), out var cap);
            var titleCase = input.Substring(1).ToLower();
            return cap + titleCase;
        }

        public static List<string> ParseChildren(this JToken token, string property)
        {
            var containers = token.Children();

            return containers
                .Select(child => child.ParseToken(property))
                .Where(result => !string.IsNullOrEmpty(result))
                .ToList();
        }

        public static string ParseDynamicData(this JToken token, string prop1, string prop2)
        {
            var property = token[prop1];

            if(property == null || !property.HasValues) return string.Empty;

            var property2= property.ParseToken(prop2);

            return property2 ?? string.Empty;
        }

        public static string CleanString(this string input)
        {
            var name = string.Empty;
            var num = string.Empty;
            var firstChar = true;
            foreach (var c in input)
            {
                if (char.IsDigit(c))
                {
                    num += c;
                    continue;
                }

                if (!char.IsLetter(c))
                    continue;

                if (firstChar)
                {
                    name += c.ToString().ToUpper();
                    firstChar = false;
                }
                else
                    name += c.ToString().ToLower();
            }

            return name + num;
        }

        public static bool IsMirrorField(this JToken jToken)
        {
            var defaultValue = jToken.ParseToken(ListTransferFields.DefaultValue.GetDescription());

            var isCoalesce = !string.IsNullOrEmpty(defaultValue) && defaultValue.Contains("COALESCE", StringComparison.OrdinalIgnoreCase);

            return isCoalesce;
        }

        public static string Indent(this int level)
        {
            var dict = new Dictionary<int, string>()
            {
                { 0, "   " },
                { 1, "       " },
                { 2, "           " },
                { 3, "               " },
            };
            return dict[level];
        }

    }
}
