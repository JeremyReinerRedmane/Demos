﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using mCASE_ADMIN.DataAccess.mCase.Static;
using Newtonsoft.Json.Linq;
using Extensions = mCASE_ADMIN.DataAccess.mCase.Static.Extensions;

namespace mCASE_ADMIN.DataAccess.mCase
{
    public static class Factory
    {
        private static List<MCaseTypes> _stringCheck => new List<MCaseTypes>()
        {
            MCaseTypes.EmailAddress, MCaseTypes.Number,
            MCaseTypes.Phone, MCaseTypes.Time, MCaseTypes.URL
        };

        public static StringBuilder ClassInitializer(JObject jObject, string className, string nameSpace, string mainUsings)
        {
            var sb = new StringBuilder();

            var id = jObject.ParseJson(ListTransferFields.Id.GetDescription());
            var sysName = jObject.ParseJson(ListTransferFields.SystemName.GetDescription());
            var dtNow = DateTime.Now.ToString(Extensions.MCaseDateTimeStorageFormat);

            sb.AppendLine(mainUsings);
            sb.AppendLine($"namespace {nameSpace}");
            sb.AppendLine("{"); //open namespace
            sb.AppendLine(0.Indent() + $"/// <summary> Synchronized data list [{id}][{sysName}] on {dtNow} </summary>");
            sb.AppendLine(0.Indent() + $"public class {className}");
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.AppendLine(1.Indent() + "public RecordInstanceData RecordInsData;");
            sb.AppendLine(1.Indent() + "private readonly AEventHelper _eventHelper;");
            sb.AppendLine(1.Indent() + "/// <summary> Class for Updating Existing RecordInstanceData. To create a new RecordInstance data, initialize with the appropriate datalist ID. </summary>");
            sb.AppendLine(1.Indent() +
                          $"public {className}(RecordInstanceData recordInsData, AEventHelper eventHelper)");
            sb.AppendLine(1.Indent() + "{"); //open constructor #Existing Record Instance Data
            sb.AppendLine(2.Indent() + "_eventHelper = eventHelper;");
            sb.AppendLine(2.Indent() + $"if (recordInsData.DataListID != DataListId) throw new Exception(\"RecordInstance is not of type {sysName}\");");
            sb.AppendLine(2.Indent() + "RecordInsData = recordInsData;");
            sb.AppendLine(2.Indent() + "_eventHelper.AddInfoLog($\"{SystemName} has been instantiated for recordInstance value: {recordInsData.RecordInstanceID}\");");
            sb.AppendLine(1.Indent() + "}"); //close constructor
            sb.AppendLine(1.Indent() + "#region Fields");
            sb.AppendLine(1.Indent() + $"public string SystemName => \"{sysName}\";");
            sb.AppendLine(1.Indent() + "private int _dataListId = -1;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Data list identifier is -1 if not found");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "public int DataListId");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + "if(_dataListId>0) return _dataListId;");
            sb.AppendLine(3.Indent() + "var id=_eventHelper.GetDataListID(SystemName);");
            sb.AppendLine(3.Indent() + "if(id.HasValue&&id.Value>0)_dataListId=id.Value;");
            sb.AppendLine(3.Indent() + "return _dataListId;");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb;
        }

        public static StringBuilder BuildInfoClass(JObject jObject, string className, bool hasFields)
        {
            var sb = new StringBuilder();

            var id = jObject.ParseJson(ListTransferFields.Id.GetDescription());
            var sysName = jObject.ParseJson(ListTransferFields.SystemName.GetDescription());
            var dtNow = DateTime.Now.ToString(Extensions.MCaseDateTimeStorageFormat);

            #region Dl Info Class

            sb.AppendLine(0.Indent() + $"/// <summary>  Synchronized data list [{id}][{sysName}] on {dtNow} </summary>");
            sb.AppendLine(0.Indent() + $"public class {className}Info");
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.AppendLine(1.Indent() + "private AEventHelper _eventHelper;");
            sb.AppendLine(1.Indent() + $"public {className}Info(AEventHelper eventHelper)");
            sb.AppendLine(1.Indent() + "{"); //open constructor
            sb.AppendLine(2.Indent() + "_eventHelper = eventHelper;");
            sb.AppendLine(1.Indent() + "}"); //close constructor
            sb.AppendLine(1.Indent() + $"public string SystemName => \"{sysName}\";");
            sb.AppendLine(1.Indent() + "private int _dataListId = -1;");
            sb.AppendLine(1.Indent() + "/// <summary> Data list identifier is -1 if not found </summary>");
            sb.AppendLine(1.Indent() + "public int DataListId");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + "if(_dataListId != -1) return _dataListId;");
            sb.AppendLine(3.Indent() + "var id = _eventHelper.GetDataListID(SystemName);");
            sb.AppendLine(3.Indent() + "if(id.HasValue && id.Value > 0) _dataListId = id.Value;");
            sb.AppendLine(3.Indent() + "return _dataListId;");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(1.Indent() + "}"); //close Property


            if (hasFields)
            {
                #region SQL Query

                var sysNames = $"{className}Static.SystemNamesEnum";
                var sysNamesMap = $"{className}Static.SystemNamesMap";

                sb.AppendLine(1.Indent() + "/// <summary> Creates a raw sql query filter for the specified property, based off the filter params passed through</summary>");
                sb.AppendLine(1.Indent() + $"/// <param name=\"fieldSysName\">Use {sysNames} to find the field system name </param>");
                sb.AppendLine(1.Indent() + "/// <param name=\"filters\">Any string query for filteration. Sql will index '%%' query search</param>");
                sb.AppendLine(1.Indent() + "/// <returns>DirectSQLFieldFilterData object </returns>");
                sb.AppendLine(1.Indent() + $"public DirectSQLFieldFilterData CreateFilter({sysNames} fieldSysName, List<string> filters ) => " +
                              $"new DirectSQLFieldFilterData(_eventHelper.GetFieldID(DataListId, {sysNamesMap}[fieldSysName]) ?? 0, filters);");

                sb.AppendLine(1.Indent() + "/// <summary> Creates a raw sql query filter for the specified property, based off the filter params passed through</summary>");
                sb.AppendLine(1.Indent() + $"/// <param name=\"fieldSysName\">Use {sysNames} to find the field system name </param>");
                sb.AppendLine(1.Indent() + "/// <param name=\"min\">Minimum value for range index querying. Ex: DateTime.Min, or '0'</param>");
                sb.AppendLine(1.Indent() + "/// <param name=\"max\">Maximum value for range index querying. Ex: DateTime.Max, or '100'</param>");
                sb.AppendLine(1.Indent() + "/// <returns>DirectSQLFieldFilterData object </returns>");
                sb.AppendLine(1.Indent() + $"public DirectSQLFieldFilterData CreateFilter({sysNames} fieldSysName, string min, string max) => " +
                              $"new DirectSQLFieldFilterData(_eventHelper.GetFieldID(DataListId, {sysNamesMap}[fieldSysName]) ?? 0, min, max);");

                sb.AppendLine(1.Indent() + "/// <summary> Generate the filtered query, using a list of filtered query items.</summary>");
                sb.AppendLine(1.Indent() + "/// <param name=\"filters\"> List of filter data objects. Advised to use CreateFilter() method to generate filters</param>");
                sb.AppendLine(1.Indent() + $"/// <returns>List of {className} objects that match with all filter queries. Query is 'and' operator not by 'or' operator</returns>");
                sb.AppendLine(1.Indent() + $"public List<{className}> CreateQuery(List<DirectSQLFieldFilterData> filters) => _eventHelper.SearchSingleDataListSQLProcess(DataListId, filters).Select(x => new {className}(x, _eventHelper)).ToList();");

                #endregion
            }

            sb.AppendLine(0.Indent() + "}"); //close class

            #endregion

            return sb;
        }

        #region Factories

        public static StringBuilder StringFactory(JToken jToken, string propertyName, string sysName, string type, bool required)
        {
            var sb = new StringBuilder();
            var enumType = type.GetEnumValue<MCaseTypes>();

            var privateSysName = $"_{propertyName.ToLower()}";
            var mirroredField = jToken.IsMirrorField();
            var mirroredString = mirroredField ? "[ Mirrored field. No setting / updating allowed.] " : string.Empty;
            var requiredString = required ? "[Required Field] ": string.Empty;

            sb.AppendLine(1.Indent() + $"private string {privateSysName} = string.Empty;");
            sb.AppendLine(1.Indent() + $"/// <summary>{requiredString}[mCase data type: {type}] {mirroredString}</summary>");
            
            if (_stringCheck.Contains(enumType) && !mirroredField)
                sb.AppendLine(1.Indent() +
                              "/// <returns>\"-1 if string does not pass mCase data type validation.\"</returns>");

            sb.AppendLine(1.Indent() + $"public string {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if(!string.IsNullOrEmpty({privateSysName})) return {privateSysName};");
            sb.AppendLine(3.Indent() + $"{privateSysName} = RecordInsData.GetFieldValue(\"{sysName}\");");
            sb.AppendLine(3.Indent() + $"return {privateSysName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter

            if (!mirroredField)
            {
                //string is not readonly, add setter
                sb.AppendLine(2.Indent() + "set");
                sb.AppendLine(2.Indent() + "{"); //open Setter
                if (_stringCheck.Contains(enumType))
                    sb.Append(AddStringValidations(enumType));
                sb.AppendLine(3.Indent() + $"{privateSysName} = value;");
                sb.AppendLine(3.Indent() + $"RecordInsData.SetValue(\"{sysName}\", {privateSysName});");
                sb.AppendLine(2.Indent() + "}"); //close Setter
            }

            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb;
        }

        public static StringBuilder LongFactory(string propertyName, string sysName, string type, bool required)
        {
            var sb = new StringBuilder();

            var privateSysName = $"_{propertyName.ToLower()}";

            var requiredString = required ? "[Required Field] " : string.Empty;
            sb.AppendLine(1.Indent() + $"private string {privateSysName} = string.Empty;");

            sb.AppendLine(1.Indent() + $"/// <summary> {requiredString}[mCase data type: {type}] Gets value, and sets long value </summary>");
            
            sb.AppendLine(1.Indent() + $"public string {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if(!string.IsNullOrEmpty({privateSysName})) return {privateSysName};");
            sb.AppendLine(3.Indent() + $"{privateSysName} = RecordInsData.GetFieldValue(\"{sysName}\");");
            sb.AppendLine(3.Indent() + $"return {privateSysName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{"); //open Setter
            sb.AppendLine(3.Indent() + $"{privateSysName} = value;");
            sb.AppendLine(3.Indent() + $"RecordInsData.SetLongValue(\"{sysName}\", {privateSysName});");
            sb.AppendLine(2.Indent() + "}"); //close setter
            sb.AppendLine(1.Indent() + "}"); //close 

            return sb;
        }

        public static StringBuilder DateFactory(JToken jToken, string propertyName, string sysName, string type, bool required)
        {
            var sb = new StringBuilder();
            var enumType = type.GetEnumValue<MCaseTypes>();

            var privateSysName = $"_{propertyName.ToLower()}";
            var mirroredField = jToken.IsMirrorField();
            var requiredString = required ? "[Required Field] " : string.Empty;
            var mirroredString = mirroredField ? "[Mirrored field. No setting / updating allowed.] " : string.Empty;

            sb.AppendLine(1.Indent() + $"private DateTime? {privateSysName} = null;");

            sb.AppendLine(1.Indent() + $"/// <summary> {requiredString}[mCase data type: {type}] {mirroredString}</summary>");

            if (!mirroredField)
                sb.AppendLine(1.Indent() +
                              "/// <returns>If unable to parse string to datetime, datetime will be set to null</returns>");

            sb.AppendLine(1.Indent() + $"public DateTime? {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if({privateSysName} != null) return {privateSysName};");
            sb.AppendLine(3.Indent() + "if(DateTime.TryParse(RecordInsData.GetFieldValue(\"" + sysName + $"\"), out var dt)) {privateSysName} = dt;");
            sb.AppendLine(3.Indent() + $"else {privateSysName} = null;");
            sb.AppendLine(3.Indent() + $"return {privateSysName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter

            if (!mirroredField)
            {
                var setter = enumType == MCaseTypes.Date //only gets here if Date or DateTime
                    ? ".ToString(MCaseEventConstants.DateStorageFormat)"
                    : ".ToString(MCaseEventConstants.DateTimeStorageFormat)";

                //string is not readonly, add setter
                sb.AppendLine(2.Indent() + "set");
                sb.AppendLine(2.Indent() + "{"); //open Setter
                sb.AppendLine(3.Indent() + $"{privateSysName} = value;");
                sb.AppendLine(3.Indent() + $"RecordInsData.SetValue(\"{sysName}\", {privateSysName} == null ? string.Empty : {privateSysName}?{setter});");
                sb.AppendLine(2.Indent() + "}"); //close Setter
            }

            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb;
        }

        public static StringBuilder BooleanFactory(JToken jToken, string propertyName, string sysName, string type, bool required)
        {
            var sb = new StringBuilder();

            var privateSysName = $"_{propertyName.ToLower()}";
            var mirroredField = jToken.IsMirrorField();
            var mirroredString = mirroredField ? "[ Mirrored field. No setting / updating allowed.] " : string.Empty;
            var requiredString = required ? "[Required Field] " : string.Empty;

            sb.AppendLine(1.Indent() + $"private string {privateSysName} = string.Empty;");
            sb.AppendLine(1.Indent() + $"/// <summary>{requiredString}[mCase data type: {type}] {mirroredString}[Convert to Bool by using {propertyName}.ToBoolean()]</summary>");
            sb.AppendLine(1.Indent() + $"public string {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if(!string.IsNullOrEmpty({privateSysName})) return {privateSysName};");
            sb.AppendLine(3.Indent() + $"{privateSysName} = RecordInsData.GetFieldValue(\"{sysName}\");");
            sb.AppendLine(3.Indent() + $"return {privateSysName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter

            if (!mirroredField)
            {
                //string is not readonly, add setter
                sb.AppendLine(2.Indent() + "set");
                sb.AppendLine(2.Indent() + "{"); //open Setter
                sb.AppendLine(3.Indent() + $"{privateSysName} = value;");
                sb.AppendLine(3.Indent() + $"RecordInsData.SetValue(\"{sysName}\", {privateSysName});");
                sb.AppendLine(2.Indent() + "}"); //close Setter
            }

            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb;
        }

        private static StringBuilder DynamicDropDownFactory(string propertyName, string sysName, string fieldType,
            string privateName, string multiSelect, string dynamics, bool notAbleToSelectManyValues, bool required)
        {
            var requiredString = required ? "[Required Field] " : string.Empty;
            var sb = new StringBuilder();

            var dynamicData = dynamics.GetPropertyNameFromSystemName();
            sb.AppendLine(1.Indent() + $"private List<{dynamicData}> {privateName} = null;");
            sb.AppendLine(1.Indent() + $"/// <summary> {requiredString}[mCase data type: {fieldType}] [Multi Select: {multiSelect}] [Updating: Use AddTo(), or RemoveFrom()] </summary>");
            sb.AppendLine(1.Indent() + $"public List<{dynamicData}> {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if({privateName}!=null) return {privateName};");
            sb.AppendLine(3.Indent() +
                          $"{privateName}=_eventHelper.GetDynamicDropdownRecords(RecordInsData.RecordInstanceID, \"{sysName}\").Select(x => new {dynamicData}(x, _eventHelper)).ToList();");
            sb.AppendLine(3.Indent() + $"return {privateName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{"); //open Setter
            sb.AppendLine(3.Indent() +
                          $"if({privateName} == null || value == null || !value.Any()) {privateName} = new List<{dynamicData}>();");
            sb.AppendLine(3.Indent() +
                          "if (value != null && value.Any(x => x == null)) value.RemoveAll(x => x == null);");
            sb.AppendLine(3.Indent() +
                          $"if(value == null || !value.Any()) {privateName} = new List<{dynamicData}>();");
            if (notAbleToSelectManyValues)
                sb.AppendLine(3.Indent() +
                              $"if (value != null && value.Count > 1) throw new Exception(\"[Multi Select is Disabled] {sysName} only accepts a list length of 1.\");");
            sb.AppendLine(3.Indent() + $"else {privateName} = value;");
            sb.AppendLine(3.Indent() +
                          $"RecordInsData.SetValue(\"{sysName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, {privateName}.Select(x => x.RecordInsData.RecordInstanceID)));");
            sb.AppendLine(2.Indent() + "}"); //close Setter
            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb;
        }

        private static Tuple<StringBuilder, StringBuilder> DropDownFactory(JToken jToken, string propertyName, string sysName,
            string fieldType, string privateName, string multiSelect, bool notAbleToSelectManyValues, string className, bool required)
        {
            var defaultValues = jToken.ParseDefaultData(ListTransferFields.FieldValues.GetDescription());
            var staticName = $"{className}Static.DefaultValuesEnum";
            var defaultMap = $"{className}Static.DefaultValuesMap";
            var defaultOptionsName = $"{privateName}Values";
            var requiredString = required ? "[Required Field] " : string.Empty;

            var enumValues = new StringBuilder();
            var returnValue = new StringBuilder();

            if (defaultValues.Any())
            {
                returnValue = GenerateDDWithDefaultValues(propertyName, sysName, fieldType, privateName, multiSelect, notAbleToSelectManyValues, defaultValues, staticName, defaultOptionsName, defaultMap, requiredString);

                var enums = new StringBuilder();
                enums.Append(string.Join("$~*@*~$", defaultValues));
                enumValues = enums;
            }
            else
            {
                returnValue = NoDefaultValues(propertyName, sysName, privateName, fieldType, multiSelect, requiredString);
            }


            return new Tuple<StringBuilder, StringBuilder>(returnValue, enumValues);
        }

        /// <summary>
        /// All enum values are required to be pushed to a enum static extension file
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <param name="sysName"></param>
        /// <param name="fieldType"></param>
        /// <returns>Property, Enum</returns>
        public static Tuple<StringBuilder, StringBuilder> EnumerableFactory(JToken jToken, MCaseTypes type,
            string propertyName, string sysName, string fieldType, string className, bool required)
        {
            var fieldOptions = jToken.ParseToken(ListTransferFields.FieldOptions.GetDescription());
            var dynamicData = jToken.ParseDynamicData(ListTransferFields.DynamicData.GetDescription());

            var privateName = $"_{propertyName.ToLower()}";
            var notAbleToSelectManyValues = fieldOptions.ToLowerInvariant().Contains("\"Able to Select Multiple values\"" + ":" + "\"No\"".ToLowerInvariant());
            var multiSelect = notAbleToSelectManyValues ? "False" : "True";

            switch (type)
            {
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                    var dropDownValues = DropDownFactory(jToken, propertyName, sysName, fieldType, privateName,
                        multiSelect, notAbleToSelectManyValues, className, required);// has default values. Values are strings

                    //Item 1 = property, Item 2 = Enum
                    return new Tuple<StringBuilder, StringBuilder>(dropDownValues.Item1, dropDownValues.Item2);

                case MCaseTypes.CascadingDynamicDropDown:
                case MCaseTypes.DynamicDropDown:
                    var dynamicValues = DynamicDropDownFactory(propertyName, sysName, fieldType, privateName,
                        multiSelect, dynamicData, notAbleToSelectManyValues, required);//does not have default values. Values are RecordInstances (pointers)
                    return new Tuple<StringBuilder, StringBuilder>(dynamicValues, new StringBuilder());

                default:
                    return new Tuple<StringBuilder, StringBuilder>(new StringBuilder(), new StringBuilder());
            }
        }

        #endregion
        #region Additional Factory Helpers

        private static string AddStringValidations(MCaseTypes type)
        {
            var sb = new StringBuilder();

            switch (type)
            {
                case MCaseTypes.EmailAddress:
                    sb.AppendLine(3.Indent() + "if(!value.Contains(\"@\")) value = \"-1\";");
                    break;
                case MCaseTypes.Number:
                    sb.AppendLine(3.Indent() +
                                  "var isNumeric = long.TryParse(value, out _) || int.TryParse(value, out _) || double.TryParse(value, out _);");
                    sb.AppendLine(3.Indent() + "if(!isNumeric) value = \"-1\";");
                    break;
                case MCaseTypes.Phone:
                    sb.AppendLine(3.Indent() + "var isNumeric = int.TryParse(value, out _);");
                    sb.AppendLine(3.Indent() + "if(!isNumeric) value = \"-1\";");
                    break;
                case MCaseTypes.Time:
                    sb.AppendLine(3.Indent() + "var result = DateTime.TryParse(value, out var dt);");
                    sb.AppendLine(3.Indent() + "if(!result) value = \"-1\";");
                    sb.AppendLine(3.Indent() + "else value = dt.ToString(\"HH:mm\");");
                    break;
                case MCaseTypes.URL:
                    sb.AppendLine(3.Indent() +
                                  "if(!value.Contains(\".com\") || !value.Contains(\"https://\")) value = \"-1\";");
                    break;
                default:
                    return string.Empty;
            }

            return sb.ToString();
        }

        private static StringBuilder NoDefaultValues(string propertyName, string systemName, string privateName, string fieldType, string multiSelect, string required)
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + $"private List<string> {privateName}= null;");
            sb.AppendLine(1.Indent() + $"/// <summary> [mCase data type: {fieldType}] [Multi Select: {multiSelect}] {required}[Getting: Returns the list of field labels] [Updating: AddTo(), AddRangeTo(), RemoveFrom()] </summary>");

            if (!string.Equals(multiSelect, "True"))
            {
                sb.AppendLine(1.Indent() + "/// <returns>If multi select is set to false, and more than one value in list. List will be set to a single item of MultiSelect = false</returns>");
            }

            sb.AppendLine(1.Indent() + $"public List<string> {propertyName}");
            sb.AppendLine(1.Indent() + "{");//open property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{");//open getter
            sb.AppendLine(3.Indent() + $"if({privateName} != null) return {privateName};");
            sb.AppendLine(3.Indent() + $"{privateName} = RecordInsData.GetMultiSelectFieldValue(\"{systemName}\");");
            sb.AppendLine(3.Indent() + $"return {privateName};");
            sb.AppendLine(2.Indent() + "}");//close getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{");//open setter
            sb.AppendLine(3.Indent() + "if(value == null)  value = new List<string>();");
            if (!string.Equals(multiSelect, "True"))
            {
                sb.AppendLine(3.Indent() + "if(value.Count > 1) value = new List<string>()" + "{ \" MultiSelect = false\" };");
            }
            sb.AppendLine(3.Indent() + $"{privateName} = value;");
            sb.AppendLine(3.Indent() + $"RecordInsData.SetValue(\"{systemName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, {privateName}));");
            sb.AppendLine(2.Indent() + "}");//close setter
            sb.AppendLine(1.Indent() + "}");//close property

            return sb;
        }

        private static StringBuilder GenerateDDWithDefaultValues(string propertyName, string sysName, string fieldType,
            string privateName, string multiSelect, bool notAbleToSelectManyValues, List<string> defaultValues,
            string staticName, string defaultOptionsName, string map, string required)
        {
            var sb = new StringBuilder();

            #region private property

            var defaultAggregate = defaultValues.Aggregate(
                $"private List<{staticName}> {defaultOptionsName} = new List<{staticName}>() " + "{", (current, child) => current + $"{staticName}.{child.GetPropertyNameFromSystemName()},");

            defaultAggregate += "};";

            sb.AppendLine(1.Indent() + defaultAggregate);

            sb.AppendLine(1.Indent() + $"private List<{staticName}> {privateName} = null;");

            #endregion

            #region Summary

            sb.AppendLine(1.Indent() + $"/// <summary> {required}[mCase data type: {fieldType}] [Multi Select: {multiSelect}] [Default field values can be found in {staticName}] [Available options can be found in {defaultOptionsName}] [Getting: Returns the list of \"{staticName}'s\"] [Updating: AddTo(), RemoveFrom(), MapTo()] </summary>");

            if (!string.Equals(multiSelect, "True"))
            {
                sb.AppendLine(1.Indent() + $"/// <returns> List of {defaultOptionsName}. If multi select is false and value count is greater than one. New value will return: \"Multi Select: False\" </returns>");
            }

            #endregion

            #region Public property

            sb.AppendLine(1.Indent() + $"public List<{staticName}> {propertyName}");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + $"if({privateName} != null) return {privateName};");
            sb.AppendLine(3.Indent() + $"{privateName} = GetMultiSelectValue(\"" + sysName + "\");");
            sb.AppendLine(3.Indent() + $"return {privateName};");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(2.Indent() + "set");
            sb.AppendLine(2.Indent() + "{"); //open Setter
            sb.AppendLine(3.Indent() + $"if({privateName} == null) {privateName} = new List<{staticName}>();");
            if (notAbleToSelectManyValues)
            {
                sb.AppendLine(3.Indent() + $"if (value.Count > 1) value = new List<{staticName}>() " + "{" + $"{staticName}.Multiselectfalse" + "};");
            }

            sb.AppendLine(3.Indent() + $"{privateName} = SetDefaultList(value, value.Any(x => !{defaultOptionsName}.Contains(x)));");
            sb.AppendLine(3.Indent() + $"RecordInsData.SetValue(\"{sysName}\", string.Join(MCaseEventConstants.MultiDropDownDelimiter, {privateName}.Select(x => {map}[x])));");
            sb.AppendLine(2.Indent() + "}"); //close Setter
            sb.AppendLine(1.Indent() + "}"); //close Property

            #endregion

            return sb;
        }

        #endregion
        #region Static File Extensions

        public static string GenerateStaticFile(string namespace_, string staticUsings)
        {
            var sb = new StringBuilder();

            sb.AppendLine( //TODO continue to add usings, as more and more validations are made
                staticUsings);
            sb.AppendLine($"namespace {namespace_}"); //TODO: project specific namespace
            sb.AppendLine("{"); //Open class
            sb.AppendLine(0.Indent() + "/// <summary> Much to learn, this static extension still has. Powerful tool it can be. But remember, foresee the consequences of your code you must. With caution, use this extension you should. </summary>"); //static class
            sb.AppendLine(0.Indent() + "public static class FactoryExtensions"); //static class
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.Append(BuildEnumExtensions());
            sb.Append(AddStaticEnumerableExtensions());
            sb.Append(AddBooleanMethod());

            sb.AppendLine(0.Indent() + "}"); //close class
            sb.AppendLine("}"); //close  class
            return sb.ToString();
        }

        private static StringBuilder AddBooleanMethod()
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + "/// <summary> If string is not null or empty, this method returns an accurate boolean from a string based off of MCaseEventConstants.TrueValues && MCaseEventConstants.FalseValues</summary>"); //static class
            sb.AppendLine(1.Indent() + "public static bool? ToBoolean(this string value)");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "if (string.IsNullOrEmpty(value)) return null;");
            sb.AppendLine(2.Indent() + "if (MCaseConstants.TrueValues.Contains(value, StringComparer.OrdinalIgnoreCase)) return true;");
            sb.AppendLine(2.Indent() + "if (MCaseConstants.FalseValues.Contains(value, StringComparer.OrdinalIgnoreCase)) return false;");
            sb.AppendLine(2.Indent() + "return null;");
            sb.AppendLine(1.Indent() + "}");//close method

            return sb;
        }

        /// <summary>
        /// Required because c# has getters and setters  but no updaters. If you add something to a list, it is required to update the internal state
        /// This method allows us to update the internal state property on updation. Essentially, creating an updater. Since we do not
        /// operate directly on the property itself, we aquire the property name using autogenerated enums. Using this we must fetch the class property using
        /// this.GetType() and GetProperty(). Verify that the property was succesfully returned and then we must verify that the return type of the property is the
        /// same as the type passed into the method. This is done by fetching the methodinfo using GetGetMethod().Return type and then comparing that to the incoming
        /// value. If both the property return type, and the argument type are the same we can begin checks for setting the value.
        /// </summary>
        /// <returns></returns>
        private static string AddStaticEnumerableExtensions()
        {
            var sb = new StringBuilder();

            sb.Append(BuildAddMethods());

            sb.Append(BuildAddRangeMethods());

            sb.Append(BuildRemoveFromMethods());

            sb.Append(BuildClearMethods());

            sb.Append(GetRelatedRecordsStatic());

            return sb.ToString();
        }

        private static StringBuilder GetRelatedRecordsStatic()
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + "public static List<RecordInstanceData> GetRelatedRecords(this AEventHelper eventHelper, long recordInstanceId, string sysName)");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "var childDataListId = eventHelper.GetDataListID(sysName);");
            sb.AppendLine(2.Indent() + "return !childDataListId.HasValue ? new List<RecordInstanceData>() : eventHelper.GetActiveChildRecordsByListId(recordInstanceId, childDataListId.Value).ToList();");
            sb.AppendLine(1.Indent() + "}");//close method

            return sb;

        }

        private static StringBuilder BuildClearMethods()
        {
            var sb = new StringBuilder();

            #region Clear

            sb.AppendLine(1.Indent() + "/// <summary> Clears internal state list for class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"TEnum\">Property Enum specific to class</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Null Errors - 2, Invalid Types -1, Success 0</returns>");
            sb.AppendLine(1.Indent() + "public static int Clear<T, TEnum>(this T classObject, string property) where TEnum : Enum");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if(getMethod == null) return -2;");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType == typeof(List<string>)) propertyInfo.SetValue(classObject, new List<string>());");
            sb.AppendLine(2.Indent() + "else if (getMethod.ReturnType == typeof(List<TEnum>)) propertyInfo.SetValue(classObject, new List<TEnum>());");
            sb.AppendLine(2.Indent() + "else if (getMethod.ReturnType == typeof(List<RecordInstanceData>)) propertyInfo.SetValue(classObject, new List<RecordInstanceData>());");
            sb.AppendLine(2.Indent() + "else return -1;");
            sb.AppendLine(2.Indent() + "return 0;");
            sb.AppendLine(1.Indent() + "}"); //close method


            #endregion

            return sb;
        }

        private static StringBuilder BuildAddMethods()
        {
            var sb = new StringBuilder();

            #region Add RecordInstance

            sb.AppendLine(1.Indent() + "/// <summary> Adds object to internal state of class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">Value added to property</param>");
            sb.AppendLine(1.Indent() + "/// <returns>RecordInstance not Created -5, Null Errors - 2, Invalid Types -1, Success 1</returns>");
            sb.AppendLine(1.Indent() + "public static int AddTo<T>(this T classObject, string property, RecordInstanceData value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if (value.RecordInstanceID == 0) return -5;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<RecordInstanceData>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "getter.Add(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return 1;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region Add String

            sb.AppendLine(1.Indent() + "/// <summary> Adds object to internal state of class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">Value added to property</param>");
            sb.AppendLine(1.Indent() + "/// <returns>RecordInstance not Created -5, Null Errors - 2, Invalid Types -1, Success 1</returns>");
            sb.AppendLine(1.Indent() + "public static int AddTo<T>(this T classObject, string property, string value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<string>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<string>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "getter.Add(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return 1;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region Add DefaultValue

            sb.AppendLine(1.Indent() + "/// <summary> Adds object to internal state of class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"TEnum\">Property value related to Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">Value added to property</param>");
            sb.AppendLine(1.Indent() + "/// <returns>RecordInstance not Created -5, Null Errors - 2, Invalid Types -1, Success 1</returns>");
            sb.AppendLine(1.Indent() +
                          $"public static int AddTo<T, TEnum>(this T classObject,string property, TEnum value) where TEnum : Enum");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() +
                          "if (getMethod.ReturnType != typeof(List<TEnum>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<TEnum>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "getter.Add(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return 1;");
            sb.AppendLine(1.Indent() + "}"); //close method


            #endregion

            return sb;
        }

        private static StringBuilder BuildAddRangeMethods()
        {
            var sb = new StringBuilder();

            #region AddRange RecordInstance

            sb.AppendLine(1.Indent() + "/// <summary> Adds Range object to internal state of class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">Value added to property</param>");
            sb.AppendLine(1.Indent() + "/// <returns>RecordInstance not Created -5, Null Errors - 2, Invalid Types -1, Success returns amount added</returns>");
            sb.AppendLine(1.Indent() +
                          $"public static int AddRangeTo<T>(this T classObject, string property, List<RecordInstanceData> value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if (!value.Any()) return 0;");
            sb.AppendLine(2.Indent() + "if (value.Any(x => x.RecordInstanceID == 0)) return -5;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() +
                          "if (getMethod.ReturnType != typeof(List<RecordInstanceData>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "value.RemoveAll(x => x == null);");
            sb.AppendLine(2.Indent() + "if(value.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.AddRange(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return getter.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region AddRange RecordInstance

            sb.AppendLine(1.Indent() + "/// <summary> Adds Range object to internal state of class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">Value added to property</param>");
            sb.AppendLine(1.Indent() + "/// <returns>RecordInstance not Created -5, Null Errors - 2, Invalid Types -1, Success returns amount added</returns>");
            sb.AppendLine(1.Indent() +
                          $"public static int AddRangeTo<T>(this T classObject, string property, List<string> value)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if(!value.Any()) return 0;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() +
                          "if (getMethod.ReturnType != typeof(List<string>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<string>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "value.RemoveAll(x => x == null);");
            sb.AppendLine(2.Indent() + "if(value.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.AddRange(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return value.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region AddRange Default Values

            sb.AppendLine(1.Indent() + "/// <summary> Adds Range object to internal state of class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"TEnum\">Property type specific to class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"value\">Value added to property</param>");
            sb.AppendLine(1.Indent() + "/// <returns>RecordInstance not Created -5, Null Errors - 2, Invalid Types -1, Success returns amount added</returns>");
            sb.AppendLine(1.Indent() + "public static int AddRangeTo<T, TEnum>(this T classObject, string property, List<TEnum> value) where TEnum : Enum");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (value == null) return -2;");
            sb.AppendLine(2.Indent() + "if(!value.Any()) return 0;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<TEnum>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<TEnum>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "value.RemoveAll(x => x == null);");
            sb.AppendLine(2.Indent() + "if(value.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.AddRange(value);");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return value.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            return sb;
        }

        private static StringBuilder BuildRemoveFromMethods()
        {
            var sb = new StringBuilder();

            #region Remove RecordInstance

            sb.AppendLine(1.Indent() + "/// <summary> Removes object from internal state of class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"predicate\">Predicate value to remove</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Null Errors - 2, Invalid Types -1, Success returns amount removed</returns>");
            sb.AppendLine(1.Indent() + "public static int RemoveFrom<T>(this T classObject, string property, Func<RecordInstanceData, bool> predicate)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (predicate == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var getMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (getMethod.ReturnType != typeof(List<RecordInstanceData>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<RecordInstanceData>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if (getter == null) return -2;");
            sb.AppendLine(2.Indent() + "var foundValues = getter.Where(predicate).ToList();");
            sb.AppendLine(2.Indent() + "if (foundValues.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "getter.RemoveAll(x => foundValues.Contains(x));");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter);");
            sb.AppendLine(2.Indent() + "return foundValues.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region Remove string
            sb.AppendLine(1.Indent() + "/// <summary> Removes object from internal state of class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"predicate\">Predicate value to remove</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Null Errors - 2, Invalid Types -1, Success returns amount removed</returns>");
            sb.AppendLine(1.Indent() +
                          "public static int RemoveFrom<T>(this T classObject, string property, Func<string, bool> predicate)");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (predicate == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var propertyMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (propertyMethod.ReturnType != typeof(List<string>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<string>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "var foundStrings = getter.Where(predicate).ToList();");
            sb.AppendLine(2.Indent() + "if (foundStrings.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter.RemoveAll(x => foundStrings.Contains(x)));");
            sb.AppendLine(2.Indent() + "return foundStrings.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            #region Remove enum

            sb.AppendLine(1.Indent() + "/// <summary> Removes object from internal state of class property </summary>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"T\">Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <typeparam name=\"TEnum\">PropertyValue related to Class object</typeparam>");
            sb.AppendLine(1.Indent() + "/// <param name=\"classObject\">The actual class object</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"property\">String param referencing the class property</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"predicate\">Predicate value to remove</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Null Errors - 2, Invalid Types -1, Success returns amount removed</returns>");
            sb.AppendLine(1.Indent() +
                          "public static int RemoveFrom<T, TEnum>(this T classObject, string property, Func<TEnum, bool> predicate) where TEnum : Enum");
            sb.AppendLine(1.Indent() + "{"); //open method
            sb.AppendLine(2.Indent() + "if (predicate == null) return -2;");
            sb.AppendLine(2.Indent() + "var objectType = classObject.GetType();");
            sb.AppendLine(2.Indent() + "var propertyInfo = objectType.GetProperty(property);");
            sb.AppendLine(2.Indent() + "if (propertyInfo == null) return -2;");
            sb.AppendLine(2.Indent() + "var propertyMethod = propertyInfo.GetGetMethod();");
            sb.AppendLine(2.Indent() + "if (propertyMethod.ReturnType != typeof(List<TEnum>)) return -1;//Verify that the argument can be added to the property type");
            sb.AppendLine(2.Indent() + "var getter = (List<TEnum>)propertyInfo.GetValue(classObject);");
            sb.AppendLine(2.Indent() + "if(getter == null) return -2;");
            sb.AppendLine(2.Indent() + "var foundStrings = getter.Where(predicate).ToList();");
            sb.AppendLine(2.Indent() + "if (foundStrings.Count < 1) return 0;");
            sb.AppendLine(2.Indent() + "propertyInfo.SetValue(classObject, getter.RemoveAll(x => foundStrings.Contains(x)));");
            sb.AppendLine(2.Indent() + "return foundStrings.Count;");
            sb.AppendLine(1.Indent() + "}"); //close method

            #endregion

            return sb;
        }

        public static StringBuilder BuildEnumExtensions()
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + "public static TEnum TryGetValue<TEnum>(this string value) where TEnum : struct => Enum.TryParse<TEnum>(value.GetEnumName(), out var converted) ? converted : default;");
            sb.AppendLine(1.Indent() + "public static List<TEnum> MapTo<TEnum>(this IEnumerable<string> values) where TEnum : struct => values.Select(value => value.TryGetValue<TEnum>()).ToList();");

            sb.AppendLine(1.Indent() + "private static string GetEnumName(this string input)");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "input = Regex.Replace(input, @\"[^\\w]\", \"\");");
            sb.AppendLine(2.Indent() + "if (int.TryParse(input, out _)) return \"F_\" + input;");
            sb.AppendLine(2.Indent() + "if (int.TryParse(input[0].ToString(), out _)) input = \"F_\" + input;");
            sb.AppendLine(2.Indent() + "char.TryParse(input[0].ToString().ToUpperInvariant(), out var cap);");
            sb.AppendLine(2.Indent() + "var lowerCase = input.Substring(1).ToLower();");
            sb.AppendLine(2.Indent() + "return cap + lowerCase;");
            sb.AppendLine(1.Indent() + "}");// close method


            return sb;
        }

        #endregion

        public static string GetActiveRelatedRecords(HashSet<string> embedded)
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + "#region Related Records");
            foreach (var value in embedded)
            {
                var propertyName = value.GetPropertyNameFromSystemName();

                sb.AppendLine(1.Indent() + $"/// <summary> Gets active related records of type: {propertyName}</summary>");
                sb.AppendLine(1.Indent() + $"/// <returns>Related children from {propertyName}</returns>");
                sb.AppendLine(1.Indent() +
                              $"public List<{propertyName}> GetActive{propertyName}Records() => _eventHelper.GetRelatedRecords(RecordInsData.RecordInstanceID, new {propertyName}Info(_eventHelper).SystemName).Select(x => new {propertyName}(x, _eventHelper)).ToList();"); // property name is added back with enum name appended 

            }

            sb.AppendLine(1.Indent() + "#endregion Related Records");
            return sb.ToString();
        }

        public static string AddEnumerableExtensions(string className, bool addDefaults, List<Tuple<string, string, bool, string, string>> requiredFields, HashSet<Tuple<string, string>> fields)
        {
            var sb = new StringBuilder();

            var entity = $"{className}";
            var staticProperties = $"{className}Static.PropertiesEnum";
            var defaultValues = $"{className}Static.DefaultValuesEnum";
            var propertyMap = $"{className}Static.PropertiesMap";
            var defaultMap = $"{className}Static.DefaultValuesMap";
            
            if (addDefaults)
            {
                // Map To Enum
                sb.AppendLine(1.Indent() + "/// <summary>  Maps a list of enums, to the next enum type </summary>");
                sb.AppendLine(1.Indent() + "/// <typeparam name=\"TEnum\">The enum you are mapping to</typeparam>");
                sb.AppendLine(1.Indent() + "/// <param name=\"values\">Enum Values to convert</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Returns the converted list of enums, or default values if not found</returns>");
                sb.AppendLine(1.Indent() + $"public List<TEnum> MapToEnum<TEnum>(List<{className}Static.DefaultValuesEnum> values) where TEnum : struct => values.Select(x => {defaultMap}[x]).MapTo<TEnum>();");

                //remove by enum predicate 
                sb.AppendLine(1.Indent() + "/// <summary> Remove all data from enumerable class property that matches predicate </summary>");
                sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
                sb.AppendLine(1.Indent() + "/// <param name=\"predicate\">Default value to remove</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. null errors: -2</returns>");
                sb.AppendLine(1.Indent() + $"public int RemoveFrom({staticProperties} propertyEnum, Func<{defaultValues}, bool> predicate) => this.RemoveFrom({propertyMap}[propertyEnum], predicate);");
            }

            #region Remove by predicate Predicate
            // remove value by Record instance predicate
            sb.AppendLine(1.Indent() + "/// <summary>  Remove all data from enumerable class property that matches predicate </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"predicate\">Record instance value to remove</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. null errors: -2</returns>");
            sb.AppendLine(1.Indent() + $"public int RemoveFrom({staticProperties} propertyEnum, Func<RecordInstanceData, bool> predicate) => this.RemoveFrom({propertyMap}[propertyEnum], predicate);");

            //remove by string predicate
            sb.AppendLine(1.Indent() + "/// <summary> Remove all data from enumerable class property that matches predicate </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"predicate\">String value to remove</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. null errors: -2</returns>");
            sb.AppendLine(1.Indent() + $"public int RemoveFrom({staticProperties} propertyEnum, Func<string, bool> predicate) => this.RemoveFrom({propertyMap}[propertyEnum], predicate);");

            #endregion

            #region Add To

            // add single
            sb.AppendLine(1.Indent() + "/// <summary> Add data onto enumerable class property. </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
            sb.AppendLine(1.Indent() +
                          "/// <returns>Amount of values removed. Type errors: -1. Null errors: -2.</returns>");
            sb.AppendLine(1.Indent() +
                          $"public int AddTo({staticProperties} propertyEnum, string param) => this.AddTo({propertyMap}[propertyEnum], param);");

            //add single Record Instance
            sb.AppendLine(1.Indent() + "/// <summary> Add data onto enumerable class property. </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values removed. Type errors: -1. Null errors: -2. RecordInstance not Created: -5.</returns>");
            sb.AppendLine(1.Indent() + $"public int AddTo({staticProperties} propertyEnum, RecordInstanceData param) => this.AddTo({propertyMap}[propertyEnum], param);");

            if (addDefaults)
            {
                //add default value (Enum)
                sb.AppendLine(1.Indent() + "/// <summary> Add data onto enumerable class property. </summary>");
                sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
                sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2.</returns>");
                sb.AppendLine(1.Indent() + $"public int AddTo({staticProperties} propertyEnum, {defaultValues} param) => this.AddTo({propertyMap}[propertyEnum], param);");
            }

            #endregion

            #region Add Range

            //add range string
            sb.AppendLine(1.Indent() + "/// <summary> Add data onto enumerable class property. List of string's </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2.</returns>");
            sb.AppendLine(1.Indent() + $"public int AddRangeTo({staticProperties} propertyEnum, List<string> param) => this.AddRangeTo({propertyMap}[propertyEnum], param);");

            //add range Record Instance
            sb.AppendLine(1.Indent() + "/// <summary> Add data onto enumerable class property. List of RecordInstanceData's </summary>");
            sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
            sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
            sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2. RecordInstance not Created: -5</returns>");
            sb.AppendLine(1.Indent() + $"public int AddRangeTo({staticProperties} propertyEnum, List<RecordInstanceData> param) => this.AddRangeTo({propertyMap}[propertyEnum], param);");


            if (addDefaults)
            {
                //add default value range (Enum)
                sb.AppendLine(1.Indent() + $"/// <summary>  Add data onto enumerable class property. List of {defaultValues}'s </summary>");
                sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
                sb.AppendLine(1.Indent() + "/// <param name=\"param\">Value added to Class</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Amount of values added. Type errors: -1. null errors: -2.</returns>");
                sb.AppendLine(1.Indent() + $"public int AddRangeTo({staticProperties} propertyEnum, List<{defaultValues}> param) => this.AddRangeTo({propertyMap}[propertyEnum], param);");

            }

            #endregion
            if (addDefaults)
            {
                #region Clear With Default Values

                //clear
                sb.AppendLine(1.Indent() + "/// <summary> Clears all existing values from list. </summary>");
                sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Cleared list = 0. Type errors: -1. null errors: -2.</returns>");
                sb.AppendLine(1.Indent() + $"public int Clear({staticProperties} propertyEnum) => this.Clear<{entity}, {defaultValues}>({propertyMap}[propertyEnum]);");

                #endregion
            }
            else
            {
                #region Clear Without DefaultValues

                //clear
                sb.AppendLine(1.Indent() + "/// <summary>  Clears all existing values from list. </summary>");
                sb.AppendLine(1.Indent() + "/// <param name=\"propertyEnum\">Class public property name</param>");
                sb.AppendLine(1.Indent() + "/// <returns>Cleared list = 0. Type errors: -1. null errors: -2.</returns>");
                sb.AppendLine(1.Indent() + $"public int Clear({staticProperties} propertyEnum) => this.Clear<{entity}, {staticProperties}>({propertyMap}[propertyEnum]);");

                #endregion
            }
            #region Save

            // Can save record
            sb.AppendLine(1.Indent() + "/// <summary>Checks all required fields in Datalist</summary>");
            sb.AppendLine(1.Indent() + "/// <returns>All required fields that have yet been filled in.</returns>");
            sb.AppendLine(1.Indent() + "public List<string> RequiredFieldsCheck()");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "var requiredFields = new List<string>();");

            foreach (var required in requiredFields)
            {
                var check = AddSaveRecordCheckForRequiredProperty(required, fields, defaultValues);

                if (!string.IsNullOrEmpty(check))
                    sb.AppendLine(2.Indent() + check);
            }
            sb.AppendLine(2.Indent() + "if(requiredFields.Count > 0) _eventHelper.AddWarningLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: Required fields check returned {requiredFields.Count} mandatory field(s) unfilled\");");
            sb.AppendLine(2.Indent() + "return requiredFields;");
            sb.AppendLine(1.Indent() + "}");//close method



            // save record
            sb.AppendLine(1.Indent() + "/// <summary> If all required fields have been filled in. This will save the RecordInstanceData</summary>");
            sb.AppendLine(1.Indent() + "/// <returns>A list of all unfilled required fields. Empty list means succesfully saved</returns>");
            sb.AppendLine(1.Indent() + "public List<string> SaveRecord()");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "var requiredFields = RequiredFieldsCheck();");
            sb.AppendLine(2.Indent() + "if(requiredFields.Count > 0) return requiredFields;");
            sb.AppendLine(2.Indent() + "_eventHelper.SaveRecord(RecordInsData);");
            sb.AppendLine(2.Indent() + "_eventHelper.AddInfoLog($\"[{SystemName}] Successfully saved record: {RecordInsData.RecordInstanceID}\");");
            sb.AppendLine(2.Indent() + "return requiredFields;");
            sb.AppendLine(1.Indent() + "}");//close method


            // Delete record
            sb.AppendLine(1.Indent() + "/// <summary> If exception thrown here, attempt to save record prior to soft delete </summary>");
            sb.AppendLine(1.Indent() + "public EventStatusCode SoftDelete()");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "RecordInsData.Status = MCaseEventConstants.RecordStatusDeleted;");
            sb.AppendLine(2.Indent() + "RecordInsData.FrozenInd = true;");
            sb.AppendLine(2.Indent() + "_eventHelper.SaveRecord(RecordInsData);");
            sb.AppendLine(2.Indent() + "_eventHelper.AddInfoLog($\"[{SystemName}] Soft Deleted Record: {RecordInsData.RecordInstanceID}\");");
            sb.AppendLine(2.Indent() + "return EventStatusCode.Success;");
            sb.AppendLine(1.Indent() + "}");//close method


            sb.AppendLine(1.Indent() + "public void LogDebug(string log) => _eventHelper.AddDebugLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: {log}\");");
            sb.AppendLine(1.Indent() + "public void LogInfo(string log) => _eventHelper.AddInfoLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: {log}\");");
            sb.AppendLine(1.Indent() + "public void LogWarning(string log) => _eventHelper.AddWarningLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: {log}\");");
            sb.AppendLine(1.Indent() + "public void LogError(string log) => _eventHelper.AddErrorLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: {log}\");");

            #endregion
            #region private method extractions

            if (addDefaults)
            {
                sb.AppendLine(1.Indent() + "#region Private");
                //add MultiSelectValue
                sb.AppendLine(1.Indent() + $"private List<{defaultValues}> GetMultiSelectValue(string sysName)");
                sb.AppendLine(1.Indent() + "{");//open method
                sb.AppendLine(2.Indent() + "var storedValue = RecordInsData.GetMultiSelectFieldValue(sysName);");
                sb.AppendLine(2.Indent() + $"return !storedValue.Any() ? new List<{defaultValues}>() : storedValue.Select(x => x.TryGetValue<{defaultValues}>()).ToList();");
                sb.AppendLine(1.Indent() + "}");//close method

                //Set Default values
                sb.AppendLine(1.Indent() + $"private List<{defaultValues}> SetDefaultList(List<{defaultValues}> value, bool invalidValues)");
                sb.AppendLine(1.Indent() + "{");//open method
                sb.AppendLine(2.Indent() + $"if (value == null || !value.Any()) return new List<{defaultValues}>();");
                sb.AppendLine(2.Indent() + $"if (value == new List<{defaultValues}>() " + "{ " + defaultValues + ".Multiselectfalse }) return value;");
                sb.AppendLine(2.Indent() + $"return invalidValues ? new List<{defaultValues}>()" + "{ " + defaultValues + ".Invalidselection } : value;");
                sb.AppendLine(1.Indent() + "}");//close method

                sb.AppendLine(1.Indent() + "#endregion Private");
            }

            #endregion
            return sb.ToString();
        }

        /// <summary>
        /// [0] field type [1] system name [2] conditionally mandatory [3] Mandated by field [4] mandated by value
        /// </summary>
        /// <param name="required"></param>
        /// <returns></returns>
        private static string AddSaveRecordCheckForRequiredProperty(Tuple<string, string, bool, string, string> required, HashSet<Tuple<string, string>> fields, string defaultEnum)
        {
            var type = required.Item1.GetEnumValue<MCaseTypes>();
            var privateName = "_" + required.Item2.GetPropertyNameFromSystemName().ToLower();
            var propertyName = required.Item2.GetPropertyNameFromSystemName();
            
            if (required.Item3)//field is conditionally mandatory
            {
                var mandatedByField = required.Item4;
                var mandatedByValue = required.Item5;

                var dependentField = fields.FirstOrDefault(x => string.Equals(x.Item2.GetPropertyNameFromSystemName(), mandatedByField, StringComparison.OrdinalIgnoreCase));
                if (dependentField != null)
                {
                    //field is the conditional field
                    var conditionalResult = AddConditionallyMandatoryCheck(dependentField, mandatedByValue, privateName, propertyName, type, defaultEnum);

                    return conditionalResult;
                }
                //what do we do here?

            }

            return CanSaveValidationHelper(type, privateName, propertyName);
        }

        private static string CanSaveValidationHelper(MCaseTypes type, string privateName, string propertyName)
        {
            switch (type)
            {
                //case mCaseTypes.EmbeddedList: Processed after loop completion
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                case MCaseTypes.DynamicDropDown:
                case MCaseTypes.CascadingDynamicDropDown:
                    return $"if({privateName} != null && {privateName}.Count == 0) requiredFields.Add(\"{propertyName}\");";
                case MCaseTypes.String:
                case MCaseTypes.LongString:
                case MCaseTypes.EmailAddress:
                case MCaseTypes.Phone:
                case MCaseTypes.URL:
                case MCaseTypes.Number:
                case MCaseTypes.Money:
                case MCaseTypes.Time:
                case MCaseTypes.Boolean:
                case MCaseTypes.ReadonlyField:
                case MCaseTypes.User:
                case MCaseTypes.Address:
                case MCaseTypes.Attachment:
                    return $"if(string.IsNullOrEmpty({privateName})) requiredFields.Add(\"{propertyName}\");";
                case MCaseTypes.Date:
                case MCaseTypes.DateTime:
                    return $"if({privateName} == null) requiredFields.Add(\"{propertyName}\");";
                case MCaseTypes.Section: //need in ce's?
                case MCaseTypes.Narrative: //need in ce's?
                case MCaseTypes.Header: //need in ce's?
                case MCaseTypes.UserRoleSecurityRestrict: //not required in CE's
                case MCaseTypes.DynamicCalculatedField: //not required in CE's
                case MCaseTypes.CalculatedField: // not required in CE's 
                case MCaseTypes.UniqueIdentifier: //not required in CE's
                case MCaseTypes.EmbeddedDocument: // blob?
                case MCaseTypes.HiddenField: //not required in CE's
                case MCaseTypes.LineBreak: //not required in CE's
                case MCaseTypes.Position0: //not required in CE's
                case MCaseTypes.Score1: //not required in CE's
                case MCaseTypes.Score2: //not required in CE's
                case MCaseTypes.Score3: //not required in CE's
                case MCaseTypes.Score4: //not required in CE's
                case MCaseTypes.Score5: //not required in CE's
                case MCaseTypes.Score6: //not required in CE's
                default:
                    return string.Empty;
            }
        }

        private static string AddConditionallyMandatoryCheck(Tuple<string, string> dependentField, string mandatedByValue, string privateName, string propertyName, MCaseTypes mandatoryType , string defaultValue)

        {
            var sb = new StringBuilder();
            var type = dependentField.Item1.GetEnumValue<MCaseTypes>();
            var dependentOnField = "_" +dependentField.Item2.GetPropertyNameFromSystemName().ToLower();
            var demandsValues = string.Join(",", mandatedByValue.Split(new[] { "~*~" }, StringSplitOptions.None).Where(x => !string.IsNullOrEmpty(x)).Select(x => $"{defaultValue}.{x.GetPropertyNameFromSystemName()}"));

            var valuesList = $"new List<{defaultValue}> () " + "{ " + demandsValues + "}";

            switch (type)
            {
                //case mCaseTypes.EmbeddedList: Processed after loop completion
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                    sb.AppendLine($"if({dependentOnField} != null && {dependentOnField}");
                    sb.AppendLine(3.Indent() + $".Intersect({valuesList})");
                    sb.AppendLine(3.Indent() + ".Any())");
                    sb.AppendLine(2.Indent() + "{");
                    sb.AppendLine(3.Indent() + $"// {propertyName} is a conditionally mandatory field. Dependent on: {dependentField.Item2}, when her values are any of the following: {demandsValues}");
                    sb.AppendLine(3.Indent() + CanSaveValidationHelper(mandatoryType, privateName, propertyName));
                    sb.AppendLine(2.Indent() + "}");
                    return sb.ToString();
                case MCaseTypes.DynamicDropDown:
                case MCaseTypes.CascadingDynamicDropDown:
                    return string.Empty; // todo ??
                case MCaseTypes.String:
                case MCaseTypes.LongString:
                case MCaseTypes.EmailAddress:
                case MCaseTypes.Phone:
                case MCaseTypes.URL:
                case MCaseTypes.Money:
                case MCaseTypes.Time:
                case MCaseTypes.Boolean:
                case MCaseTypes.ReadonlyField:
                case MCaseTypes.User:
                case MCaseTypes.Address:
                case MCaseTypes.Attachment:
                    var stringDemandsValues = string.Join(",", mandatedByValue.Split(new[] { "~*~" }, StringSplitOptions.None).Where(x => !string.IsNullOrEmpty(x)).Select(x => $"\"{x}\""));
                    var stringValuesList = "new List<string> () " + "{ " + stringDemandsValues + "}";

                    sb.AppendLine($"if(!string.IsNullOrEmpty({dependentOnField}) && {stringValuesList}.Contains({dependentOnField}, StringComparer.InvariantCultureIgnoreCase))");
                    sb.AppendLine(2.Indent() + "{");
                    sb.AppendLine(3.Indent() + $"// {propertyName} is a conditionally mandatory field. Dependent on: {dependentField.Item2}, when her values are any of the following: {demandsValues}");
                    sb.AppendLine(3.Indent() + CanSaveValidationHelper(mandatoryType, privateName, propertyName));
                    sb.AppendLine(2.Indent() + "}");
                    return sb.ToString();
                case MCaseTypes.Date:
                case MCaseTypes.DateTime:
                    return string.Empty;//TODO 
                case MCaseTypes.Number:
                    return string.Empty;//TODO
                case MCaseTypes.Section: //need in ce's?
                case MCaseTypes.Narrative: //need in ce's?
                case MCaseTypes.Header: //need in ce's?
                case MCaseTypes.UserRoleSecurityRestrict: //not required in CE's
                case MCaseTypes.DynamicCalculatedField: //not required in CE's
                case MCaseTypes.CalculatedField: // not required in CE's 
                case MCaseTypes.UniqueIdentifier: //not required in CE's
                case MCaseTypes.EmbeddedDocument: // blob?
                case MCaseTypes.HiddenField: //not required in CE's
                case MCaseTypes.LineBreak: //not required in CE's
                case MCaseTypes.Position0: //not required in CE's
                case MCaseTypes.Score1: //not required in CE's
                case MCaseTypes.Score2: //not required in CE's
                case MCaseTypes.Score3: //not required in CE's
                case MCaseTypes.Score4: //not required in CE's
                case MCaseTypes.Score5: //not required in CE's
                case MCaseTypes.Score6: //not required in CE's
                default:
                    return string.Empty;
            }
            return string.Empty;
        }

        public static StringBuilder GenerateEnums(List<string> fieldSet, string className, bool titleCase)
        {
            var sb = new StringBuilder();

            var distinct = fieldSet.Distinct().OrderBy(x => x).ToList();//order enums by name

            sb.AppendLine(BuildEnums(distinct, className).ToString());

            sb.AppendLine(BuildEnumMapper(distinct, className, titleCase).ToString());

            return sb;
        }

        private static StringBuilder BuildEnums(List<string> fieldSet, string className)
        {
            var sb = new StringBuilder();

            var distinctEnums = new List<string>();

            if (fieldSet.Count == 0)
                return sb;

            sb.Append(1.Indent() + $"public enum {className}Enum" + "{ Invalidselection,"); //open

            for (var i = 0; i < fieldSet.Count; i++)
            {
                var field = fieldSet[i];

                if (field.Contains("\""))
                {
                    field = field.Replace("\"", "\\\"");
                }

                var enumName = field.GetPropertyNameFromSystemName();

                if (distinctEnums.Contains(enumName))
                {
                    sb.Append(enumName + $"_{i}_,"); //generate enum duplicate
                }
                else
                {
                    sb.Append(enumName + ","); //generate enum 
                    distinctEnums.Add(enumName);
                }
            }

            sb.Append("}"); //close enum

            return sb;
        }

        public static StringBuilder BuildEnumMapper(List<string> fieldSet, string className, bool titleCase)
        {
            var sb = new StringBuilder();

            var distinctEnums = new List<string>();

            if (fieldSet.Count == 0)
                return sb;

            var staticClass = $"{className}Enum";

            sb.Append(1.Indent() + $"public static Dictionary<{staticClass}, string> {className}Map => new Dictionary<{staticClass}, string>()" + "{ {" + $"{staticClass}.Invalidselection, \"#~Invalid Selection~#\"" + "},"); //open

            for (var i = 0; i < fieldSet.Count; i++)
            {
                var field = fieldSet[i];

                if (field.Contains("\""))
                {
                    field = field.Replace("\"", "\\\"");
                }

                var value = titleCase ? $"\"{field.GetPropertyNameFromSystemName()}\"" : $"\"{field}\""; //get value

                var enumName = field.GetPropertyNameFromSystemName();

                if (distinctEnums.Contains(enumName))
                {
                    distinctEnums.Add(enumName);
                    enumName += $"_{i}_";
                }
                var key = enumName;

                sb.Append("{" + $"{staticClass}.{key}, {value}" + "},");
            }

            sb.Append("};"); //close dict

            return sb;
        }

        public static StringBuilder GenerateRelationships(JToken relationships)
        {
            var sb = new StringBuilder();

            if (relationships == null || !relationships.Any())
                return sb;

            var parentRelationships =
                relationships.ParseChildren(ListTransferFields.ParentSystemName.GetDescription());

            if (parentRelationships.Any())
            {
                var distinctParentRelationships = parentRelationships.Distinct();
                sb.AppendLine(GenerateEnums(distinctParentRelationships.ToList(), "ParentRelationships", false).ToString());
            }

            var childRelationships =
                relationships.ParseChildren(ListTransferFields.ChildSystemName.GetDescription());

            if (!childRelationships.Any()) return sb;

            var distinctChildRelationships = childRelationships.Distinct();
            sb.AppendLine(GenerateEnums(distinctChildRelationships.ToList(), "ChildRelationships", false).ToString());

            return sb;
        }
    }
}

//extract duplicate getters / setters to methods.