using Newtonsoft.Json.Linq;

namespace DemoKatan.mCase.Static
{
    public static class Extensions
    {
        public static string MCaseDateTimeStorageFormat => "yyyy-MM-dd HH:mm:ss";

        public static string ParseJson(this JObject jObject, string property)
        {
            var propertyValue = jObject[property]?.Parent?.FirstOrDefault()?.Value<string>();

            if (string.Equals("SystemName", property) && !string.IsNullOrEmpty(propertyValue)) 
                return propertyValue;
            
            return string.IsNullOrEmpty(propertyValue)
                ? string.Empty
                : propertyValue.CleanString();
        }

        public static string ParseToken(this JToken token, string property)
        {
            var fieldValue = token[property]?.Parent?.FirstOrDefault()?.Value<string>();

            if (string.Equals("SystemName", property) && !string.IsNullOrEmpty(fieldValue))
                return fieldValue;

            return string.IsNullOrEmpty(fieldValue)
                ? string.Empty
                : fieldValue.CleanString();
        }

        public static List<string> ParseChildren(this JToken token, string property)
        {
            var containers = token.Children();

            var relationships = new List<string>();
            
            foreach (var child in containers)
            {
                var result = child.ParseToken(property);

                if(!string.IsNullOrEmpty(result))
                    relationships.Add(result);
            }

            return relationships;
        }

        public static string ParseDynamicData(this JToken token, string prop1, string prop2)
        {
            var property = token[prop1];

            if(property == null || !property.HasValues) return string.Empty;

            var property2= property.ParseToken(prop2);

            return property2 ?? string.Empty;
        }

        private static bool IsNum(this char c) => int.TryParse(c.ToString(), out var num);

        private static bool IsLetter(this char c)
        {
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            return letters.Contains(c);
        }

        public static string CleanString(this string input)
        {
            var name = string.Empty;
            var num = string.Empty;
            var firstChar = true;
            foreach (var c in input)
            {
                if (c.IsNum())
                {
                    num += c;
                    continue;
                }

                if (!c.IsLetter())
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
    }
}
