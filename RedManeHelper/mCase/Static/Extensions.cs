using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mCASE_ADMIN.DataAccess.mCase.Static
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
                : propertyValue.GetPropertyNameFromSystemName();
        }

        public static string ParseToken(this JToken token, string property)
        {
            var fieldValue = token[property]?.Parent?.FirstOrDefault()?.Value<string>();

            if (!string.IsNullOrEmpty(fieldValue) && string.Equals("SystemName", property, StringComparison.OrdinalIgnoreCase) || string.Equals("FieldOptions", property, StringComparison.OrdinalIgnoreCase) || string.Equals(ListTransferFields.DynamicSourceSystemName.GetDescription(), property, StringComparison.OrdinalIgnoreCase))
                return fieldValue;

            return string.IsNullOrEmpty(fieldValue)
                ? string.Empty
                : fieldValue.GetPropertyNameFromSystemName();
        }

        public static Dictionary<string, string> GetFieldOptions(this JToken token)
        {
            var fieldOptions = token[ListTransferFields.FieldOptions.GetDescription()]?.Parent?.FirstOrDefault()?.Value<string>();

            return string.IsNullOrEmpty(fieldOptions)
                ? new Dictionary<string, string>()
                : JsonConvert.DeserializeObject<Dictionary<string, string>>(fieldOptions);
        }

        public static List<string> ParseDefaultData(this JToken token, string property)
        {
            var fieldValue = token[property];

            if (fieldValue == null)
                return new List<string>();

            var hasValues = fieldValue.HasValues;

            if (!hasValues) return new List<string>();

            var values = fieldValue.Value<JToken>();

            if (values == null) return new List<string>();

            var returnValues = new List<string>();

            foreach (var value in values)
            {
                if (!value.HasValues) continue;

                var childValues = value.Value<JToken>();

                if (childValues == null) continue;

                //get value
                var actualValue = childValues[ListTransferFields.Value.GetDescription()];

                if (actualValue == null) continue;

                var valueObject = actualValue.FirstOrDefault();

                if (valueObject == null) continue;

                var x = valueObject[ListTransferFields.Value.GetDescription()]?.Value<string>();

                if (string.IsNullOrEmpty(x)) continue;

                returnValues.Add(x);

            }

            return returnValues;

        }

        /// <summary>
        /// if first char is a num / the whole word is a num, then a f_ is inserted beforehand.
        /// The final product is a word where the first char is uppercase, and everything else is lower case
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetPropertyNameFromSystemName(this string input)
        {
            input = Regex.Replace(input, @"[^\w]", "");

            var inputIsNumber = int.TryParse(input, out _);

            if (inputIsNumber)
                return "f_" + input;

            var firstCharIsNum = int.TryParse(input[0].ToString(), out _);

            if (firstCharIsNum)
            {
                input = "f_" + input;
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

        public static string ParseDynamicData(this JToken token, string prop1)
        {
            var property = token[prop1];

            if (property == null || !property.HasValues) return string.Empty;

            var property2 = property.ParseToken(ListTransferFields.DynamicSourceSystemName.GetDescription());

            return property2;
        }

        public static bool IsMirrorField(this JToken jToken)
        {
            var defaultValue = jToken.ParseToken(ListTransferFields.DefaultValue.GetDescription());

            var isCoalesce = !string.IsNullOrEmpty(defaultValue) && defaultValue.ToLower().Contains("coalesce");//, StringComparison.OrdinalIgnoreCase);

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
                { 4, "                  " },
                { 5, "                      " },
            };
            return dict[level];
        }

    }
}
