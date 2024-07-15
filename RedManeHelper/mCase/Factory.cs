using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        /// <summary>
        /// Opens class initializer for Datalist, as well as the region for fields. This does not close the class, or namespace.
        /// </summary>
        /// <param name="jObject"></param>
        /// <param name="className"></param>
        /// <param name="nameSpace"></param>
        /// <param name="mainUsings"></param>
        /// <returns></returns>
        public static StringBuilder ClassInitializer(JObject jObject, string sysName, string nameSpace, string mainUsings)
        {
            var sb = new StringBuilder();

            var id = jObject.ParseJson(ListTransferFields.Id.GetDescription());
            var className = jObject.ParseNameValue(ListTransferFields.Name.GetDescription());
            var dtNow = DateTime.Now.ToString(Extensions.MCaseDateTimeStorageFormat);

            sb.AppendLine(mainUsings);
            sb.AppendLine($"namespace {nameSpace}");
            sb.AppendLine("{"); //open namespace
            sb.AppendLine(0.Indent() + $"/// <summary> Synchronized data list [{id}][{className}] on {dtNow} </summary>");
            sb.AppendLine(0.Indent() + $"public class {sysName}");
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.AppendLine(1.Indent() + "public RecordInstanceData RecordInsData;");
            sb.AppendLine(1.Indent() + "private readonly AEventHelper _eventHelper;");
            sb.AppendLine(1.Indent() + "/// <summary> Class for Updating Existing RecordInstanceData. To create a new RecordInstance data, initialize with the appropriate datalist ID. </summary>");
            sb.AppendLine(1.Indent() +
                          $"public {sysName}(RecordInstanceData recordInsData, AEventHelper eventHelper)");
            sb.AppendLine(1.Indent() + "{"); //open constructor #Existing Record Instance Data
            sb.AppendLine(2.Indent() + "_eventHelper = eventHelper;");
            sb.AppendLine(2.Indent() + $"if (recordInsData.DataListID != DataListId) throw new Exception(\"RecordInstance is not of type {sysName}\");");
            sb.AppendLine(2.Indent() + "RecordInsData = recordInsData;");
            sb.AppendLine(2.Indent() + "_eventHelper.AddInfoLog($\"{SystemName} has been instantiated for recordInstance value: {recordInsData.RecordInstanceID}\");");
            sb.AppendLine(1.Indent() + "}"); //close constructor
            sb.AppendLine(1.Indent() + "#region Fields");
            sb.AppendLine(1.Indent() + "public string Label => RecordInsData.Label;");
            sb.AppendLine(1.Indent() + $"public string SystemName => \"{sysName}\";");
            sb.AppendLine(1.Indent() + "private int _dataListId = -1;");
            sb.AppendLine(1.Indent() + "/// <summary>");
            sb.AppendLine(1.Indent() + "/// Data list identifier is -1 if not found");
            sb.AppendLine(1.Indent() + "/// </summary>");
            sb.AppendLine(1.Indent() + "public int DataListId");
            sb.AppendLine(1.Indent() + "{"); //open Property
            sb.AppendLine(2.Indent() + "get");
            sb.AppendLine(2.Indent() + "{"); //open Getter
            sb.AppendLine(3.Indent() + "if(_dataListId > 0) return _dataListId;");
            sb.AppendLine(3.Indent() + "var id = _eventHelper.GetDataListID(SystemName);");
            sb.AppendLine(3.Indent() + "if(id.HasValue && id.Value > 0) _dataListId = id.Value;");
            sb.AppendLine(3.Indent() + "return _dataListId;");
            sb.AppendLine(2.Indent() + "}"); //close Getter
            sb.AppendLine(1.Indent() + "}"); //close Property

            return sb;
        }

        /// <summary>
        /// This opens the info class, and closes the namespace
        /// </summary>
        /// <param name="jObject"></param>
        /// <param name="className"></param>
        /// <param name="hasFields"></param>
        /// <returns></returns>
        public static StringBuilder BuildInfoClass(JObject jObject, string sysName, bool hasFields)
        {
            var sb = new StringBuilder();

            var id = jObject.ParseJson(ListTransferFields.Id.GetDescription());
            var className = jObject.ParseNameValue(ListTransferFields.Name.GetDescription());
            var dtNow = DateTime.Now.ToString(Extensions.MCaseDateTimeStorageFormat);

            #region Dl Info Class

            sb.AppendLine(0.Indent() + $"/// <summary>  Synchronized data list [{id}][{className}] on {dtNow} </summary>");
            sb.AppendLine(0.Indent() + $"public class {sysName}Info");
            sb.AppendLine(0.Indent() + "{"); //open class
            sb.AppendLine(1.Indent() + "private AEventHelper _eventHelper;");
            sb.AppendLine(1.Indent() + $"public {sysName}Info(AEventHelper eventHelper)");
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

                var sysNames = $"{sysName}Static.SystemNamesEnum";
                var sysNamesMap = $"{sysName}Static.SystemNamesMap";

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
                sb.AppendLine(1.Indent() + $"/// <returns>List of {sysName} objects that match with all filter queries. Query is 'and' operator not by 'or' operator</returns>");
                sb.AppendLine(1.Indent() + $"public List<{sysName}> CreateQuery(List<DirectSQLFieldFilterData> filters) => _eventHelper.SearchSingleDataListSQLProcess(DataListId, filters).Select(x => new {sysName}(x, _eventHelper)).ToList();");

                #endregion
            }

            sb.AppendLine(0.Indent() + "}"); //close class

            #endregion

            return sb;
        }

        #region Factories
        /// <summary>
        /// used specifically for generating c# string data types. (majority of mCase data structures can remain strings)
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="propertyName"></param>
        /// <param name="sysName"></param>
        /// <param name="type"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        public static StringBuilder StringFactory(JToken jToken, string propertyName, string sysName, string type, bool required)
        {
            var sb = new StringBuilder();
            var enumType = type.GetEnumValue<MCaseTypes>();

            var privateSysName = $"_{propertyName.ToLower()}";
            var mirroredField = jToken.IsMirrorField() || enumType == MCaseTypes.DynamicCalculatedField;

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

        /// <summary>
        /// used specifically for generating c# string data types, but mCase LongValues. Think geolocations?
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="propertyName"></param>
        /// <param name="sysName"></param>
        /// <param name="type"></param>
        /// <param name="required"></param>
        /// <returns></returns>
        public static StringBuilder LongStringFactory(string propertyName, string sysName, string type, bool required)
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
        /// <summary>
        /// used specifically for generating c# DateTime data types. Dates / DateTimes
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="propertyName"></param>
        /// <param name="sysName"></param>
        /// <param name="type"></param>
        /// <param name="required"></param>
        /// <returns></returns>
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

        /// <summary>
        /// used specifically for generating c# bool data types, but stored as strings. mCase does not have the classical true / false boolean. there are 5 known true / false values
        /// in mCase [0 / 1] [y / n] [Yes / No] [on / off] [true / false] and any one of these can be true or false. 
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="propertyName"></param>
        /// <param name="sysName"></param>
        /// <param name="type"></param>
        /// <param name="required"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Used to generate property who's data structure returns other recordInstances. In the case of this tool, other Facotry entities
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="sysName"></param>
        /// <param name="fieldType"></param>
        /// <param name="privateName"></param>
        /// <param name="multiSelect"></param>
        /// <param name="dynamics"></param>
        /// <param name="notAbleToSelectManyValues"></param>
        /// <param name="required"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates either a list of string(no default values), or if default values are provided
        /// then a list of constant default value enums are also generated.
        /// The result being the property is created, as well as a property default value list that controls entry values to list.
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="propertyName"></param>
        /// <param name="sysName"></param>
        /// <param name="fieldType"></param>
        /// <param name="privateName"></param>
        /// <param name="multiSelect"></param>
        /// <param name="notAbleToSelectManyValues"></param>
        /// <param name="className"></param>
        /// <param name="required"></param>
        /// <returns>StringBuilder 1 being the property, StringBuilder 2 being the enum values</returns>
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
                    //Item 1 = property, Item 2 = Enum
                    return new Tuple<StringBuilder, StringBuilder>(dynamicValues, new StringBuilder());

                default:
                    return new Tuple<StringBuilder, StringBuilder>(new StringBuilder(), new StringBuilder());
            }
        }

        #endregion
        #region Additional Factory Helpers

        /// <summary>
        /// Because string is so dynamic in mCase, not all strings are truly strings, some are numbers, some are bools, and each require their own validations
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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
                                  "var isNumeric = long.TryParse(value, out _) || int.TryParse(value, out _) || double.TryParse(value, out _) || BigInteger.TryParse(value, out _);");
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

        /// <summary>
        /// Generates the Drop down list property where no default values are specified (List of strings)
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="systemName"></param>
        /// <param name="privateName"></param>
        /// <param name="fieldType"></param>
        /// <param name="multiSelect"></param>
        /// <param name="required"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates the Drop down where the entered value is required to be one of the default value properties specified in configurations
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="sysName"></param>
        /// <param name="fieldType"></param>
        /// <param name="privateName"></param>
        /// <param name="multiSelect"></param>
        /// <param name="notAbleToSelectManyValues"></param>
        /// <param name="defaultValues"></param>
        /// <param name="staticName"></param>
        /// <param name="defaultOptionsName"></param>
        /// <param name="map"></param>
        /// <param name="required"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates the GetActive....Records() methods. Any child / embedded record will have their own call
        /// </summary>
        /// <param name="embedded"></param>
        /// <returns></returns>
        public static StringBuilder GetActiveRelatedRecords(HashSet<string> embedded)
        {
            var sb = new StringBuilder();

            if (embedded.Any())
                sb.AppendLine(1.Indent() + "#region Related Records");

            foreach (var value in embedded)
            {
                var propertyName = value.GetPropertyNameFromSystemName();

                sb.AppendLine(1.Indent() + $"/// <summary> Gets active related records of type: {propertyName}</summary>");
                sb.AppendLine(1.Indent() + $"/// <returns>Related records from {propertyName}</returns>");
                sb.AppendLine(1.Indent() +
                              $"public List<{propertyName}> GetActive{propertyName}Records() => _eventHelper.GetRelatedRecords(RecordInsData.RecordInstanceID, new {propertyName}Info(_eventHelper).SystemName).Select(x => new {propertyName}(x, _eventHelper)).ToList();"); // property name is added back with enum name appended 

                sb.AppendLine();// add extra space between methods...?

            }
            if (embedded.Any())
                sb.AppendLine(1.Indent() + "#endregion Related Records");

            return sb;
        }

        /// <summary>
        /// Largest method here generates connections to static files for add, remove, clear if method has enumerated values
        /// </summary>
        /// <param name="className"></param>
        /// <param name="addDefaults"></param>
        /// <returns></returns>
        public static StringBuilder AddEnumerableExtensions(string className, bool addDefaults)
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

            return sb;
        }

        public static StringBuilder AddConstantMethods(bool addDefaults, List<Tuple<string, string, bool, string, string>> requiredFields, HashSet<Tuple<string, string>> fields, string className)
        {
            var sb = new StringBuilder();
            var defaultValues = $"{className}Static.DefaultValuesEnum";

            // Conditionally Required Fields 
            sb.AppendLine(1.Indent() + "/// <summary>Checks all required fields in Datalist</summary>");
            sb.AppendLine(1.Indent() + "/// <returns>All required fields that have yet been filled in.</returns>");
            sb.AppendLine(1.Indent() + "public List<string> RequiredFieldsCheck()");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "var requiredFields = new List<string>();");

            for (var i = 0; i < requiredFields.Count; i++)
            {
                var required = requiredFields[i];
                var check = AddSaveRecordCheckForRequiredProperty(i, required, fields, defaultValues);

                if (!string.IsNullOrEmpty(check))
                    sb.AppendLine(2.Indent() + check);
            }
            sb.AppendLine(2.Indent() + "if(requiredFields.Count > 0) _eventHelper.AddWarningLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: Required fields check returned {requiredFields.Count} mandatory field(s) unfilled\");");
            sb.AppendLine(2.Indent() + "return requiredFields;");
            sb.AppendLine(1.Indent() + "}");//close method

            // Try Save record
            sb.AppendLine(1.Indent() + "/// <summary> Attempts to pass all required fields prior to saving recordInstanceData</summary>");
            sb.AppendLine(1.Indent() + "/// <returns>A list of all unfilled required fields. Empty list means successfully saved</returns>");
            sb.AppendLine(1.Indent() + "public List<string> TrySaveRecord()");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "var requiredFields = RequiredFieldsCheck();");
            sb.AppendLine(2.Indent() + "if(requiredFields.Count > 0) return requiredFields;");
            sb.AppendLine(2.Indent() + "var status = SaveRecord(false);");
            sb.AppendLine(2.Indent() + "if (status == EventStatusCode.Success) return new List<string>();");
            sb.AppendLine(2.Indent() + "return new List<string>(){ \"Error in saving, check error log for exception\"};");
            sb.AppendLine(1.Indent() + "}");//close method

            // Save record
            sb.AppendLine(1.Indent() + "/// <summary> Save RecordInstanceData without requirement checks</summary>");
            sb.AppendLine(1.Indent() + "/// <returns>EventStatusCode.Success, or Failure if exception thrown. If Exception thrown, it will be logged to error log</returns>");
            sb.AppendLine(1.Indent() + "public EventStatusCode SaveRecord() => SaveRecord(false);");

            // Delete record
            sb.AppendLine(1.Indent() + "/// <summary> Attempts to update status field on record instance to soft deleted</summary>");
            sb.AppendLine(1.Indent() + "/// <returns>EventStatusCode.Success, or Failure if exception thrown. If Exception thrown, it will be logged to error log</returns>");
            sb.AppendLine(1.Indent() + "public EventStatusCode SoftDelete()");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "RecordInsData.Status = MCaseEventConstants.RecordStatusDeleted;");
            sb.AppendLine(2.Indent() + "RecordInsData.FrozenInd = true;");
            sb.AppendLine(2.Indent() + "return SaveRecord(true);");
            sb.AppendLine(1.Indent() + "}");//close method

            //logging
            sb.AppendLine(1.Indent() + "public void LogDebug(string log) => _eventHelper.AddDebugLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: {log}\");");
            sb.AppendLine(1.Indent() + "public void LogInfo(string log) => _eventHelper.AddInfoLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: {log}\");");
            sb.AppendLine(1.Indent() + "public void LogWarning(string log) => _eventHelper.AddWarningLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: {log}\");");
            sb.AppendLine(1.Indent() + "public void LogError(string log) => _eventHelper.AddErrorLog($\"[{SystemName}][{RecordInsData.RecordInstanceID}]: {log}\");");

            sb.AppendLine(1.Indent() + "#region Private");

            // private save record
            sb.AppendLine(1.Indent() + "/// <summary> Save RecordInstanceData without requirement checks</summary>");
            sb.AppendLine(1.Indent() + "/// <returns>EventStatusCode</returns>");
            sb.AppendLine(1.Indent() + "private EventStatusCode SaveRecord(bool delete)");
            sb.AppendLine(1.Indent() + "{");//open method
            sb.AppendLine(2.Indent() + "var crud = delete ? \"Deleted\" : \"Saved\";");
            sb.AppendLine(2.Indent() + "try");
            sb.AppendLine(2.Indent() + "{");//open try
            sb.AppendLine(3.Indent() + "_eventHelper.SaveRecord(RecordInsData);");
            sb.AppendLine(3.Indent() + "_eventHelper.AddInfoLog($\"[Success]-[{SystemName}] {crud} Record: {RecordInsData.RecordInstanceID}\");");
            sb.AppendLine(3.Indent() + "return EventStatusCode.Success;");
            sb.AppendLine(2.Indent() + "}");//close try
            sb.AppendLine(2.Indent() + "catch (Exception ex)");
            sb.AppendLine(2.Indent() + "{");//open catch
            sb.AppendLine(3.Indent() + "_eventHelper.AddErrorLog($\"[Failed]-[{SystemName}] {crud} Record: {RecordInsData.RecordInstanceID}\\n=============================================\\n{ex}\\n=============================================\\n\");");
            sb.AppendLine(3.Indent() + "return EventStatusCode.Failure;");
            sb.AppendLine(2.Indent() + "}");//close catch
            sb.AppendLine(1.Indent() + "}");//close method

            if (addDefaults)
            {
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
            }

            sb.AppendLine(1.Indent() + "#endregion Private");

            return sb;
        }

        /// <summary>
        /// [0] field type [1] system name [2] conditionally mandatory [3] Mandated by field [4] mandated by value
        /// </summary>
        /// <param name="currentIter"></param>
        /// <param name="required"></param>
        /// <param name="fields"></param>
        /// <param name="defaultEnum"></param>
        /// <returns></returns>
        private static string AddSaveRecordCheckForRequiredProperty(int currentIter, Tuple<string, string, bool, string, string> required, HashSet<Tuple<string, string>> fields, string defaultEnum)
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
                    if (string.Equals("Mandated If Field Has Value", mandatedByValue, StringComparison.OrdinalIgnoreCase))
                    {
                        var notEmptyCheck = AddNotEmptyConditionalCheck(dependentField, privateName, propertyName, type);

                        return notEmptyCheck;
                    }

                    //field is the conditional field
                    var conditionalResult = AddConditionallyMandatoryCheck(currentIter, dependentField, mandatedByValue, privateName, propertyName, type, defaultEnum);

                    return conditionalResult;
                }
                //what do we do here?
                Console.WriteLine();

            }

            return CanSaveValidationHelper(type, privateName, propertyName);
        }

        private static string AddNotEmptyConditionalCheck(Tuple<string, string> dependentField, string privateName, string propertyName, MCaseTypes type)
        {
            var sb = new StringBuilder();

            var dependentType = dependentField.Item1.GetEnumValue<MCaseTypes>();
            var dependentValue = "_" + dependentField.Item2.GetPropertyNameFromSystemName().ToLower();
            var helperComment =
                $"// {propertyName} is a conditionally mandatory field. Dependent on: {dependentField.Item2}, when her values are not empty.";

            switch (dependentType)
            {
                //case mCaseTypes.EmbeddedList: Processed after loop completion
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                case MCaseTypes.DynamicDropDown:
                case MCaseTypes.CascadingDynamicDropDown:
                    sb.AppendLine($"if({dependentValue} != null && {dependentValue}.Count > 0) ");
                    sb.AppendLine(2.Indent() + "{");
                    sb.AppendLine(3.Indent() + helperComment);
                    sb.AppendLine(3.Indent() + CanSaveValidationHelper(type, privateName, propertyName));
                    sb.AppendLine(2.Indent() + "}");
                    break;
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
                    sb.AppendLine($"if(string.IsNullOrEmpty({dependentValue}))");
                    sb.AppendLine(2.Indent() + "{");
                    sb.AppendLine(3.Indent() + helperComment);
                    sb.AppendLine(3.Indent() + CanSaveValidationHelper(type, privateName, propertyName));
                    sb.AppendLine(2.Indent() + "}");
                    return sb.ToString();
                case MCaseTypes.Date:
                case MCaseTypes.DateTime:
                    sb.AppendLine($"if({dependentValue} == null)");
                    sb.AppendLine(2.Indent() + "{");
                    sb.AppendLine(3.Indent() + helperComment);
                    sb.AppendLine(3.Indent() + CanSaveValidationHelper(type, privateName, propertyName));
                    sb.AppendLine(2.Indent() + "}");
                    return sb.ToString();
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

            return sb.ToString();
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

        private static string AddConditionallyMandatoryCheck(int currentIteration, Tuple<string, string> dependentField, string mandatedByValue, string privateName, string propertyName, MCaseTypes mandatoryType, string defaultValue)

        {
            var sb = new StringBuilder();
            var type = dependentField.Item1.GetEnumValue<MCaseTypes>();
            var dependentOnField = "_" + dependentField.Item2.GetPropertyNameFromSystemName().ToLower();
            var demandsValues = string.Join(",", mandatedByValue.Split(new[] { "~*~" }, StringSplitOptions.None).Where(x => !string.IsNullOrEmpty(x)).Select(x => $"{defaultValue}.{x.GetPropertyNameFromSystemName()}"));
            var helperComment =
                $"// {propertyName} is a conditionally mandatory field. Dependent on: {dependentField.Item2}, when her values are any of the following: {demandsValues}";
            switch (type)
            {
                //case mCaseTypes.EmbeddedList: Processed after loop completion
                case MCaseTypes.CascadingDropDown:
                case MCaseTypes.DropDownList:
                    var valuesList = $"new List<{defaultValue}> () " + "{ " + demandsValues + "}";

                    sb.AppendLine($"if({dependentOnField} != null && {dependentOnField}");
                    sb.AppendLine(3.Indent() + $".Intersect({valuesList})");
                    sb.AppendLine(3.Indent() + ".Any())");
                    sb.AppendLine(2.Indent() + "{");
                    sb.AppendLine(3.Indent() + helperComment);
                    sb.AppendLine(3.Indent() + CanSaveValidationHelper(mandatoryType, privateName, propertyName));
                    sb.AppendLine(2.Indent() + "}");
                    return sb.ToString();
                case MCaseTypes.DynamicDropDown:
                case MCaseTypes.CascadingDynamicDropDown:
                    sb.AppendLine($"if({dependentOnField} != null && {dependentOnField}");
                    sb.AppendLine(3.Indent() + ".Any())");
                    sb.AppendLine(2.Indent() + "{");
                    sb.AppendLine(3.Indent() + helperComment);
                    sb.AppendLine(3.Indent() + CanSaveValidationHelper(mandatoryType, privateName, propertyName));
                    sb.AppendLine(2.Indent() + "}");
                    return sb.ToString();
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
                    var stringDemandsValues = string.Join(",", mandatedByValue
                        .Split(new[] { "~*~" }, StringSplitOptions.None)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => $"\"{x}\""));

                    helperComment =
                        $"// {propertyName} is a conditionally mandatory field. Dependent on: {dependentField.Item2}, when her values are any of the following: {stringDemandsValues}";

                    var stringValuesList = "new List<string> () " + "{ " + stringDemandsValues + "}";

                    sb.AppendLine($"if(!string.IsNullOrEmpty({dependentOnField}) && {stringValuesList}.Contains({dependentOnField}, StringComparer.OrdinalIgnoreCase))");
                    sb.AppendLine(2.Indent() + "{");
                    sb.AppendLine(3.Indent() + helperComment);
                    sb.AppendLine(3.Indent() + CanSaveValidationHelper(mandatoryType, privateName, propertyName));
                    sb.AppendLine(2.Indent() + "}");
                    return sb.ToString();
                case MCaseTypes.Number:

                    var values = mandatedByValue.Split(new[] { "~*~" }, StringSplitOptions.None);
                    helperComment =
                        $"// {propertyName} is a conditionally mandatory field. Dependent on: {dependentField.Item2}, when her value is greater than {values.First()} and less than {values.Last()}";
                    var localLow = $"lowValue_{currentIteration}";
                    var localHigh = $"highValue_{currentIteration}";
                    var currentVal = $"dependentOnField_{currentIteration}";

                    sb.AppendLine($"if (BigInteger.TryParse(\"{values.First()}\", out var {localLow}) && BigInteger.TryParse(\"{values.Last()}\", out var {localHigh}) && BigInteger.TryParse({dependentOnField}, out var {currentVal}))");
                    sb.AppendLine(2.Indent() + "{");//open if
                    sb.AppendLine(3.Indent() + $"if ({localLow} <= {currentVal} && {currentVal} <= {localHigh})");
                    sb.AppendLine(3.Indent() + "{");//open embedded if
                    sb.AppendLine(4.Indent() + helperComment);
                    sb.AppendLine(4.Indent() + CanSaveValidationHelper(mandatoryType, privateName, propertyName));
                    sb.AppendLine(3.Indent() + "}");//close embedded if
                    sb.AppendLine(2.Indent() + "}");//close if

                    return sb.ToString();
                case MCaseTypes.Date:
                case MCaseTypes.DateTime:
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

        private static StringBuilder BuildEnumMapper(List<string> fieldSet, string className, bool titleCase)
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

        /// <summary>
        /// Nullable types are not supported in upper solutions. Jtoken still requires nullable checks, although the argument itself is not nullable
        /// </summary>
        /// <param name="relationships"></param>
        /// <returns></returns>
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
                sb.Append(GenerateEnums(distinctParentRelationships.ToList(), "ParentRelationships", false));
            }

            var childRelationships =
                relationships.ParseChildren(ListTransferFields.ChildSystemName.GetDescription());

            if (!childRelationships.Any()) return sb;

            var distinctChildRelationships = childRelationships.Distinct();
            sb.Append(GenerateEnums(distinctChildRelationships.ToList(), "ChildRelationships", false));

            return sb;
        }

        #endregion
        #region Static File Extensions

        /// <summary>
        /// This file holds internal logic for using reflection to update internal states to class properties [comnplex as fuuq] this level of programming is difficult using an
        /// IDE, i would recommend to update, validate, and check that code works before updating the meta code here in string. (10x more difficult here)
        /// </summary>
        /// <param name="namespace_"></param>
        /// <param name="staticUsings"></param>
        /// <returns></returns>
        public static string GenerateStaticFile(string nameSpace, string staticUsings)
        {
            var sb = new StringBuilder();

            sb.AppendLine( //TODO continue to add usings, as more and more validations are made
                staticUsings);
            sb.AppendLine($"namespace {nameSpace}"); //TODO: project specific namespace
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

        /// <summary>
        /// Adds boolean static conversion method
        /// </summary>
        /// <returns></returns>
        private static StringBuilder AddBooleanMethod()
        {
            var sb = new StringBuilder();

            sb.AppendLine(1.Indent() + "/// <summary> If string is not null or empty, this method returns an accurate boolean from a string based off of MCaseEventConstants.TrueValues and MCaseEventConstants.FalseValues</summary>"); //static class
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

        /// <summary>
        /// Currently only set to retrieving the values for embedded items, and child records
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Adds clear method to static file
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// There are three different add methods. Add to List of string, List of RecordInstanceData, List of Default Values,
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// There are three different add Range methods. Add to List of string, List of RecordInstanceData, List of Default Values,
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// There are three different Remove methods. This uses a predicate for each type, to remove any item that meets the predicate req.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// This builds out the enum value class as well as the dictionary mapping class to said enum
        /// </summary>
        /// <returns></returns>
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
    }
}